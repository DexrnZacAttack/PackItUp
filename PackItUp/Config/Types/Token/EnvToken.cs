using System.ComponentModel;

namespace PackItUp.Config.Types.Token;

public class EnvToken : Token
{
    [Description("The environment variable name")]
    public required string Name { get; set; }

    public override string? GetToken()
    {
        return Environment.GetEnvironmentVariable(Name);
    }
}