namespace Denxorz.Satisfactory.Routes.Types;

public record Resource(
    string Id,
    string Type,
    float Flow,
    int Max,
    float X,
    float Y);

