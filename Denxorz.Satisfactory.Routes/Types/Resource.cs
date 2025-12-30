namespace Denxorz.Satisfactory.Routes.Types;

public record Resource(
    string Id,
    string Type,
    string? MinerId,
    float PurityModifier,
    int MinerLevel,
    float PercentageProducing,
    float ClockSpeed,
    float X,
    float Y)
{
    public float Flow => (float)Math.Round(60 * PurityModifier * MinerLevel * PercentageProducing * ClockSpeed, 1);
    public int Max => (int)Math.Round(60 * PurityModifier * 4 * 2.5, 0);
}

