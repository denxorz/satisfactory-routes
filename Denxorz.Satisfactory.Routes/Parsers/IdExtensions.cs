namespace Denxorz.Satisfactory.Routes.Parsers;

public static class IdExtensions
{
    public static string Short(this string id) => id.Split("_")[^1];
}
