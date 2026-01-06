using Nett;

namespace PackItUp.Packwiz.Types;

// https://packwiz.infra.link/reference/pack-format/pack-toml/#index
/// <summary>
/// Information about the index file in this modpack.
/// </summary>
public class PackwizIndexInfo
{
    /// <summary>
    /// The path to the file that contains the index.
    /// </summary>
    [TomlMember(Key = "file")]
    public required string File { get; set; }
    /// <summary>
    /// The hash format for the hash of the index file.
    /// </summary>
    [TomlMember(Key = "hash-format")]
    public required string HashFormat { get; set; }
}