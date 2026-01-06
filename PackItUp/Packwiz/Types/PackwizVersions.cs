using Nett;

namespace PackItUp.Packwiz.Types;

// https://packwiz.infra.link/reference/pack-format/pack-toml/#versions
/// <summary>
/// The versions of components used by this modpack - usually Minecraft and the mod loader this pack uses. The existence of a component implies that it should be installed. These values can also be used by tools to determine which versions of mods should be installed.
/// </summary>
public class PackwizVersions
{
    public enum Loader
    {
        Fabric,
        NeoForge,
        Forge,
        Quilt,
        LiteLoader
    }

    [TomlMember(Key = "minecraft")]
    public required string MinecraftVersion { get; set; }

    [TomlMember(Key = "fabric")]
    public string? FabricLoaderVersion { get; set; }

    [TomlMember(Key = "neoforge")]
    public string? NeoForgeLoaderVersion { get; set; }

    [TomlMember(Key = "forge")]
    public string? ForgeLoaderVersion { get; set; }

    [TomlMember(Key = "quilt")]
    public string? QuiltLoaderVersion { get; set; }

    [TomlMember(Key = "liteloader")]
    public string? LiteLoaderVersion { get; set; }

    public Loader?[] GetSupportedLoaders()
    {
        Array loaders = Enum.GetValues(typeof(Loader));
        Loader?[] supported = new Loader?[loaders.Length];

        foreach (Loader loader in loaders)
        {
            if (IsLoaderSupported(loader))
                supported[(int)loader] = loader;
        }

        return supported;
    }

    public static string GetLoaderId(Loader? loader)
        => loader switch
        {
            Loader.Fabric     => "fabric",
            Loader.NeoForge   => "neoforge",
            Loader.Forge      => "forge",
            Loader.Quilt      => "quilt",
            Loader.LiteLoader => "liteloader",
            _                 => ""
        };

    public static string? GetLoaderName(Loader? loader)
        => loader switch
        {
            Loader.Fabric     => "Fabric",
            Loader.NeoForge   => "NeoForge",
            Loader.Forge      => "Forge",
            Loader.Quilt      => "Quilt",
            Loader.LiteLoader => "LiteLoader",
            _                 => null
        };

    public static string? GetLoaderName(string? loader)
        => loader switch
        {
            "fabric"     => "Fabric",
            "neoforge"   => "NeoForge",
            "forge"      => "Forge",
            "quilt"      => "Quilt",
            "liteloader" => "LiteLoader",
            _            => null
        };


    public static Loader? GetLoader(string? loader)
        => loader switch
        {
            "fabric"     => Loader.Fabric,
            "neoforge"   => Loader.NeoForge,
            "forge"      => Loader.Forge,
            "quilt"      => Loader.Quilt,
            "liteloader" => Loader.LiteLoader,
            _            => null
        };

    public string? GetLoaderVersion(Loader? loader)
        => loader switch
        {
            Loader.Fabric     => FabricLoaderVersion,
            Loader.NeoForge   => NeoForgeLoaderVersion,
            Loader.Forge      => ForgeLoaderVersion,
            Loader.Quilt      => QuiltLoaderVersion,
            Loader.LiteLoader => LiteLoaderVersion,
            _                 => null
        };

    public bool IsLoaderSupported(Loader? loader)
        => loader switch
        {
            Loader.Fabric     => FabricLoaderVersion   != null,
            Loader.NeoForge   => NeoForgeLoaderVersion != null,
            Loader.Forge      => ForgeLoaderVersion    != null,
            Loader.Quilt      => QuiltLoaderVersion    != null,
            Loader.LiteLoader => LiteLoaderVersion     != null,
            _                 => false
        };
}