using PackItUp.Packwiz;
using PackItUp.Packwiz.Types;

namespace PackItUp;

public static class ManifestPlaceholderFormatter
{
    public static string Format(string placeholder, PackwizManifest manifest)
    {
        PackwizVersions.Loader? loader = manifest.Versions.GetSupportedLoaders().First(l => l != null);

        return SmartFormat.Smart.Format(placeholder,
                                        new
                                        {
                                            ModpackName = manifest.Name,
                                            ModpackVersion = manifest.Version ?? string.Empty,
                                            MinecraftVersion = manifest.Versions.MinecraftVersion,
                                            ModpackLoaderId = PackwizVersions.GetLoaderId(loader) ?? "",
                                            ModpackLoaderName = PackwizVersions.GetLoaderName(loader) ?? "",
                                            ModpackLoaderVersion = manifest.Versions.GetLoaderVersion(loader),
                                            DateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")
                                        });
    }
}