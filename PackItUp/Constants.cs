using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace PackItUp;

public static class Constants {
	public const string Name = "PackItUp";
	public static readonly string Version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion ?? "Unknown";
    public const string GitLink = "https://codeberg.org/Dexrn/PackItUp";

	public static readonly string UserAgent = $"{Name}/{Version} ({GitLink} | https://dexrn.me)";
	
	public const string ConsoleOutputTemplate =
		"{Timestamp:HH:mm:ss} [{Level:u3} | {SourceContext}] {Message:lj}{NewLine}{Exception}";

	public const string FileOutputTemplate =
		"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3} | {SourceContext}] {Message:lj}{NewLine}{Exception}";

    public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.General)
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = JsonSerializerOptions.Default.TypeInfoResolver
    };
}