using Smod2;
using Smod2.Attributes;
using System.Collections.Generic;

namespace SCP008PLUGIN
{
	[PluginDetails(
		author = "Evan",
		name = "SCP008",
		description = "Plugin that replicates SCP008 behaviour",
		id = "rnen.scp.008",
		version = "1.0",
		SmodMajor = 3,
		SmodMinor = 1,
		SmodRevision = 18
		)]
	public class SCP008 : Plugin
	{
		public static List<string> playersToDamage = new List<string>();
		public static bool isEnabled = true;


		public override void OnDisable()
		{
		}

		public override void OnEnable()
		{
			this.Info( this.Details.name + " loaded successfully!");
		}

		public override void Register()
		{
			// Register Events
			this.AddEventHandlers(new EventHandlers(this));
			// Register Commands
			this.AddCommands(new string[] { "scp008", "scp08", "scp8" }, new Command.EnableDisableCommand(this));
			// Register config settings
			this.AddConfig(new Smod2.Config.ConfigSetting("scp008_enabled", true, Smod2.Config.SettingType.BOOL, true, "test"));
			this.AddConfig(new Smod2.Config.ConfigSetting("scp008_damage_amount", 1, Smod2.Config.SettingType.NUMERIC, true, "test"));
			this.AddConfig(new Smod2.Config.ConfigSetting("scp008_damage_interval", 2, Smod2.Config.SettingType.NUMERIC, true, "test"));
		}
	}
}
