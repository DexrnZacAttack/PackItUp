namespace PackItUp.Config;

public class PackItUpCommandLineConfig
{
    public bool ShouldForceUploadAll { get; set; }= false;
    public bool ShouldExport { get; set; }= false;
    public bool ShouldUpload { get; set; }= false;
    public string? PackwizExec { get; set; }
}