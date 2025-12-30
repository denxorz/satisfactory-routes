using Denxorz.Satisfactory.Routes.Types;
using SatisfactorySaveNet.Abstracts.Model;

namespace Denxorz.Satisfactory.Routes.Parsers;

public class ResourceParser(List<ComponentObject> objects, Dictionary<string, ComponentObject> objectsByName)
{
    public IEnumerable<Resource> Parse()
    {
        var miners = objects
            .Where(o => o.TypePath.StartsWith("/Game/FactoryGame/Buildable/Factory/Miner"))
            .ToList();

        var minersByResource = miners
            .GroupBy(m => m.Properties.GetObjectPathName("mExtractableResource"))
            .ToDictionary(m => m.Key, m => m.First());

        return [.. Resources.All
             .Select(r =>
             {
                 var miner = minersByResource.TryGetValue(r.Id, out var m) ? m : null;

                 float percentageProducing = 0;
                 int minerLevel = 1;
                 float clockSpeed = 1;

                 if (miner is not null)
                 {
                     float? currentProductivityMeasurementDuration = miner.Properties.GetFloat("mCurrentProductivityMeasurementProduceDuration");
                     if (currentProductivityMeasurementDuration is not null)
                     {
                         percentageProducing = currentProductivityMeasurementDuration.Value /
                                                     (miner.Properties.GetFloat("mCurrentProductivityMeasurementDuration") ?? 100);
                     }

                     minerLevel = miner.TypePath.EndsWith("Mk3_C") ? 4 : miner.TypePath.EndsWith("Mk2_C") ? 2 : 1;
                     clockSpeed = miner.Properties.GetFloat("mCurrentPotential") ?? 1;
                 }

                 return new Resource(
                     r.Id.ToId(),
                     r.Type,
                     miner?.ObjectReference.PathName.ToId(),
                     r.PurityModifier,
                     minerLevel,
                     percentageProducing,
                     clockSpeed,                     
                     r.X,
                     r.Y);
             })];
    }
}



