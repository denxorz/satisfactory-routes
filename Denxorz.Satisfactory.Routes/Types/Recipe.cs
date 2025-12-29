namespace Denxorz.Satisfactory.Routes.Types;

public record Recipe(
    string ClassName,
    string Name,
    string UnlockedBy,
    float Duration,
    RecipeItemFlow[] Ingredients,
    RecipeItemFlow[] Products,
    string[] ProducedIn,
    bool InCraftBench,
    bool InWorkshop,
    bool InBuildGun,
    bool InCustomizer,
    float ManualCraftingMultiplier,
    bool Alternate,
    int? MinPower,
    int? MaxPower,
    object[] Seasons,
    bool Stable,
    bool Experimental);
