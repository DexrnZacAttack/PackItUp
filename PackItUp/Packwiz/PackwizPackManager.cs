using System.Diagnostics;
using System.Text;
using Nett;
using PackItUp.ModpackProvider;
using PackItUp.Packwiz.Extensions;
using PackItUp.Packwiz.Types;
using Serilog;

namespace PackItUp.Packwiz;

public class PackwizPackManager
{

    private readonly ILogger _logger;
    public readonly string ModpackDirectory;
    public readonly PackwizManifest Manifest;
    public readonly string PackItUpLocalDirectory;

    public PackwizPackManager(string modpackDirectory, PackwizManifest manifest)
    {
        this.ModpackDirectory = modpackDirectory;
        this.Manifest = manifest;
        this.PackItUpLocalDirectory = Path.Combine(modpackDirectory, "PackItUp");

        this._logger = Log.ForContext("SourceContext", $"PackItUp/PackwizPackManager:{this.Manifest.Name} v{this.Manifest.Version ?? "Unknown"}");
    }

    public PackwizPackManager(string modpackDirectory) : this(modpackDirectory, Toml.ReadFile<PackwizManifest>(Path.Combine(modpackDirectory, "pack.toml")))
    {
    }

    public async Task<(int ExitCode, string? ExportedPath)> Export(string outputName, PackwizExportType exportType)
    {
        if (!Directory.Exists(ModpackDirectory))
            throw new DirectoryNotFoundException("Modpack directory not found");

        string exportsDir = Path.Combine(PackItUpLocalDirectory, "Exports");

        if (!Directory.Exists(exportsDir))
        {
            Directory.CreateDirectory(exportsDir);
        }

        string exp = Path.Combine(exportsDir, outputName + "." + exportType.GetExportFileExtension());

        Process builder = new();
        builder.StartInfo.FileName = PackItUp.Instance.CommandLineConfig.PackwizExec;
        builder.StartInfo.WorkingDirectory = ModpackDirectory;
        builder.StartInfo.Arguments = $"{exportType.GetString()} export -o \"{exp}\"";
        _logger.Debug(builder.StartInfo.Arguments);
        builder.StartInfo.RedirectStandardOutput = true;
        builder.StartInfo.RedirectStandardError = true;
        builder.StartInfo.UseShellExecute = false;

        ILogger logger = _logger.ForContext("SourceContext", $"PackItUp/PackwizPackManager:{this.Manifest.Name} v{this.Manifest.Version ?? "Unknown"} | Export");
        builder.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                logger.Information("{Msg}", e.Data);
        };

        builder.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                logger.Error("{Msg}", e.Data);
        };

        builder.Start();
        builder.BeginOutputReadLine();
        builder.BeginErrorReadLine();
        await builder.WaitForExitAsync();

        return builder.ExitCode != 0 ? (builder.ExitCode, null) : (builder.ExitCode, exp);
    }
}

