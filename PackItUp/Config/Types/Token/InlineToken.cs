using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PackItUp.Config.Types.Token;

public class InlineToken : Token
{
    [Description("The authentication token")]
    public required string Value { get; set; }

    public override string? GetToken()
    {
        return Value;
    }
}