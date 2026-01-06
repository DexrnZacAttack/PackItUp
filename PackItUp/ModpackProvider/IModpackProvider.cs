namespace PackItUp.ModpackProvider;

public interface IModpackProvider
{
    /// <summary>
    /// Initializes the provider
    /// </summary>
    /// <returns>True if the provider initialized successfully with eligible packs</returns>
    Task<bool> InitializeAsync();
    /// <summary>
    /// Exports all eligible packs to the filesystem
    /// </summary>
    Task ExportEligibleAsync();
    /// <summary>
    /// Uploads all eligible packs to the provider's remote.
    /// </summary>
    Task UploadEligibleAsync();
}