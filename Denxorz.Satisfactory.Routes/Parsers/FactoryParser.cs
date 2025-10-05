using Denxorz.Satisfactory.Routes.Types;
using SatisfactorySaveNet.Abstracts.Model;
using SatisfactorySaveNet.Abstracts.Model.Properties;

namespace Denxorz.Satisfactory.Routes.Parsers;

public class FactoryParser(List<ComponentObject> objects, Dictionary<string, ComponentObject> objectsByName)
{
    public IEnumerable<Factory> Parse()
    {
        var powerCircuits = objects
            .Where(o => o.TypePath == "/Script/FactoryGame.FGPowerCircuit")
            .Select(o => new
            {
                CircuitId = (o.Properties.FirstOrDefault(p => p.Name == "mCircuitID") as IntProperty)?.Value ?? 0,
                Components = ((o.Properties.FirstOrDefault(p => p.Name == "mComponents") as ArrayProperty)?.Property as ArrayObjectProperty)?.Values.Select(c => c.PathName).ToList() ?? [],
            })
            .Where(o => o.Components.Count > 1)
            .ToList();

        var powerCircuits2 = powerCircuits
            .SelectMany(o => o.Components.Select(c => new { ObjectReference = string.Join(".", c.Split('.').Take(2)), CircuitIndex = o.CircuitId }))
            .DistinctBy(o => o.ObjectReference)
            .ToDictionary(o => o.ObjectReference, o => o.CircuitIndex);

        var powerCircuits3 = powerCircuits
            .SelectMany(o => o.Components.Select(c => new { ObjectReference = c, CircuitIndex = o.CircuitId }))
            .ToDictionary(o => o.ObjectReference, o => o.CircuitIndex);

        var switches = objects
              .Where(o => o.TypePath.EndsWith("PowerSwitch_C"))
              .Where(o => (o.Properties.FirstOrDefault(p => p.Name == "mIsSwitchOn") as BoolProperty)?.Value == 1)
              .OfType<ActorObject>()
              .ToList();

        var switchGroups = switches
            .Select(o => (
                powerCircuits3.TryGetValue(o.Components.First().PathName, out var circuit) ? circuit : -1,
                powerCircuits3.TryGetValue(o.Components.Skip(1).First().PathName, out var circuit2) ? circuit2 : -1,
                (o.Properties.FirstOrDefault(p => p.Name == "mBuildingTag") as StrProperty)?.Value ?? ""
            ))
            .ToList();

        var mainPowerCircuits = GroupConnectedIds(switchGroups);

        return objects
              .GroupBy(o => o.TypePath)
              .Where(o => o.First().TypePath.StartsWith("/Game/FactoryGame/Buildable/Factory/"))
              .Where(o => o.First().Properties.Any(p => p.Name == "mCurrentProductivityMeasurementDuration"))
              .SelectMany(o => o)
              .OfType<ActorObject>()
              .Select(o =>
              {
                  var shortId = o.ObjectReference.PathName.Split("_")[^1];

                  var typeFull = o.TypePath.Replace("/Game/FactoryGame/Buildable/Factory/", null);
                  var type = typeFull[..typeFull.IndexOf('/')];

                  var percentageProducing = Math.Floor(((o.Properties.FirstOrDefault(p => p.Name == "mCurrentProductivityMeasurementProduceDuration") as FloatProperty)?.Value ?? 0) /
                    ((o.Properties.FirstOrDefault(p => p.Name == "mCurrentProductivityMeasurementDuration") as FloatProperty)?.Value ?? 100) * 100);

                  var subCircuitId = powerCircuits2.TryGetValue(o.ObjectReference.PathName, out var circuit) ? circuit : -1;

                  return new Factory(
                      shortId,
                      type,
                      (int)percentageProducing,
                      mainPowerCircuits.FindIndex(c => c.Contains(subCircuitId)),
                      subCircuitId,
                      o.Position.X,
                      o.Position.Y
                  );
              });
    }


    public static List<List<int>> GroupConnectedIds(List<(int, int, string)> pairs)
    {
        var groups = new List<List<int>>();
        var processed = new HashSet<int>();

        var graph = new Dictionary<int, HashSet<int>>();
        foreach (var (id1, id2, name) in pairs.Where(p => p.Item1 >= 0 && p.Item2 >= 0))
        {
            if (!graph.TryGetValue(id1, out var value))
            {
                value = [];
                graph[id1] = value;
            }

            if (!graph.TryGetValue(id2, out var value2))
            {
                value2 = [];
                graph[id2] = value2;
            }

            value.Add(id2);
            value2.Add(id1);
        }

        foreach (var node in graph.Keys)
        {
            if (processed.Contains(node))
            {
                continue;
            }

            var group = new List<int>();
            var toProcess = new Queue<int>();
            toProcess.Enqueue(node);

            while (toProcess.Count > 0)
            {
                var current = toProcess.Dequeue();
                if (processed.Contains(current))
                {
                    continue;
                }

                processed.Add(current);
                group.Add(current);
                foreach (var neighbor in graph[current].Where(neighbor => !processed.Contains(neighbor)))
                {
                    toProcess.Enqueue(neighbor);
                }
            }

            if (group.Count > 0)
            {
                groups.Add(group);
            }
        }

        return groups;
    }
}

