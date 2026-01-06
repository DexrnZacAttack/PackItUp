using System.ComponentModel;
using System.Text.Json.Serialization;
namespace PackItUp.Config.Types;

[Description("Modpack configuration")]
public class ModpackConfig
{
	[Description("Modpack name")]
	public required string Name { get; set; }
	
	[Description("Modpack providers to upload new versions to")]
	public required List<ModpackProvider.ModpackProvider> Providers { get; set; }
}