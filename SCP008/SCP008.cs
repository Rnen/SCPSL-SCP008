using Smod2;
using Smod2.API;
using Smod2.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCP008PLUGIN
{
	/// <summary>
	/// The <see cref="SCP008"/> <see cref="Plugin"/>!
	/// </summary>
	[PluginDetails(
		author = "Evan",
		name = "SCP008",
		description = "Plugin that replicates SCP008 behaviour",
		id = "rnen.scp.008",
		version = assemblyVersion + "-0",
		SmodMajor = 3,
		SmodMinor = 5,
		SmodRevision = 0
		)]
	public partial class SCP008 : Plugin
	{
		/// <summary>
		/// The current <see cref="SCP008"/> plugin version
		/// </summary>
		public const string assemblyVersion = "1.6";

		public static string TranslationFileName => plugin.Details.name + "-Translations";

		internal static List<string> playersToDamage = new List<string>();
		internal static int roundCount = 0;

		static SCP008 plugin;


		#region ConfigKeys
		internal static readonly string
			enableConfigKey = "scp008_enabled",
			damageAmountConfigKey = "scp008_damage_amount",
			damageIntervalConfigKey = "scp008_damage_interval",
			swingDamageConfigKey = "scp008_swing_damage",
			infectChanceConfigKey = "scp008_infect_chance",
			infectKillChanceConfigKey = "scp008_killinfect_chance",
			cureChanceConfigKey = "scp008_cure_chance",
			ranksAllowedConfigKey = "scp008_ranklist_commands",
			rolesCanBeInfectedConfigKey = "scp008_roles_caninfect",
			canHitTutConfigKey = "scp008_canhit_tutorial",
			announementsenabled = "scp008_announcement_enabled",
			announceRequire049ConfigKey = "scp008_announcement_count049",
			anyDeathCounts = "scp008_anyDeath",
			assist079EXP = "scp008_assist079_experience",
			personalBroadcast = "scp008_broadcast",
			personalBroadcastDuration = "scp008_broadcast_duration";
		#endregion

		public override void OnDisable()
		{
			//Info(Details.name + " has been disabled.");
		}

		public override void OnEnable()
		{
			//this.Info(this.Details.name + " loaded successfully!");
			SCP008.plugin = this;
		}

		public override void Register()
		{
			#region EventRegister
			this.AddEventHandlers(new EventHandlers(this),Smod2.Events.Priority.Low);
			#endregion

			#region CommandRegister
			this.AddCommands(new string[] { "scp008", "scp08", "scp8" }, new Command.EnableDisableCommand(this));
			this.AddCommands(new string[] { "infect" }, new Command.InfectCommand(this));
			this.AddCommands(new string[] { "008help", "scp008help", "scp8help" }, new Command.HelpCommand());
			#endregion

			#region ConfigRegister
			this.AddConfig(new Smod2.Config.ConfigSetting(enableConfigKey, true, true, "Enable/Disable plugin"));
			this.AddConfig(new Smod2.Config.ConfigSetting(canHitTutConfigKey, true, true, "If zombies can hit TUTORIAL players or not"));
			this.AddConfig(new Smod2.Config.ConfigSetting(announementsenabled, false, true, "If server announcements are enabled or not"));
			this.AddConfig(new Smod2.Config.ConfigSetting(announceRequire049ConfigKey, false, true, "If server require 049 to be dead for announcement"));

			this.AddConfig(new Smod2.Config.ConfigSetting(ranksAllowedConfigKey, new string[0], true, "What ranks are allowed to run the commands of the plugin"));
			this.AddConfig(new Smod2.Config.ConfigSetting(rolesCanBeInfectedConfigKey, new int[] { -1 }, true, "What roles can be infected"));
			this.AddConfig(new Smod2.Config.ConfigSetting(anyDeathCounts, false, true, "If any death cause while infected should convert"));

			this.AddConfig(new Smod2.Config.ConfigSetting(damageAmountConfigKey, 1, true, "Amount of damage per interval."));
			this.AddConfig(new Smod2.Config.ConfigSetting(damageIntervalConfigKey, 2, true, "The interval at which to apply damage."));
			this.AddConfig(new Smod2.Config.ConfigSetting(swingDamageConfigKey, 0, true, "The damage applied on swing."));
			this.AddConfig(new Smod2.Config.ConfigSetting(assist079EXP, 35f, true, "Amount of EXP to award 079 for assisting"));

			this.AddConfig(new Smod2.Config.ConfigSetting(personalBroadcast, true, true, "If people should be notified via a personal broadcast on convert"));
			this.AddConfig(new Smod2.Config.ConfigSetting(personalBroadcastDuration, 7, true, "Personal broadcast duration"));

			this.AddConfig(new Smod2.Config.ConfigSetting(infectKillChanceConfigKey, 100, true, "Infection Chance on zombie kill"));
			this.AddConfig(new Smod2.Config.ConfigSetting(infectChanceConfigKey, 100, true, "Infection Chance"));
			this.AddConfig(new Smod2.Config.ConfigSetting(cureChanceConfigKey, 100, true, "Cure chance of medpacks"));

			this.AddConfig(new Smod2.Config.ConfigSetting("scp008_spawn_room", string.Empty, true, "The room ID that scp008 will spawn."));
			#endregion

			this.AddTranslation(new Smod2.Lang.LangSetting("youAre008", "You are now SCP008! Infect others with melee", TranslationFileName));
			this.AddTranslation(new Smod2.Lang.LangSetting("youAreInfected", "You have been infected by SCP008!", TranslationFileName));
		}
	}
}