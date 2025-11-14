namespace Denxorz.Satisfactory.Routes.Parsers;

public static class IdExtensions
{
    public static string ToId(this string id) => id.Split("_")[^1];

    public static string ToIdOnlyName(this string fullName)
    {
        if (fullName.StartsWith('['))
        {
            return fullName.Split('[')[1].Trim(']');
        }

        return fullName.Split('[')[0].Trim();
    }
}
