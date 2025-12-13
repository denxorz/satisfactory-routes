namespace Denxorz.Satisfactory.Routes.Types;

public record PowerCircuit(
    int Id,
    int? ParentCircuitId,
    string? Name, 
    int? Priority,
    bool IsOn)
{
    public record InternalPowerCircuit(
        int Id,
        int? ParentCircuitId,
        string? Name,
        int? Priority,
        bool IsOn,
        List<string> AttachedComponents) 
        : PowerCircuit(Id, ParentCircuitId, Name, Priority, IsOn);
}
