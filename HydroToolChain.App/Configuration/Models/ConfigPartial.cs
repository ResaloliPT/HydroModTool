using HydroToolChain.App.Configuration.Data;

namespace HydroToolChain.App.Configuration.Models;

public sealed class ConfigPartial
{
    public ConfigPartials PartialType { get; init; }

    public List<GuidData>? Guids { get; init; }
    
    public List<UidData>? Uids { get; init; }
}