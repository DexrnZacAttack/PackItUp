using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using PackItUp.Config;
using PackItUp.Config.Types;
using PackItUp.Util;
using Serilog;

namespace PackItUp;

// WARNING: dirty, radioactive code.
// beware!
//
// TODO on a real note this should be cleaned up before full release, but for now we prototype the functionality.
internal class PackItUp {
	public static PackItUp Instance = null!;
	private readonly string RootDir;
	private readonly string ConfigPath;

    public readonly PackItUpCommandLineConfig CommandLineConfig;
	public readonly PackItUpConfig Config;
	
	private PackItUp(string path, PackItUpCommandLineConfig config) {
		RootDir = path;
		ConfigPath = Path.Combine(RootDir, "packitup.json");
        CommandLineConfig = config;

        // gen schema
		JsonNode schema = Constants.JsonOptions.GetJsonSchemaAsNode(typeof(PackItUpConfig), new JsonSchemaExporterOptions()
        {
            TransformSchemaNode = JsonSchemaOptions.TransformAddDescriptions
        });
		if (schema is JsonObject sc)
			sc.Insert(0, "$schema", "https://json-schema.org/draft/2020-12/schema");

        // write
		File.WriteAllText(Path.Combine(RootDir, "packitup.schema.json"), schema.ToString());

        // config file should always exist, Main enforces that.
        // however, if it doesn't for some reason, we check again here.
		if (!File.Exists(ConfigPath))
		    throw new FileNotFoundException($"Couldn't find config file at \"{ConfigPath}\"");

        // read conf
		using (FileStream fs = new(ConfigPath, FileMode.Open)) {
			PackItUpConfig? conf = JsonSerializer.Deserialize<PackItUpConfig>(fs, Constants.JsonOptions);
			if (conf == null)
				throw new NullReferenceException("Couldn't deserialize config");

            Config = conf;
        }
	}

    private async Task BeginAsync()
    {
        int updatable = 0;
        int providers = 0;

        Dictionary<ModpackConfig, List<ModpackProvider.ModpackProvider>> successful = [];

        foreach (ModpackConfig pack in Config.Modpacks)
        {
            foreach (ModpackProvider.ModpackProvider provider in pack.Providers)
            {
                bool success = await provider.InitializeAsync();
                if (!success)
                    continue;

                updatable += provider.ManifestsEligible.Count;
                providers++;

                if (!successful.ContainsKey(pack))
                    successful.Add(pack, []);

                successful[pack].Add(provider);
            }
        }

        Log.Information("{Count} packs across {ModpackCount} modpacks and {ProviderCount} providers are eligible for upload", updatable, successful.Count, providers);

        if (!CommandLineConfig.ShouldExport)
        {
            Log.Information("Done! Run PackItUp with --export or --upload to export eligible modpacks.");
            return;
        }

        foreach (var mp in successful)
        {
            Log.Information("Exporting packs in modpack {Name}", mp.Key.Name);
            foreach (ModpackProvider.ModpackProvider provider in mp.Value)
            {
                await provider.ExportEligibleAsync();
            }
        }

        if (!CommandLineConfig.ShouldUpload)
        {
            Log.Information("Done! Run PackItUp with --upload to upload eligible modpacks.");
            return;
        }

        foreach (var mp in successful)
        {
            Log.Information("Uploading packs in modpack {Name}", mp.Key.Name);
            foreach (ModpackProvider.ModpackProvider provider in mp.Value)
            {
                await provider.UploadEligibleAsync();
            }
        }

    }

    private static async Task<int> Main(string[] args) {
		Log.Logger = new LoggerConfiguration()
		            .WriteTo.Console(outputTemplate: Constants.ConsoleOutputTemplate)
		            .WriteTo.File($"Logs/{Constants.Name}-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log",
		                          retainedFileCountLimit: null,
		                          outputTemplate: Constants.FileOutputTemplate)
		            .MinimumLevel.Debug()
		            .CreateLogger()
		            .ForContext("SourceContext", typeof(PackItUp).Namespace);

        Log.Information("{Name} v{Version} | {GitLink}", Constants.Name, Constants.Version, Constants.GitLink);

        RootCommand root = new("Utility to streamline uploading Packwiz-powered modpack versions to supported modpack providers (such as Modrinth)");

        Option<bool> forceUploadOption = new("--force-upload-all")
        {
            Description = "Uploads all packs in specified providers regardless if the version has changed"
        };

        Option<bool> ignoreMissingChangelog = new("--ignore-missing-changelog")
        {
            Description = "Does not prompt for confirmation before uploading a pack release without a changelog"
        };

        Option<string> rootOption = new("--rootDir")
        {
            Description = "Root directory to read the config files from",
            HelpName = "path"
        };

        Option<bool> uploadOption = new("--upload")
        {
            Description = "Uploads eligible packs to the specified providers",
        };

        Option<bool> exportOption = new("--export")
        {
            Description = "Exports eligible packs to the filesystem. Always true when called with --upload.",
        };

        Option<string> packwizExec = new("--packwizExec")
        {
            Description = "Absolute path of the Packwiz executable to use. If not provided, defaults to packwiz found on path",
            HelpName = "path"
        };

        root.Options.Add(uploadOption);
        root.Options.Add(exportOption);
        root.Options.Add(rootOption);
        root.Options.Add(packwizExec);
        root.Options.Add(forceUploadOption);
        root.Options.Add(ignoreMissingChangelog);

        rootOption.Validators.Add(res =>
        {
            string? p = res.GetValue(rootOption);
            if (p == null) // we will use current dir
                return;

            if (!Directory.Exists(p))
                res.AddError($"Directory '{p}' does not exist");
        });

        packwizExec.Validators.Add(res =>
        {
            string? p = res.GetValue(packwizExec);
            if (p == null)
            {
                if (!PathUtils.ExecExistsOnPath("packwiz"))
                    res.AddError("Packwiz executable not found, you may need to specify it's path with --packwizExec <path>");

                return;
            }

            if (string.IsNullOrEmpty(p) || !File.Exists(p))
                res.AddError($"Could not find packwiz executable at provided path '{p}'");
        });

        root.SetAction(async parseResult =>
        {
            PackItUpCommandLineConfig clc = new()
            {
                ShouldExport = parseResult.GetValue(exportOption) || parseResult.GetValue(forceUploadOption) || parseResult.GetValue(uploadOption),
                ShouldUpload = parseResult.GetValue(forceUploadOption) || parseResult.GetValue(uploadOption),
                ShouldForceUploadAll = parseResult.GetValue(forceUploadOption),
                PackwizExec = parseResult.GetValue(packwizExec) ?? PathUtils.GetExecFromPath("packwiz")
            };

            Log.Information("Using packwiz found at path {Path}", clc.PackwizExec);

            // If user wants to use specific config folder, they can pass it as an arg.
            // Otherwise, we will use the current directory.
            string path = parseResult.GetValue(rootOption) ?? Directory.GetCurrentDirectory();

            // if it doesn't exist, create it.
            // We will still continue initialization since an empty config should never break the program's state.
            if (!File.Exists(Path.Combine(path, "packitup.json"))) {
                PackItUpConfig c = new();

                await using (FileStream fs = new(Path.Combine(path, "packitup.json"), FileMode.Create)) {
                    await JsonSerializer.SerializeAsync(fs, c, Constants.JsonOptions);

                    Log.Information("Created config at {Path}", fs.Name);
                }
            }

            // lights camera action
            Instance = new PackItUp(path, clc);
            await Instance.BeginAsync();

            return 0;
        });

        return await root.Parse(args).InvokeAsync();
    }
}