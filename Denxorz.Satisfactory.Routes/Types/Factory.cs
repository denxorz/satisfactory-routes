namespace Denxorz.Satisfactory.Routes.Types;

public record Factory(
    string Id,
    string Type,
    int PercentageProducing,
    int MainPowerCircuitId,
    int SubPowerCircuitId,
    float X,
    float Y);
