using System.ComponentModel;
using System.Text.Json.Serialization;
using PackItUp.Config.Types;
namespace PackItUp.Config;

[Description("PackItUp configuration")]
public class PackItUpConfig {
	[JsonPropertyName("$schema")]
	public string Schema => "packitup.schema.json";
	
	[Description("List of modpacks for PackItUp to use")]
	public List<ModpackConfig> Modpacks { get; set; } = [];

    [Description("Delete changelog files after publish")]
    public bool DeleteChangelogs { get; set; } = false;
}