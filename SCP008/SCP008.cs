using Smod2;
using Smod2.Attributes;
using System.Collections.Generic;

namespace SCP008PLUGIN
{
	[PluginDetails(
		author = "Evan | PoofImaFox",
		name = "SCP008",
		description = "Plugin that replicates SCP008 behaviour",
		id = "rnen.scp.008",
		version = pluginVersion,
		SmodMajor = 3,
		SmodMinor = 1,
		SmodRevision = 22
		)]
	public class SCP008 : Plugin
	{
		public const string pluginVersion = "1.2";

		public static List<string> playersToDamage = new List<string>();
		public static bool isEnabled = true;
		public static int roundCount = 0;

		#region ConfigKeys
		public const string
			enableConfigKey = "scp008_enabled",
			damageAmountConfigKey = "scp008_damage_amount",
			damageIntervalConfigKey = "scp008_damage_interval",
			swingDamageConfigKey = "scp008_swing_damage",
			zombieKillInfectsConfigKey = "scp008_zombiekill_infects",
			infectChanceConfigKey = "scp008_infect_chance",
			cureEnabledConfigKey = "scp008_cure_enabled",
			cureChanceConfigKey = "scp008_cure_chance",
			ranksAllowedConfigKey = "scp008_ranklist_commands",
			rolesCanBeInfected = "scp008_roles_caninfect";
		#endregion

		public override void OnDisable() => this.Info(this.Details.name + " has been disabled.");

		public override void OnEnable() => this.Info(this.Details.name + " loaded successfully!");

		public override void Register()
		{
			#region EventRegister
			this.AddEventHandlers(new EventHandlers(this),Smod2.Events.Priority.Low);
			#endregion

			#region CommandRegister
			this.AddCommands(new string[] { "scp008", "scp08", "scp8" }, new Command.EnableDisableCommand(this));
			this.AddCommands(new string[] { "infect" }, new Command.InfectCommand(this));
			#endregion

			#region ConfigRegister
			this.AddConfig(new Smod2.Config.ConfigSetting(enableConfigKey, true, Smod2.Config.SettingType.BOOL, true, "Enable/Disable plugin"));
			this.AddConfig(new Smod2.Config.ConfigSetting(ranksAllowedConfigKey, new string[] { }, Smod2.Config.SettingType.LIST, true, "What ranks are allowed to run the commands of the plugin"));
			this.AddConfig(new Smod2.Config.ConfigSetting(rolesCanBeInfected, new int[] { -1 }, Smod2.Config.SettingType.NUMERIC_LIST, true, "What roles can be infected"));

			this.AddConfig(new Smod2.Config.ConfigSetting(damageAmountConfigKey, 1, Smod2.Config.SettingType.NUMERIC, true, "Amount of damage per interval."));
			this.AddConfig(new Smod2.Config.ConfigSetting(damageIntervalConfigKey, 2, Smod2.Config.SettingType.NUMERIC, true, "The interval at which to apply damage."));
			this.AddConfig(new Smod2.Config.ConfigSetting(swingDamageConfigKey, 0, Smod2.Config.SettingType.NUMERIC, true, "The damage applied on swing."));

			this.AddConfig(new Smod2.Config.ConfigSetting(zombieKillInfectsConfigKey, false, Smod2.Config.SettingType.BOOL, true, "If regular kills by zombies should infect"));
			this.AddConfig(new Smod2.Config.ConfigSetting(infectChanceConfigKey, 100, Smod2.Config.SettingType.NUMERIC, true, "Infection Chance"));
			this.AddConfig(new Smod2.Config.ConfigSetting(cureEnabledConfigKey, true, Smod2.Config.SettingType.BOOL, true, "If medpacks can stop the infection"));
			this.AddConfig(new Smod2.Config.ConfigSetting(cureChanceConfigKey, 100, Smod2.Config.SettingType.NUMERIC, true, "Cure chance of medpacks"));

			this.AddConfig(new Smod2.Config.ConfigSetting("scp008_spawn_room", string.Empty, Smod2.Config.SettingType.STRING, true, "The room ID that scp008 will spawn."));
			#endregion
		}
	}
}