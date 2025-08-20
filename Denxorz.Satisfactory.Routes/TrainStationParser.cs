using System.Linq;
using SatisfactorySaveNet.Abstracts.Model;
using SatisfactorySaveNet.Abstracts.Model.Properties;

namespace Denxorz.Satisfactory.Routes;

public class TrainStationParser(List<ComponentObject> objects, Dictionary<string, ComponentObject> objectsByName)
{
    public IEnumerable<Station> Parse()
    {
        // Train parts By TypePath
        var trainRelatedObjects = objects
            .Where(o => o.TypePath.Contains("train", StringComparison.InvariantCultureIgnoreCase) || o.TypePath.Contains("rail", StringComparison.InvariantCultureIgnoreCase))
            .ToList();

        var trainRelatedObjectsByType = trainRelatedObjects
            .GroupBy(o => o.TypePath)
            .ToDictionary(o => o.Key, o => o.ToList());

        // Train Timetable, by PathName. I.e. Persistent_Level:PersistentLevel.FGRailroadTimeTable_2146071228
        var trainTimeTables = trainRelatedObjectsByType["/Script/FactoryGame.FGRailroadTimeTable"];
        var trainTimeTablesWithStops = trainTimeTables
            .Select(t => new
            {
                Id = t.ObjectReference.PathName,
                StopStationIds = t.Properties.FirstOrDefault(p => p.Name == "mStops").ToStops()
            })
            .ToList();

        var trainNameByTimeTableId = trainRelatedObjectsByType["/Game/FactoryGame/Buildable/Vehicle/Train/-Shared/BP_Train.BP_Train_C"]
            .ToDictionary(
                t => t.Properties.FirstOrDefault(p => p.Name == "TimeTable") is ObjectProperty op ? op.Value.PathName : "??", 
                t => t.Properties.FirstOrDefault(p => p.Name == "mTrainName") is TextProperty tp ? tp.Value : null);

        // Train Station Identifier, by StationId. I.e. Persistent_Level:PersistentLevel.Build_TrainStation_C_2147007670
        var trainStationIdentifiers = trainRelatedObjectsByType["/Script/FactoryGame.FGTrainStationIdentifier"];
        var trainStationIdentifiersByStationId = trainStationIdentifiers
            .Select(t => new
            {
                Id = t.ObjectReference.PathName,
                Name = (t.Properties.FirstOrDefault(p => p.Name == "mStationName") as TextProperty)?.Value ?? "No custom name",
                TrainStationId = (t.Properties.FirstOrDefault(p => p.Name == "mStation") as ObjectProperty)?.Value.PathName ?? "??",
            })
            .ToDictionary(t => t.TrainStationId, t => t);

        var trainStationIdsByStationIdentifierId = trainStationIdentifiersByStationId.Values.ToDictionary(t => t.Id, t => t.TrainStationId);

        // Train Station Docking Platform, by DockingStationId. I.e. Persistent_Level:PersistentLevel.Build_TrainDockingStation_C_2147007379
        var trainStationDockings = trainRelatedObjectsByType["/Game/FactoryGame/Buildable/Factory/Train/Station/Build_TrainDockingStation.Build_TrainDockingStation_C"];
        var trainStationDockingsByStationId = trainStationDockings
            .OfType<ActorObject>()
            .Select(t => new
            {
                Id = t.ObjectReference.PathName,
                InventoryId = (t.Properties.FirstOrDefault(p => p.Name == "mInventory") as ObjectProperty)?.Value.PathName ?? "??",
                IsUnloadMode = t.Properties.FirstOrDefault(p => p.Name == "mIsInLoadMode") is BoolProperty { Value: 0 },
            })
            .ToDictionary(t => t.Id, t => t);

        // Train Station Docking Platform, by StationId. I.e. Persistent_Level:PersistentLevel.Build_TrainStation_C_2147007670
        var trainStationConnections = trainRelatedObjectsByType["/Script/FactoryGame.FGTrainPlatformConnection"];
        var trainStationConnectionsByStationId = trainStationConnections.GroupBy(t => t.ParentActorName).ToDictionary(t => t.Key, t => t.ToList());
        var trainStationConnectionToPlatformsByStationId = trainStationConnectionsByStationId
            .ToDictionary(
                t => t.Key,
                t => t.Value
                .Select(tt => trainStationDockingsByStationId.TryGetValue(string.Join('.', (tt.Properties.FirstOrDefault(o => o.Name == "mConnectedTo") as ObjectProperty)?.Value.PathName.Split('.')[..^1] ?? []), out var aa0) ? aa0 : null)
                .Where(tt => tt is not null)
                .ToList());

        // Train Station, by StationId. I.e. Persistent_Level:PersistentLevel.Build_TrainStation_C_2147007670
        var trainStations = trainRelatedObjectsByType["/Game/FactoryGame/Buildable/Factory/Train/Station/Build_TrainStation.Build_TrainStation_C"];
        return trainStations
            .OfType<ActorObject>()
            .Select(t =>
            {
                var id = t.ObjectReference.PathName;
                var idShort = id.Split("_")[^1];
                var stationIdentifier = trainStationIdentifiersByStationId[id];
                var platforms = trainStationConnectionToPlatformsByStationId[id];
                var inventory = platforms.Count > 0 ? objectsByName[platforms[0]!.InventoryId] : null;
                var cargoTypes = inventory.ToCargoTypes();
                var cargo = stationIdentifier.Name.GetFlowPerMinuteFromName(cargoTypes);
                var isUnload = platforms.Count > 0 && platforms[0]!.IsUnloadMode;

                return new Station(
                   idShort,
                    stationIdentifier.Name.ToIdOnlyName(),
                    stationIdentifier.Name,
                    "train",
                    cargoTypes,
                    cargo,
                    isUnload,
                    [.. trainTimeTablesWithStops
                        .Where(ttt => ttt.StopStationIds.Contains(stationIdentifier.Id))
                        .Select(ttt => {
                            var all = ttt.StopStationIds.Select(ssi => trainStationIdsByStationIdentifierId[ssi]).Where(ssi => ssi != id).Select(ssi => ssi.Split("_")[^1]).ToList();
                            var from = isUnload ? all.First() : idShort;
                            var to = isUnload ? idShort : all.First();
                            var others = all.Skip(1).ToList();
                            return new Transporter(
                                ttt.Id.Split("_")[^1],
                                trainNameByTimeTableId[ttt.Id] ?? "??",
                                from,
                                to,
                                others); 
                        })],
                    t.Position.X,
                    t.Position.Y
                );
            });
    }
}

