using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using PackItUp.Config.Types.Token;
using PackItUp.Extensions;
using PackItUp.ModpackProvider.Providers;
using PackItUp.Packwiz;
using PackItUp.Packwiz.Types;
using Serilog;

namespace PackItUp.ModpackProvider;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ModrinthModpackProvider), "Modrinth")]
public abstract class ModpackProvider(string id, Token token, List<string> packFolders, ILogger logger) : IModpackProvider
{
    [Description("The ID of the project to upload versions to")]
    public required string Id { get; init; } = id;

    [Description("The authentication token for this provider")]
    public required Token Token { get; init; } = token;

    [Description("List of PackWiz root folders to include in this ModpackProvider." +
                 "\nPath can be either relative from the config file root or absolute.")]
    public required List<string> PackFolders { get; init; } = packFolders;

    [Description("Placeholder that gets filled in with various Packwiz metadata, we check if the provider has a version matching this template")]
    public string VersionPlaceholder { get; set; } = "{ModpackVersion}";

    [Description("Placeholder that gets filled in with various Packwiz metadata, this gets used for the output pack filename")]
    public string ExportPlaceholder { get; set; } = "{ModpackName} v{ModpackVersion}";

    [Description("Placeholder that gets filled in with various Packwiz metadata, this gets used for the pack version on the provider's website")]
    public string ProviderVersionPlaceholder { get; set; } = "{ModpackVersion}";

    [Description("Placeholder that gets filled in with various Packwiz metadata, this gets used for the pack release's name on the provider's website")]
    public string ProviderNamePlaceholder { get; set; } = "{ModpackName} v{ModpackVersion}";

    protected ILogger Logger { get; } = logger;

    protected abstract PackwizExportType ExportType { get; }

    /// <summary>
    /// All packwiz manifests associated with this provider
    /// </summary>
    public List<(PackwizPackManager pack, PackwizVersions.Loader?[] supportedLoaders)> Manifests = [];
    /// <summary>
    /// All packwiz manifests that should be built and published
    /// </summary>
    public List<(PackwizPackManager pack, PackwizVersions.Loader?[] supportedLoaders)> ManifestsEligible = [];

    public List<(PackwizPackManager pack, string exportedPath)> ManifestsExported = [];

    /// <inheritdoc />
    public abstract Task<bool> InitializeAsync();

    /// <inheritdoc />
    public abstract Task UploadEligibleAsync();

    /// <inheritdoc />
    public virtual async Task ExportEligibleAsync()
    {
        foreach (var mf in this.ManifestsEligible)
        {
            string f = ManifestPlaceholderFormatter.Format(ExportPlaceholder, mf.pack.Manifest);

            string name = mf.pack.Manifest.Name;
            string ver = mf.pack.Manifest.Version ?? "Unknown";

            Logger.Information("Exporting pack {Name}", f);

            (int ExitCode, string? ExportedPath) exported = await mf.pack.Export(f, ExportType);
            if (exported.ExitCode != 0)
            {
                Logger.Error("Packwiz exporter exited with non-zero exit code {Code} while exporting {ManifestName} v{ManifestVersion}",
                              exported.ExitCode, name, ver);
                continue;
            }

            Log.Information("Finished exporting {ManifestName} v{ManifestVersion} to path {Path}", name, ver, exported.ExportedPath);
            ManifestsExported.Add((mf.pack, exported.ExportedPath!));
        }
    }

    public virtual async Task InitializeFoldersAsync()
    {
        for (int i = 0; i < this.PackFolders.Count; i++)
        {
            string packFolder = this.PackFolders[i];
            string path = StringExtensions.ResolvePath(packFolder);

            this.PackFolders[i] = path;

            string pkiPath = Path.Combine(path, "PackItUp");

            string pwi = Path.Combine(path, ".packwizignore");
            if (!File.Exists(pwi) || !File.ReadLines(pwi).Any(l => l == pkiPath))
            {
                await using (FileStream fs = new(pwi, FileMode.OpenOrCreate))
                {
                    fs.Seek(0, SeekOrigin.End);

                    // learned this the hard way when it filled my drive
                    ReadOnlySpan<byte> ln = Encoding.UTF8.GetBytes(Environment.NewLine + Path.GetRelativePath(path, pkiPath));
                    fs.Write(ln);
                }
            }

            if (!Directory.Exists(pkiPath))
                Directory.CreateDirectory(pkiPath);

            // changelogs
            string changelogsPath = Path.Combine(pkiPath, "Changelogs");
            if (!Directory.Exists(changelogsPath))
                Directory.CreateDirectory(changelogsPath);

            string changelogsReadmePath = Path.Combine(changelogsPath, "README.md");
            if (!File.Exists(changelogsReadmePath))
                await File.WriteAllTextAsync(changelogsReadmePath, "# PackItUp Changelogs" +
                                                                   "\nThis folder stores user created changelogs for each update, with each changelog filename being `{providerVersionPlaceholder}.md`.   " +
                                                                   "\nShould a changelog for any given version not exist, PackItUp will prompt you to create one before uploading to a provider.");

            // exports
            string exportsPath = Path.Combine(pkiPath, "Exports");
            if (!Directory.Exists(exportsPath))
                Directory.CreateDirectory(exportsPath);

            string exportsReadmePath = Path.Combine(exportsPath, "README.md");
            if (!File.Exists(exportsReadmePath))
                await File.WriteAllTextAsync(exportsReadmePath, "# PackItUp Exports" +
                                                                "\nThis folder stores exported packs, which get uploaded to the providers.");
        }
    }
}