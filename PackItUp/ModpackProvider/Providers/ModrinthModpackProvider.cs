using System.Net;
using Modrinth;
using Modrinth.Exceptions;
using Modrinth.Extensions;
using Modrinth.Models;
using Modrinth.Models.Enums.Project;
using Modrinth.Models.Enums.Version;
using Nett;
using PackItUp.Config.Types.Token;
using PackItUp.Extensions;
using PackItUp.Packwiz;
using PackItUp.Packwiz.Types;
using PackItUp.Types;
using Serilog;
using Serilog.Core;
using File = System.IO.File;
using Version = Modrinth.Models.Version;

namespace PackItUp.ModpackProvider.Providers;

public class ModrinthModpackProvider : ModpackProvider
{
    private readonly ModrinthClient _client;

    private Modrinth.Models.Version[] _versions = [];

    protected override PackwizExportType ExportType => PackwizExportType.Modrinth;

    public ModrinthModpackProvider(string id, Token token, List<string> packFolders) : base(id, token, packFolders, Log.ForContext("SourceContext", $"PackItUp/ModrinthModpackProvider:{id}"))
    {
        string? t = token?.GetToken();
        if (string.IsNullOrWhiteSpace(t))
            throw new NullReferenceException($"Token not set in provider with ID {this.Id}");

        _client = new ModrinthClient(new ModrinthClientConfig()
        {
            ModrinthToken = t,
            UserAgent = Constants.UserAgent
        });
    }

    /// <inheritdoc />
    public override async Task<bool> InitializeAsync()
    {
        foreach (string packFolder in this.PackFolders)
        {
            string path = Path.Combine(packFolder, "pack.toml");

            if (!File.Exists(path))
            {
                Logger.Warning("No pack.toml found for modpack provider with mod ID '{PackName}' in directory {Path}", this.Id, packFolder);
                continue;
            }

            PackwizManifest m = Toml.ReadFile<PackwizManifest>(path);
            if (m == null)
            {
                Logger.Warning("Failed to deserialize pack for modpack provider with mod ID '{PackName}' with path {Path}", this.Id, path);
                continue;
            }

            // TODO is tuple like this slow?
            this.Manifests.Add((new PackwizPackManager(packFolder, m), m.Versions.GetSupportedLoaders()));
        }

        Project proj;
        try
        {
            proj = await _client.Project.GetAsync(Id);
        }
        catch (ModrinthApiException ex)
        {
            if (ex.Response?.StatusCode == HttpStatusCode.NotFound)
            {
                Logger.Error("Project ID {Id} was not found on the provider, cannot process this modpack.", Id);
                return false;
            }

            throw;
        }


        this._versions = await _client.Version.GetMultipleAsync(proj.Versions);

        if (!PackItUp.Instance.CommandLineConfig.ShouldForceUploadAll)
        {
            foreach (var mf in this.Manifests)
            {
                if (mf.pack.Manifest.Version is null)
                {
                    Logger.Warning("No version present");
                    continue;
                }

                // 5000 mile long indents
                var mrVersions = _versions.Where(v =>
                                                          v.GameVersions.Contains(mf.pack.Manifest.Versions
                                                             .MinecraftVersion) &&
                                                          v.Loaders.All(l =>
                                                                            PackwizVersions.GetLoader(l) is
                                                                                { } gl && mf
                                                                               .supportedLoaders
                                                                               .Contains(gl))
                                                     );

                string f = ManifestPlaceholderFormatter.Format(VersionPlaceholder, mf.pack.Manifest);
                if (mrVersions.Any(v => v.VersionNumber == f)) continue;

                Logger.Information("Pack with version {Version} (Minecraft {MCVer}) eligible for upload", f, mf.pack.Manifest.Versions.MinecraftVersion);
                this.ManifestsEligible.Add(mf);
            }
        }
        else
        {
            this.ManifestsEligible = this.Manifests;
        }

        Logger.Information("{Amount} packs are eligible for upload", this.ManifestsEligible.Count);
        return true;
    }

    /// <inheritdoc />
    public override async Task UploadEligibleAsync()
    {
        foreach (var exp in ManifestsExported)
        {
            UploadableFile f = new UploadableFile(exp.exportedPath);

            IEnumerable<string> loaders = exp.pack.Manifest.Versions.GetSupportedLoaders().Where(l => l is not null).Select(l => PackwizVersions.GetLoaderId(l!));

            string versionName = ManifestPlaceholderFormatter.Format(this.ProviderNamePlaceholder, exp.pack.Manifest);
            string versionNumber = ManifestPlaceholderFormatter.Format(this.ProviderVersionPlaceholder, exp.pack.Manifest);

            Logger.Information("Uploading version {VersionNumber} with title {VersionName}", versionNumber, versionName);

            // TODO this is VERY messy, we need to do something about this.
            // I was thinking about using SpectreConsole.CLI but I had a bad experience with integrating SpectreConsole into here while using Serilog.
            string? changelogPath = Path.Combine(exp.pack.PackItUpLocalDirectory, "Changelogs", $"{versionNumber}.md");
            while (changelogPath == null || !File.Exists(changelogPath)) {
                Logger.Warning("No changelog has been created for this version (expected at path {ChangelogPath})", changelogPath);
                Logger.Information("Options:\n - Skip uploading this version (s)\n - Rescan for the changelog (r)\n - Continue with no changelog (c)");
                Logger.Information("Choose an option (s/r/c): ");

                string? input = Console.ReadLine();
                if (string.IsNullOrEmpty(input)) {
                    Logger.Error("Invalid option, please try again.");
                    continue;
                }

                switch(input) {
                    case "s": return;
                    case "r": continue;
                    case "c": changelogPath = null; break;
                    default:
                        Logger.Error("Invalid option, please try again.");
                        break;
                }
            }

            string changelog = changelogPath != null && File.Exists(changelogPath) ? File.ReadAllText(changelogPath) : "";
            Version v = await _client.Version.CreateAsync(Id, [f], f.FileName, versionName, versionNumber,
                                                          changelog, [], [exp.pack.Manifest.Versions.MinecraftVersion], (exp.pack.Manifest.PackItUp?.Stage ?? PackStage.Release).GetModrinth(), loaders,
                                              false, VersionStatus.Listed, null);
        }

    }
}