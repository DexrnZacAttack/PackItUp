using Nett;
using PackItUp.Packwiz.Types.Extension;

namespace PackItUp.Packwiz.Types;

// https://packwiz.infra.link/reference/pack-format/pack-toml/
/// <summary>
/// The main modpack file for a packwiz modpack. This is the first file loaded, to allow the modpack downloader to download all the files in the modpack.
/// </summary>
public class PackwizManifest
{
    /// <summary>
    /// Information about the index file in this modpack.
    /// </summary>
    [TomlMember(Key = "index")]
    public required PackwizIndexInfo IndexInfo { get; set; }

    /// <summary>
    /// The name of the modpack. This can be displayed in user interfaces to identify the pack, and it does not need to be unique between packs.
    /// </summary>
    [TomlMember(Key = "name")]
    public required string Name { get; set; }

    /// <summary>
    /// A version string identifying the pack format and version of it. Currently, this pack format uses version 1.1.0.
    /// </summary>
    [TomlMember(Key = "pack-format")]
    public required string PackFormat { get; set; }

    /// <summary>
    /// The versions of components used by this modpack - usually Minecraft and the mod loader this pack uses. The existence of a component implies that it should be installed. These values can also be used by tools to determine which versions of mods should be installed.
    /// </summary>
    [TomlMember(Key = "versions")]
    public required PackwizVersions Versions { get; set; }

    /// <summary>
    /// The author(s) of the modpack. This is output when exporting to the CurseForge pack format, and can be displayed in user interfaces.
    /// </summary>
    [TomlMember(Key = "author")]
    public string? Author { get; set; }

    /// <summary>
    /// A short description of the modpack. This is output when exporting to the Modrinth pack format, but is not currently used elsewhere by the tools or installer.
    /// </summary>
    [TomlMember(Key = "description")]
    public string? Description { get; set; }

    /// <summary>
    /// The version of the modpack. This is output when exporting to the CurseForge pack format, but is not currently used elsewhere by the tools or installer. It must not be used for determining if the modpack is outdated.
    /// </summary>
    [TomlMember(Key = "version")]
    public string? Version { get; set; }

    /// <summary>
    /// PackItUp specific info
    /// </summary>
    [TomlMember(Key = "pack-it-up")]
    public PackItUpInfo? PackItUp { get; set; }
}