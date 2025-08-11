using System.Globalization;
using System.Text.RegularExpressions;
using FuzzySharp;
using SatisfactorySaveNet.Abstracts.Model;
using SatisfactorySaveNet.Abstracts.Model.Properties;
using SatisfactorySaveNet.Abstracts.Model.Typed;

namespace Denxorz.Satisfactory.Routes;

public static class StationExtensions
{
    public static string ToIdOnlyName(this string fullName)
    {
        if (fullName.StartsWith('['))
        {
            return fullName.Split('[')[1].Trim(']');
        }

        return fullName.Split('[')[0].Trim();
    }

    public static string[] ToStops(this Property? p)
    {
        var arrayProperty = p as ArrayProperty;
        var arrayPropertyValues = arrayProperty?.Property is ArrayStructProperty arrayStruct ? arrayStruct.Values : [];
        var arrayProperties = arrayPropertyValues.OfType<ArrayProperties>();
        var stationIds = arrayProperties
            .SelectMany(sp => sp.Values
                .Where(spp => spp.Name == "Station")
                .OfType<ObjectProperty>()
                .Select(spp => spp.Value.PathName))
            .ToArray();

        return stationIds;
    }

    public static List<string> ToCargoTypes(this ComponentObject? inventory)
    {
        if (inventory is null)
        {
            return [];
        }

        var stacks = ((inventory.Properties.FirstOrDefault(p => p.Name == "mInventoryStacks") as ArrayProperty)?.Property as ArrayStructProperty)?.Values as TypedData[] ?? [];
        var inventoryItemStacks = stacks.Select(s => ((s as ArrayProperties)?.Values.FirstOrDefault() as StructProperty)?.Value as InventoryItem);
        var allStacksWithItems = inventoryItemStacks.Where(item => item?.ExtraProperty is IntProperty { Value: > 0 });
        var allStacksWithItemsDistinct = allStacksWithItems.Select(s => PrettyItemName(s?.ItemType?.Split(".")[^1] ?? "")).Distinct().Where(s => !string.IsNullOrWhiteSpace(s));

        static string PrettyItemName(string dirtyItemName) => dirtyItemName
                .Replace("Desc_", null, StringComparison.InvariantCultureIgnoreCase)
                .Replace("BP_", null, StringComparison.InvariantCultureIgnoreCase)
                .Replace("ItemDescriptor", null, StringComparison.InvariantCultureIgnoreCase)
                .Replace("_C", null, StringComparison.InvariantCultureIgnoreCase);

        return [.. allStacksWithItemsDistinct];
    }



    public static List<CargoFlow> GetFlowPerMinuteFromName(this string name, List<string> cargoTypes)
    {
        var flowSpecPattern = @"\[([^\]]+)\]";
        var flows = new List<CargoFlow>(1);

        Regex r = new(flowSpecPattern, RegexOptions.IgnoreCase);
        Match m = r.Match(name);
        while (m.Success)
        {
            for (int i = 1; i < m.Groups.Count; i++)
            {
                Group g = m.Groups[i];

                var splittedGroup = g.ToString().Split(' ');
                if (splittedGroup.Length == 3)
                {
                    string flowAsString = splittedGroup[1];
                    string flowWithoutVariable = flowAsString.Replace("~", null);
                    string flowLowest = flowWithoutVariable.Split('-')[0];

                    var potential = Process.ExtractOne(splittedGroup[2], cargoTypes.Count == 0 ? GameItems.AvailableItems : cargoTypes);
                    var cargoType = potential.Score >= 20 ? potential.Value : splittedGroup[2];

                    flows.Add(new(
                        cargoType,
                        splittedGroup[0].StartsWith("in", StringComparison.InvariantCultureIgnoreCase),
                        float.TryParse(flowLowest, NumberStyles.Float, CultureInfo.InvariantCulture, out float f) ? (int)Math.Ceiling(f) : null,
                        !flowAsString.StartsWith('~')));
                }

            }
            m = m.NextMatch();
        }

        return flows;
    }
}

