using SatisfactorySaveNet.Abstracts.Model;
using SatisfactorySaveNet.Abstracts.Model.Properties;
using SatisfactorySaveNet.Abstracts.Model.Typed;

namespace Denxorz.Satisfactory.Routes.Parsers;

public static class PropertiesExtensions
{
    public static bool? GetBool(this ICollection<Property> properties, string name)
    {
        var v = (properties.GetByName(name) as BoolProperty)?.Value;
        return v is null ? null : v == 1;
    }

    public static int? GetInt(this ICollection<Property> properties, string name)
        => (properties.GetByName(name) as IntProperty)?.Value;

    public static float? GetFloat(this ICollection<Property> properties, string name)
        => (properties.GetByName(name) as FloatProperty)?.Value;

    public static string? GetString(this ICollection<Property> properties, string name)
        => (properties.GetByName(name) as StrProperty)?.Value;

    public static string? GetText(this ICollection<Property> properties, string name)
    => (properties.GetByName(name) as TextProperty)?.Value;

    public static ObjectReference? GetObjectReference(this ICollection<Property> properties, string name)
       => (properties.GetByName(name) as ObjectProperty)?.Value;

    public static string GetObjectPathName(this ICollection<Property> properties, string name)
        => properties.GetObjectReference(name)?.PathName ?? "??";

    public static ICollection<ObjectReference> GetObjectArray(this ICollection<Property> properties, string name)
        => ((properties.GetByName(name) as ArrayProperty)?.Property as ArrayObjectProperty)?.Values ?? [];

    public static ICollection<T> GetTypedArray<T>(this ICollection<Property> properties, string name) where T : TypedData
        => ((properties.GetByName(name) as ArrayProperty)?.Property as ArrayStructProperty)?.Values?.Cast<T>().ToList() ?? [];

    public static Property? GetByName(this ICollection<Property> properties, string name)
        => properties.FirstOrDefault(p => p.Name == name);
}

