using Smod2;
using Smod2.Attributes;
using System.Collections.Generic;

namespace SCP008PLUGIN
{
    //Needs testing
	[PluginDetails(
		author = "Evan | PoofImaFox",
		name = "SCP008",
		description = "Plugin that replicates SCP008 behaviour",
		id = "rnen.scp.008",
		version = "1.1",
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
			this.AddConfig(new Smod2.Config.ConfigSetting("scp008_enabled", true, Smod2.Config.SettingType.BOOL, true, "Enable/Disable plugin"));
			this.AddConfig(new Smod2.Config.ConfigSetting("scp008_damage_amount", 1, Smod2.Config.SettingType.NUMERIC, true, "Amount of damage per interval."));
			this.AddConfig(new Smod2.Config.ConfigSetting("scp008_damage_interval", 2, Smod2.Config.SettingType.NUMERIC, true, "The interval at which to apply damage."));
            this.AddConfig(new Smod2.Config.ConfigSetting("scp008_swing_damage", 0, Smod2.Config.SettingType.NUMERIC, true, "The damage applied on swing."));
            this.AddConfig(new Smod2.Config.ConfigSetting("scp008_spawn_room", "", Smod2.Config.SettingType.STRING, true, "The room ID that scp008 will spawn."));
        }
	}
}
