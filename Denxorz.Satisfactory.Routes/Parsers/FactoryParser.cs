using SatisfactorySaveNet.Abstracts.Model;
using SatisfactorySaveNet.Abstracts.Model.Properties;

namespace Denxorz.Satisfactory.Routes.Parsers;

public class FactoryParser(List<ComponentObject> objects, Dictionary<string, ComponentObject> objectsByName)
{
    public IEnumerable<Factory> Parse()
    {
        return objects
              .GroupBy(o => o.TypePath)
              .Where(o => o.First().TypePath.StartsWith("/Game/FactoryGame/Buildable/Factory/"))
              .Where(o => o.First().Properties.Any(p => p.Name == "mCurrentProductivityMeasurementDuration"))
              .SelectMany(o => o)
              .OfType<ActorObject>()
              .Select(o => {

                  var shortId = o.ObjectReference.PathName.Split("_")[^1];

                  var typeFull = o.TypePath.Replace("/Game/FactoryGame/Buildable/Factory/", null);
                  var type = typeFull[..typeFull.IndexOf('/')];

                  var percentageProducing = Math.Floor(((o.Properties.FirstOrDefault(p => p.Name == "mCurrentProductivityMeasurementProduceDuration") as FloatProperty)?.Value ?? 0) /
                    ((o.Properties.FirstOrDefault(p => p.Name == "mCurrentProductivityMeasurementDuration") as FloatProperty)?.Value ?? 100) * 100);

                  return new Factory(
                      shortId,
                      type,
                      (int)percentageProducing,
                      o.Position.X,
                      o.Position.Y
                  );
              });
    }
}

