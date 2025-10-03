namespace Denxorz.Satisfactory.Routes.Types;

public record CargoFlow(string Type, bool IsUnload, double? FlowPerMinute, bool IsExact);

