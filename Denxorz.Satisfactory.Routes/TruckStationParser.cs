using System.Text.Json;
using SatisfactorySaveNet.Abstracts.Model;
using SatisfactorySaveNet.Abstracts.Model.Properties;

namespace Denxorz.Satisfactory.Routes;

public class TruckStationParser(List<ComponentObject> objects, Dictionary<string, ComponentObject> objectsByName)
{
    public IEnumerable<Station> Parse()
    {
        // Truck parts By TypePath
        var truckRelatedObjects = objects
            .Where(o => o.TypePath.Contains("truck", StringComparison.InvariantCultureIgnoreCase)
            || o.TypePath.Contains("vehicle", StringComparison.InvariantCultureIgnoreCase)
            || o.TypePath.Contains("docking", StringComparison.InvariantCultureIgnoreCase)
            || o.TypePath.Contains("driving", StringComparison.InvariantCultureIgnoreCase)
            )
            .ToList();

        var truckRelatedObjectsByType = truckRelatedObjects
            .GroupBy(o => o.TypePath)
            .ToDictionary(o => o.Key, o => o.ToList());

        var vehiclesByTargetListId = truckRelatedObjectsByType["/Script/FactoryGame.FGWheeledVehicleInfo"]
            .OfType<ActorObject>()
            .Where(t => !string.IsNullOrWhiteSpace(t.ParentObjectName))
            .Select(t => new
            {
                VehicleInfo = t,
                Vehicle = objectsByName[t.ParentObjectName]
            })
            .Where(v => v.Vehicle.Properties.Any(p => p.Name == "mTargetList"))
            .Select(t => new
            {
                t.VehicleInfo,
                t.Vehicle,
                TargetListId = (t.Vehicle.Properties.First(p => p.Name == "mTargetList") as ObjectProperty)!.Value.PathName
            })
            .Where(t => !string.IsNullOrWhiteSpace(t.TargetListId))
            .GroupBy(t => t.TargetListId)
            .ToDictionary(t => t.Key, t => t.ToList());

        var simpleTruckStations = truckRelatedObjectsByType["/Game/FactoryGame/Buildable/Factory/TruckStation/Build_TruckStation.Build_TruckStation_C"]
            .OfType<ActorObject>()
            .Select(t =>
            {
                var output0 = objectsByName[t.Components.First(c => c.PathName.Contains("output0", StringComparison.InvariantCultureIgnoreCase)).PathName];
                var output1 = objectsByName[t.Components.First(c => c.PathName.Contains("output1", StringComparison.InvariantCultureIgnoreCase)).PathName];
                var isUnload = output0.Properties.Count > 0 || output1.Properties.Count > 0;

                return new
                {
                    Id = t.ObjectReference.PathName,
                    Raw = t,
                    t.Position,
                    Output0 = output0,
                    Output1 = output1,
                    IsUnload = isUnload,
                };
            })
            .ToList();

        var simpleTruckStationsByStationId = simpleTruckStations
            .ToDictionary(t => t.Id, t => t);

        var stationIdsByPosition = simpleTruckStations
            .ToDictionary(s => (s.Position.X, s.Position.Y), s => s.Id);

        var targetListIdByStationId = truckRelatedObjectsByType["/Game/FactoryGame/Buildable/Vehicle/BP_VehicleTargetPoint.BP_VehicleTargetPoint_C"]
            .Where(p => p.Properties.Any(pp => pp.Name == "mWaitTime"))
            .OfType<ActorObject>()
            .Select(p => new { StationId = stationIdsByPosition.TryGetValue((p.Position.X, p.Position.Y), out var tmp) ? tmp : null, p.ParentObjectName })
            .Where(p => p.StationId is not null)
            .GroupBy(t => t.StationId!)
            .ToDictionary(p => p.Key, p => p.First().ParentObjectName);

        var unloadStationIdByTargetListId = targetListIdByStationId
            .Select(t => new { TargetListId = t.Value, StationId = t.Key, IsUnload = (simpleTruckStationsByStationId.TryGetValue(t.Key, out var tmp) ? tmp.IsUnload : (bool?)null) })
            .Where(t => t.IsUnload == true)
            .GroupBy(t => t.TargetListId)
            .ToDictionary(t => t.Key, t => t.First().StationId);

        // Truck Station Identifier, by StationId. I.e. Persistent_Level:PersistentLevel.Build_TruckStation_C_2144148257
        var truckStationIdentifiers = truckRelatedObjectsByType["/Script/FactoryGame.FGDockingStationInfo"];
        var truckStationIdentifiersByStationId = truckStationIdentifiers
            .Select(t => new
            {
                Id = t.ObjectReference.PathName,
                Name = (t.Properties.FirstOrDefault(p => p.Name == "mBuildingTag") as StrProperty)?.Value ?? "No custom name",
                TruckStationId = (t.Properties.FirstOrDefault(p => p.Name == "mStation") as ObjectProperty)?.Value.PathName ?? "??",
            })
            .ToDictionary(t => t.TruckStationId, t => t);

        // Truck Station, by StationId. I.e. Persistent_Level:PersistentLevel.Build_TruckStation_C_2144148257
        return [.. simpleTruckStations
            .Select(t =>
            {
                var shortId = t.Id.Split("_")[^1];
                var stationIdentifier = truckStationIdentifiersByStationId[t.Id];
                var inventory = objectsByName[(t.Raw.Properties.FirstOrDefault(p => p.Name == "mInventory") as ObjectProperty)?.Value.PathName ?? "??"];
                var cargoTypes = inventory.ToCargoTypes();
                var cargo = stationIdentifier.Name.GetFlowPerMinuteFromName(cargoTypes);
                var targetListId = targetListIdByStationId[t.Id];

                var vehicles = t.IsUnload ? [] : vehiclesByTargetListId[targetListId]
                    .Select(v => new Transporter(
                        v.Vehicle.ObjectReference.PathName.Split("_")[^1],
                        "??",
                        shortId,
                        unloadStationIdByTargetListId.TryGetValue(targetListId, out var tmp) ? tmp.Split("_")[^1] : "??"))
                    .ToList();

                return new Station(
                    shortId,
                    stationIdentifier.Name.ToIdOnlyName(),
                    stationIdentifier.Name,
                    "truck",
                    cargoTypes,
                    cargo,
                    t.IsUnload,
                    vehicles,
                    t.Position.X,
                    t.Position.Y
                );
            })];
    }
}

