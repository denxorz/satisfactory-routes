using System.Linq;
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
                Components = ((o.Properties.FirstOrDefault(p => p.Name == "mComponents") as ArrayProperty)?.Property as ArrayObjectProperty)?.Values.Select(c => c.PathName).ToList(),
            })
            .Where(o => (o.Components ?? []).Count > 1)
            .OrderBy(o => o.CircuitId)
            .SelectMany((o, i) => (o.Components ?? []).Select(c =>
                {
                    var componentReferenceSplitted = c.Split('.');
                    return new { ObjectReference = $"{componentReferenceSplitted[0]}.{componentReferenceSplitted[1]}", CircuitIndex = i };
                }))
            .DistinctBy(o => o.ObjectReference)
            .ToDictionary(o => o.ObjectReference, o => o.CircuitIndex);

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

                  return new Factory(
                      shortId,
                      type,
                      (int)percentageProducing,
                      powerCircuits.TryGetValue(o.ObjectReference.PathName, out var circuit) ? circuit : -1,
                      o.Position.X,
                      o.Position.Y
                  );
              });
    }
}

