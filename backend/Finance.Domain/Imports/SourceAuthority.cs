namespace Finance.Domain.Imports;

public sealed class SourceAuthority
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ShopCode { get; init; } = "";
    public string Mode { get; set; } = "Local";
    public int Version { get; set; } = 1;
    public string ApprovedBy { get; set; } = "";
}
public sealed class SourceSwitchRequest
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ShopCode { get; init; } = "";
    public string TargetMode { get; init; } = "";
    public int ExpectedVersion { get; init; }
    public string ProposedBy { get; init; } = "";
    public string Checkpoint { get; init; } = "";
    public string MappingManifest { get; init; } = "";
    public string OpenWorkManifest { get; init; } = "";
    public string? ApprovedBy { get; set; }
}
