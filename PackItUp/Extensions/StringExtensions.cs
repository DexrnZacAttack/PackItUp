namespace PackItUp.Extensions;

public static class StringExtensions
{
    public static string ResolvePath(string path)
        => Path.GetFullPath(Path.IsPathRooted(path)
                                    ? path
                                    : Path.Combine(PackItUp.Instance?.RootDir ?? ".", path));
}