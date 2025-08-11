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

        var points = truckRelatedObjectsByType["/Game/FactoryGame/Buildable/Vehicle/BP_VehicleTargetPoint.BP_VehicleTargetPoint_C"];
        var pointsWithMoreThan3Props = points.Where(p => p.Properties.Count > 3).ToList();

        // Truck Station Identifier, by StationId. I.e. Persistent_Level:PersistentLevel.Build_TruckStation_C_2144148257
        var truckStationIdentifiers = truckRelatedObjectsByType["/Script/FactoryGame.FGDockingStationInfo"];
        var truckStationIdentifiersByStationId = truckStationIdentifiers
            .Select(t => new
            {
                Id = t.ObjectReference.PathName,
                Name = (t.Properties.FirstOrDefault(p => p.Name == "mBuildingTag") as StrProperty)?.Value ?? "No custom name",
                TruckStationId = (t.Properties.FirstOrDefault(p => p.Name == "mStation") as ObjectProperty)?.Value.PathName ?? "??",
                PairedStationId = (t.Properties.FirstOrDefault(p => p.Name == "mPairedStation") as ObjectProperty)?.Value.PathName ?? "??", //Persistent_Level:PersistentLevel.FGTruckStationInfo_2147135058
            })
            .ToDictionary(t => t.TruckStationId, t => t);

        // Truck Station, by StationId. I.e. Persistent_Level:PersistentLevel.Build_TruckStation_C_2144148257
        var truckStations = truckRelatedObjectsByType["/Game/FactoryGame/Buildable/Factory/TruckStation/Build_TruckStation.Build_TruckStation_C"];
        return truckStations
            .OfType<ActorObject>()
            .Select(t =>
            {
                var id = t.ObjectReference.PathName;
                var stationIdentifier = truckStationIdentifiersByStationId[id];
                var inventory = objectsByName[(t.Properties.FirstOrDefault(p => p.Name == "mInventory") as ObjectProperty)?.Value.PathName ?? "??"];
                var cargoTypes = inventory.ToCargoTypes();
                var output0 = objectsByName[t.Components.First(c => c.PathName.Contains("output0", StringComparison.InvariantCultureIgnoreCase)).PathName];
                var output1 = objectsByName[t.Components.First(c => c.PathName.Contains("output1", StringComparison.InvariantCultureIgnoreCase)).PathName];
                var isUnload = output0.Properties.Count > 0 || output1.Properties.Count > 0;
                var cargo = stationIdentifier.Name.GetFlowPerMinuteFromName(cargoTypes);

                return new Station(
                    id.Split("_")[^1],
                    stationIdentifier.Name.ToIdOnlyName(),
                    stationIdentifier.Name,
                    "truck",
                    cargoTypes,
                    cargo,
                    isUnload,
                    [],
                    t.Position.X,
                    t.Position.Y
                );
            });
    }
}

