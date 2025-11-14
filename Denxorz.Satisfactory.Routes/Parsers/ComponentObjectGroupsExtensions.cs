using SatisfactorySaveNet.Abstracts.Model;

namespace Denxorz.Satisfactory.Routes.Parsers;

public static class ComponentObjectGroupsExtensions
{
    public static List<ComponentObject> GetGroup(this Dictionary<string, List<ComponentObject>> groups, string name)
        => groups.TryGetValue(name, out var groupOut) ? groupOut : [];
}
