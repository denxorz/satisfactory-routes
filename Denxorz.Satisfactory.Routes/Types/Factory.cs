namespace Denxorz.Satisfactory.Routes.Types;

public record Factory(
    string Id,
    string Type,
    int PercentageProducing,
    int PowerCircuitId,
    float X,
    float Y);
