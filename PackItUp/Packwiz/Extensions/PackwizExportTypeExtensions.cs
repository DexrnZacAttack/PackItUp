using PackItUp.Packwiz.Types;

namespace PackItUp.Packwiz.Extensions;

public static class PackwizExportTypeExtensions
{
    public static string GetString(this PackwizExportType type)
        => type switch
        {
            PackwizExportType.Modrinth   => "modrinth",
            PackwizExportType.CurseForge => "curseforge",
            _                            => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

    public static string GetExportFileExtension(this PackwizExportType type)
        => type switch
        {
            PackwizExportType.Modrinth   => "mrpack",
            PackwizExportType.CurseForge => "zip",
            _                            => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
}