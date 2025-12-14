using Denxorz.Satisfactory.Routes.Types;
using SatisfactorySaveNet.Abstracts.Model;

namespace Denxorz.Satisfactory.Routes.Parsers;

public class ResourceParser(List<ComponentObject> objects, Dictionary<string, ComponentObject> objectsByName)
{
    public IEnumerable<Resource> Parse()
    {
        var miners = objects.Where(o => o.TypePath.StartsWith("/Game/FactoryGame/Buildable/Factory/Miner")).ToList();
        var minersByResource = miners.ToDictionary(m => m.Properties.GetObjectPathName("mExtractableResource"), m => m);

        return Resources.All
             .Select(r =>
             {
                 var miner = minersByResource.TryGetValue(r.Id, out var m) ? m : null;

                 int percentageProducing = 0;
                 int minerMultiplier = 1;
                 float overclockMultiplier = 1;

                 if (miner is not null)
                 {
                     float? currentProductivityMeasurementDuration = miner.Properties.GetFloat("mCurrentProductivityMeasurementProduceDuration");
                     if (currentProductivityMeasurementDuration is not null)
                     {
                         percentageProducing = (int)Math.Floor(currentProductivityMeasurementDuration.Value /
                                                     (miner.Properties.GetFloat("mCurrentProductivityMeasurementDuration") ?? 100));
                     }

                     minerMultiplier = miner.TypePath.EndsWith("Mk3_C") ? 4 : miner.TypePath.EndsWith("Mk2_C") ? 2 : 1;
                     overclockMultiplier = miner.Properties.GetFloat("mCurrentPotential") ?? 1;
                 }

                 return new Resource(
                     r.Id.ToId(), 
                     r.Type,
                     (float)Math.Round(60 * r.PurityModifier * minerMultiplier * percentageProducing * overclockMultiplier, 1),
                     (int)Math.Round(60 * r.PurityModifier * 4 * 2.5, 0),
                     r.X,
                     r.Y);
             })
             .ToList();
    }
}



