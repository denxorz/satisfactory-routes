using Denxorz.Satisfactory.Routes.Types;
using SatisfactorySaveNet.Abstracts.Model;

namespace Denxorz.Satisfactory.Routes.Parsers;

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

        var vehiclesByTargetListId = truckRelatedObjectsByType.GetGroup("/Script/FactoryGame.FGWheeledVehicleInfo")
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
                TargetListId = t.Vehicle.Properties.GetObjectPathName( "mTargetList")
            })
            .Where(t => !string.IsNullOrWhiteSpace(t.TargetListId))
            .GroupBy(t => t.TargetListId)
            .ToDictionary(t => t.Key, t => t.ToList());

        var simpleTruckStations = truckRelatedObjectsByType.GetGroup("/Game/FactoryGame/Buildable/Factory/TruckStation/Build_TruckStation.Build_TruckStation_C")
            .OfType<ActorObject>()
            .Select(t =>
            {
                var output0 = objectsByName[t.Components.First(c => c.PathName.Contains("output0", StringComparison.InvariantCultureIgnoreCase)).PathName];
                var output1 = objectsByName[t.Components.First(c => c.PathName.Contains("output1", StringComparison.InvariantCultureIgnoreCase)).PathName];
                var isUnload = output0.Properties.Count > 0 || output1.Properties.Count > 0;

                return new
                {
                    t.ObjectReference.PathName,
                    Raw = t,
                    t.Position,
                    Output0 = output0,
                    Output1 = output1,
                    IsUnload = isUnload,
                };
            })
            .ToList();

        var simpleTruckStationsByStationPathName = simpleTruckStations
            .ToDictionary(t => t.PathName, t => t);

        var stationIdsByPosition = simpleTruckStations
            .ToDictionary(s => (s.Position.X, s.Position.Y), s => s.PathName);

        var targetListIdByStationPathName = truckRelatedObjectsByType.GetGroup("/Game/FactoryGame/Buildable/Vehicle/BP_VehicleTargetPoint.BP_VehicleTargetPoint_C")
            .Where(p => p.Properties.Any(pp => pp.Name == "mWaitTime"))
            .OfType<ActorObject>()
            .Select(p => new { StationId = stationIdsByPosition.TryGetValue((p.Position.X, p.Position.Y), out var tmp) ? tmp : null, p.ParentObjectName })
            .Where(p => p.StationId is not null)
            .GroupBy(t => t.StationId!)
            .ToDictionary(p => p.Key, p => p.First().ParentObjectName);

        var stationIdsByTargetListId = targetListIdByStationPathName
            .Select(t => new { TargetListId = t.Value, StationId = t.Key })
            .GroupBy(t => t.TargetListId)
            .ToDictionary(t => t.Key, t => t.Select(t => t.StationId).ToList());

        var unloadStationIdByTargetListId = targetListIdByStationPathName
            .Select(t => new { TargetListId = t.Value, StationId = t.Key, IsUnload = simpleTruckStationsByStationPathName.TryGetValue(t.Key, out var tmp) ? tmp.IsUnload : (bool?)null })
            .Where(t => t.IsUnload == true)
            .GroupBy(t => t.TargetListId)
            .ToDictionary(t => t.Key, t => t.First().StationId);

        // Truck Station Identifier, by StationId. I.e. Persistent_Level:PersistentLevel.Build_TruckStation_C_2144148257
        var truckStationIdentifiers = truckRelatedObjectsByType.GetGroup("/Script/FactoryGame.FGDockingStationInfo");
        var truckStationIdentifiersByStationPathName = truckStationIdentifiers
            .Select(t => new
            {
                Id = t.ObjectReference.PathName,
                Name = t.Properties.GetString("mBuildingTag") ?? "No custom name",
                TruckStationId = t.Properties.GetObjectPathName("mStation"),
            })
            .ToDictionary(t => t.TruckStationId, t => t);

        // Truck Station, by StationId. I.e. Persistent_Level:PersistentLevel.Build_TruckStation_C_2144148257
        return [.. simpleTruckStations
            .Select(t =>
            {
                var id = t.PathName.ToId();
                var stationIdentifier = truckStationIdentifiersByStationPathName[t.PathName];
                var cargoTypes = t.Raw.Properties.GetObject(objectsByName, "mInventory").ToCargoTypes();
                var cargo = stationIdentifier.Name.GetFlowPerMinuteFromName(cargoTypes);

                var vehicles = new List<Transporter>();

                if (targetListIdByStationPathName.TryGetValue(t.PathName, out var targetListId))
                {
                    var unloadStationId = unloadStationIdByTargetListId.TryGetValue(targetListId, out var tmp) ? tmp : "??";
                    var otherStations = stationIdsByTargetListId[targetListId].Where(s => s != t.PathName && s != unloadStationId).Select(s => s.ToId()).ToList();

                    vehicles = t.IsUnload
                    ? []
                    : [.. vehiclesByTargetListId[targetListId]
                        .Select(v => new Transporter(
                            v.Vehicle.ObjectReference.PathName.ToId(),
                            "Truck",
                            id,
                            unloadStationId.ToId(),
                            otherStations))];
                }

                return new Station(
                    id,
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

