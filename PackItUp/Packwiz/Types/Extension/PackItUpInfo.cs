using Nett;
using PackItUp.Types;

namespace PackItUp.Packwiz.Types.Extension;

/// <summary>
/// Custom PackItUp-specific properties inside the Packwiz manifest
/// </summary>
public class PackItUpInfo
{
    /// <summary>
    /// The stage of the modpack
    /// </summary>
    [TomlMember(Key = "modpack-stage")]
    public PackStage Stage { get; set; }
}