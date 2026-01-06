using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PackItUp.Config.Types.Token;

[Description("A modpack provider auth token")]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(EnvToken), "Environment")]
[JsonDerivedType(typeof(InlineToken), "Inline")]
public abstract class Token
{
    public abstract string? GetToken();
}