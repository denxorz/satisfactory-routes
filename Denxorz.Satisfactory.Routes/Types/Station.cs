namespace Denxorz.Satisfactory.Routes.Types;

public record Station(
    string Id,
    string ShortName,
    string Name,
    string Type,
    List<string> CargoTypes,
    List<CargoFlow> CargoFlows,
    bool IsUnload,
    List<Transporter> Transporters,
    float X,
    float Y);
