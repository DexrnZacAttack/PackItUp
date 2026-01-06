namespace PackItUp.Util;

public static class PathUtils
{
    public static bool ExistsOnPath(string filename)
    {
        return GetFromPath(filename) != null;
    }

    // :trolleyzoom:
    public static bool ExecExistsOnPath(string filename)
    {
        string fn = OperatingSystem.IsWindows() ? filename + ".exe" : filename;
        return GetFromPath(fn) != null;
    }

    public static string? GetExecFromPath(string filename)
        => GetFromPath(OperatingSystem.IsWindows() ? filename + ".exe" : filename);

    // jesus christ my ide really lobotomized this to hell
    public static string? GetFromPath(string filename)
        => File.Exists(filename)
               ? Path.GetFullPath(filename)
               : Environment.GetEnvironmentVariable("PATH")?
                  .Split(Path.PathSeparator)
                            .Select(path => Path.Combine(path, filename))
                            .FirstOrDefault(File.Exists);

}