using SatisfactorySaveNet.Abstracts.Model;
using SatisfactorySaveNet.Abstracts.Model.Properties;
using System.Linq;
using System.Text.Json;

namespace Denxorz.Satisfactory.Routes;

public class DroneStationParser(List<ComponentObject> objects, Dictionary<string, ComponentObject> objectsByName)
{
    public IEnumerable<Station> Parse()
    {
        // Drone parts By TypePath
        var droneRelatedObjects = objects
            .Where(o => o.TypePath.Contains("drone", StringComparison.InvariantCultureIgnoreCase))
            .ToList();

        var droneRelatedObjectsByType = droneRelatedObjects
            .GroupBy(o => o.TypePath)
            .ToDictionary(o => o.Key, o => o.ToList());

        // Drone Station Identifier, by StationId. I.e. Persistent_Level:PersistentLevel.Build_DroneStation_C_2144148257
        var droneStationIdentifiers = droneRelatedObjectsByType["/Script/FactoryGame.FGDroneStationInfo"];
        var droneStationIdentifiersByStationId = droneStationIdentifiers
            .Select(t => new
            {
                Id = t.ObjectReference.PathName,
                Name = (t.Properties.FirstOrDefault(p => p.Name == "mBuildingTag") as StrProperty)?.Value ?? "No custom name",
                DroneStationId = (t.Properties.FirstOrDefault(p => p.Name == "mStation") as ObjectProperty)?.Value.PathName ?? "??",
                PairedStationId = (t.Properties.FirstOrDefault(p => p.Name == "mPairedStation") as ObjectProperty)?.Value.PathName ?? "??", //Persistent_Level:PersistentLevel.FGDroneStationInfo_2147135058
            })
            .ToDictionary(t => t.DroneStationId, t => t);

        var droneStationIdentifiersByStationIdentifier = droneStationIdentifiersByStationId.Values.ToDictionary(t => t.Id, t => t);

        // Drone Station, by StationId. I.e. Persistent_Level:PersistentLevel.Build_DroneStation_C_2144148257
        var droneStations = droneRelatedObjectsByType["/Game/FactoryGame/Buildable/Factory/DroneStation/Build_DroneStation.Build_DroneStation_C"];
        return droneStations
            .OfType<ActorObject>()
            .Select(t =>
            {
                var id = t.ObjectReference.PathName;
                var stationIdentifier = droneStationIdentifiersByStationId[id];
                var drone = (t.Properties.FirstOrDefault(p => p.Name == "mStationDrone") as ObjectProperty)?.Value.PathName ?? "??";
                var inputInventory = objectsByName[(t.Properties.FirstOrDefault(p => p.Name == "mInputInventory") as ObjectProperty)?.Value.PathName ?? "??"];
                var outputInventory = objectsByName[(t.Properties.FirstOrDefault(p => p.Name == "mOutputInventory") as ObjectProperty)?.Value.PathName ?? "??"];
                var inputCargoTypes = inputInventory.ToCargoTypes();
                var outputCargoTypes = outputInventory.ToCargoTypes();
                var isUnload = inputCargoTypes.Count <= outputCargoTypes.Count;
                List<string> cargoTypes = [.. inputCargoTypes, .. outputCargoTypes];
                var cargo = stationIdentifier.Name.GetFlowPerMinuteFromName(cargoTypes);
                var shortId = id.Split("_")[^1];

                var pairedStationIdentifier = droneStationIdentifiersByStationIdentifier.TryGetValue(stationIdentifier.PairedStationId, out var tmp1) ? tmp1 : null;

                return new Station(
                    shortId,
                    stationIdentifier.Name.ToIdOnlyName(),
                    stationIdentifier.Name,
                    "drone",
                    cargoTypes,
                    cargo,
                    isUnload,
                    pairedStationIdentifier is not null ? [
                        new Transporter(
                            drone.Split("_")[^1],
                            stationIdentifier.Name,
                            pairedStationIdentifier.DroneStationId.Split("_")[^1], 
                            shortId,
                            [])] : [],
                    t.Position.X,
                    t.Position.Y
                );
            });
    }
}

