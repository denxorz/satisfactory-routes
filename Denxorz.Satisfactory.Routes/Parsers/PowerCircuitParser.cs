using Denxorz.Satisfactory.Routes.Types;
using SatisfactorySaveNet.Abstracts.Model;
using static Denxorz.Satisfactory.Routes.Types.PowerCircuit;

namespace Denxorz.Satisfactory.Routes.Parsers;

public class PowerCircuitParser(List<ComponentObject> objects, Dictionary<string, ComponentObject> objectsByName)
{
    public IEnumerable<PowerCircuit> Parse()
    {
        var switches = objects
             .Where(o => o.TypePath.EndsWith("PowerSwitch_C"))
             .OfType<ActorObject>()
             .Select(a => new TmpPowerCircuit(
                 a.ObjectReference.PathName,
                 $"{a.ObjectReference.PathName}.PowerConnection1",
                 $"{a.ObjectReference.PathName}.PowerConnection2",
                 a.Properties.GetInt("mPriority"),
                 a.Properties.GetBool("mIsSwitchOn") == true,
                 a.Properties.GetString("mBuildingTag")
             ))
             .ToList();

        var switchByPowerConnection2PathName = switches.ToDictionary(s => s.CircuitBPathName, s => s);

        var powerCircuits = objects
            .Where(o => o.TypePath == "/Script/FactoryGame.FGPowerCircuit")
            .Select(o => new
            {
                o.ObjectReference.PathName,
                CircuitId = o.Properties.GetInt("mCircuitID") ?? 0,
                Components = o.Properties.GetObjectArray("mComponents").Select(c => c.PathName).ToList(),
            })
            .Where(o => o.Components.Count > 1)
            .ToList();

        var circuitIdByComponentReference = powerCircuits
            .SelectMany(o => o.Components.Select(c => new { ObjectReference = c, CircuitIndex = o.CircuitId }))
            .ToDictionary(o => o.ObjectReference, o => o.CircuitIndex);

        var componentReferencesByCircuitId = circuitIdByComponentReference
            .GroupBy(o => o.Value)
            .ToDictionary(o => o.Key, o => o.Select(s => string.Join(".", s.Key.Split('.').Take(2))));

        var circuitNames = switchByPowerConnection2PathName
            .Select(s => (circuitIdByComponentReference.TryGetValue(s.Key, out var name) ? name : -1, s.Value))
            .Where(s => s.Item1 != -1)
            .GroupBy(s => s.Item1!)
            .ToDictionary(s => s.Key!, s => s.First().Value);

        var switchesOn = switches
              .Where(o => o.IsSwitchedOn)
              .ToList();

        var switchGroups = switchesOn
             .Select(s => (
                 circuitIdByComponentReference.TryGetValue(s.CircuitAPathName, out var circuit) ? circuit : -1,
                 circuitIdByComponentReference.TryGetValue(s.CircuitBPathName, out var circuit2) ? circuit2 : -1
             ))
             .ToList();

        var connectedPowerCircuits = GroupConnectedIds(switchGroups);

        var topCircuits = connectedPowerCircuits
            .Select((c, i) =>
            {
                circuitNames.TryGetValue(i, out var sw);
                return new PowerCircuit(i, null, sw?.Name, sw?.Priority, sw?.IsSwitchedOn ?? false);
            });

        var subCircuits = connectedPowerCircuits
            .SelectMany((c, i) => c.Select(sc =>
            {
                circuitNames.TryGetValue(sc, out var sw);
                return new InternalPowerCircuit(
                                    sc,
                                    i,
                                    sw?.Name, 
                                    sw?.Priority, 
                                    sw?.IsSwitchedOn ?? false,
                                    componentReferencesByCircuitId.TryGetValue(sc, out var components) ? [.. components] : []);
            }));

        var leftOverCircuits = powerCircuits
            .Where(c => !subCircuits.Any(s => s.Id == c.CircuitId))
            .Select(c =>
            {
                circuitNames.TryGetValue(c.CircuitId, out var sw);
                return new InternalPowerCircuit(
                    c.CircuitId,
                    null, 
                    sw?.Name, 
                    sw?.Priority, 
                    sw?.IsSwitchedOn ?? false,
                    componentReferencesByCircuitId.TryGetValue(c.CircuitId, out var components) ? [.. components] : []);
            }).
            Where(c => c.AttachedComponents.Count > 1);

        return topCircuits.Concat(leftOverCircuits).Concat(subCircuits);
    }

    private static List<List<int>> GroupConnectedIds(List<(int A, int B)> pairs)
    {
        var connected = new List<List<int>>();
        var processed = new HashSet<int>();

        var graph = new Dictionary<int, HashSet<int>>();
        foreach (var (a, b) in pairs.Where(p => p.A >= 0 && p.B >= 0))
        {
            if (!graph.TryGetValue(a, out var value))
            {
                value = [];
                graph[a] = value;
            }
            value.Add(b);

            if (!graph.TryGetValue(b, out var value2))
            {
                value2 = [];
                graph[b] = value2;
            }
            value2.Add(a);
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
                connected.Add(group);
            }
        }

        return connected;
    }
}
