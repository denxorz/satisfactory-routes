using System.Linq;
using Denxorz.Satisfactory.Routes.Types;
using SatisfactorySaveNet.Abstracts.Model;

namespace Denxorz.Satisfactory.Routes.Parsers;

public class FactoryParser(List<ComponentObject> objects, Dictionary<string, ComponentObject> objectsByName)
{
    public IEnumerable<Factory> Parse(List<PowerCircuit> powerCircuits)
    {
        var internalPowerCircuits = powerCircuits
            .OfType<PowerCircuit.InternalPowerCircuit>()
            .ToList();

        return objects
              .GroupBy(o => o.TypePath)
              .Where(o => o.First().TypePath.StartsWith("/Game/FactoryGame/Buildable/Factory/"))
              .Where(o => !o.First().TypePath.StartsWith("/Game/FactoryGame/Buildable/Factory/Storage"))
              .Where(o => !o.First().TypePath.StartsWith("/Game/FactoryGame/Buildable/Factory/Holiday"))
              .Where(o => o.First().TypePath != "/Game/FactoryGame/Buildable/Factory/Train/Station/Build_TrainDockingStation.Build_TrainDockingStation_C")
              .Where(o => o.First().TypePath != "/Game/FactoryGame/Buildable/Factory/Train/Station/Build_TrainPlatformEmpty.Build_TrainPlatformEmpty_C")
              .Where(o => o.First().Properties.Any(p => p.Name == "mPowerInfo"))
              .Where(o => !o.First().Properties.Any(p => p.Name == "mFluidBox"))
              .SelectMany(o => o)
              .OfType<ActorObject>()
              .Select(o =>
              {
                  var id = o.ObjectReference.PathName.ToId();

                  var typeFull = o.TypePath.Replace("/Game/FactoryGame/Buildable/Factory/", null);
                  var type = typeFull[..typeFull.IndexOf('/')];

                  int? percentageProducing = null;
                  float? currentProductivityMeasurementDuration = o.Properties.GetFloat("mCurrentProductivityMeasurementProduceDuration");
                  if (currentProductivityMeasurementDuration is not null)
                  {
                      percentageProducing = (int)Math.Floor(currentProductivityMeasurementDuration.Value /
                                                  (o.Properties.GetFloat("mCurrentProductivityMeasurementDuration") ?? 100) * 100);
                  }

                  var circuit = internalPowerCircuits.FirstOrDefault(pc => pc.AttachedComponents.Contains(o.ObjectReference.PathName));

                  return new Factory(
                      id,
                      type,
                      percentageProducing,
                      circuit?.ParentCircuitId ?? -1,
                      circuit?.Id ?? -1,
                      o.Position.X,
                      o.Position.Y
                  );
              });
    }
}

internal record TmpPowerCircuit(string PathName, string CircuitAPathName, string CircuitBPathName, int? Priority, bool IsSwitchedOn, string? Name);
