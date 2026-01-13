using System.Reflection;
using System.Text.Json;
using Denxorz.Satisfactory.Routes.Types;
using SatisfactorySaveNet.Abstracts.Model;

namespace Denxorz.Satisfactory.Routes.Parsers;

public class FactoryParser(List<ComponentObject> objects, Dictionary<string, ComponentObject> objectsByName)
{
    private static Dictionary<string, Recipe> LoadData()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Denxorz.Satisfactory.Routes.data.json")!;
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        return (JsonSerializer.Deserialize<Dictionary<string, List<Recipe>>>(stream, opts) ?? [])
            .ToDictionary(r => r.Key, r => r.Value[0]);
    }

    public IEnumerable<Factory> Parse(List<PowerCircuit> powerCircuits)
    {
        var recipes = LoadData();

        var internalPowerCircuits = powerCircuits
            .OfType<PowerCircuit.InternalPowerCircuit>()
            .ToList();

        return objects
              .GroupBy(o => o.TypePath)
              .Where(o => o.First().TypePath.StartsWith("/Game/FactoryGame/Buildable/Factory/"))
              .Where(o =>
              {
                  var type = o.First().TypePath;
                  return type.StartsWith("/Game/FactoryGame/Buildable/Factory/")
                      && !type.StartsWith("/Game/FactoryGame/Buildable/Factory/Storage")
                      && !type.StartsWith("/Game/FactoryGame/Buildable/Factory/Holiday")
                      && type != "/Game/FactoryGame/Buildable/Factory/CentralStorage/Build_CentralStorage.Build_CentralStorage_C"
                      && type != "/Game/FactoryGame/Buildable/Factory/TradingPost/Build_TradingPost.Build_TradingPost_C"
                      && type != "/Game/FactoryGame/Buildable/Factory/ResourceSinkShop/Build_ResourceSinkShop.Build_ResourceSinkShop_C"
                      && type != "/Game/FactoryGame/Buildable/Factory/Train/Station/Build_TrainPlatformEmpty_02.Build_TrainPlatformEmpty_02_C"
                      && type != "/Game/FactoryGame/Buildable/Factory/PipeHyperTJunction/Build_HypertubeTJunction.Build_HypertubeTJunction_C"
                      && type != "/Game/FactoryGame/Buildable/Factory/PipeHyperJunction/Build_HyperTubeJunction.Build_HyperTubeJunction_C"
                      && type != "/Game/FactoryGame/Buildable/Factory/Train/Station/Build_TrainDockingStation.Build_TrainDockingStation_C"
                      && type != "/Game/FactoryGame/Buildable/Factory/Train/Station/Build_TrainPlatformEmpty.Build_TrainPlatformEmpty_C";
              })
              .Where(o => o.First().Properties.Any(p => p.Name == "mPowerInfo"))
              .Where(o => !o.First().Properties.Any(p => p.Name == "mFluidBox"))
              .SelectMany(o => o)
              .OfType<ActorObject>()
              .Select(o =>
              {
                  var id = o.ObjectReference.PathName.ToId();

                  var typeFull = o.TypePath.Replace("/Game/FactoryGame/Buildable/Factory/", null);
                  var type = typeFull[..typeFull.IndexOf('/')];

                  if (type == "Train")
                  {
                      type = "TrainStation";
                  }

                  int? percentageProducing = null;
                  float? currentProductivityMeasurementDuration = o.Properties.GetFloat("mCurrentProductivityMeasurementProduceDuration");
                  if (currentProductivityMeasurementDuration is not null)
                  {
                      percentageProducing = (int)Math.Floor(currentProductivityMeasurementDuration.Value /
                                                  (o.Properties.GetFloat("mCurrentProductivityMeasurementDuration") ?? 100) * 100);
                  }

                  var circuit = internalPowerCircuits.FirstOrDefault(pc => pc.AttachedComponents.Contains(o.ObjectReference.PathName));

                  var recipeFullName = o.Properties.GetObjectPathName("mCurrentRecipe");
                  recipes.TryGetValue(recipeFullName.Split('.')[^1], out var recipe);
                  var durationMultiplier = recipe?.Duration ?? 1;

                  var clockSpeed = o.Properties.GetFloat("mPendingPotential") * 100 ?? DefaultClockSpeed(percentageProducing);

                  return new Factory(
                      id,
                      type,
                      percentageProducing,
                      circuit?.ParentCircuitId ?? -1,
                      circuit?.Id ?? -1,
                      o.Position.X,
                      o.Position.Y,
                      clockSpeed is not null ? (float)Math.Round(clockSpeed.Value, 3) : null,
                      (int?)o.Properties.GetFloat("mPendingProductionBoost") == 2,
                      recipe?.ClassName,
                      recipe?.Name,
                      [.. recipe?.Ingredients.Select(i => new FactoryFlow(i.Item.PrettyItemName(), i.Amount * durationMultiplier)) ?? []],
                      [.. recipe?.Products.Select(i => new FactoryFlow(i.Item.PrettyItemName(), i.Amount * durationMultiplier)) ?? []]
                  );
              });
    }

    private static bool HasClockSpeed(int? percentageProducing) => percentageProducing is not null;
    private static float? DefaultClockSpeed(int? percentageProducing) => HasClockSpeed(percentageProducing) ? 100 : null;
}

internal record TmpPowerCircuit(string PathName, string CircuitAPathName, string CircuitBPathName, int? Priority, bool IsSwitchedOn, string? Name);
