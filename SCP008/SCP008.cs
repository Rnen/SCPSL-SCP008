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
		version = assemblyVersion + "-1",
		SmodMajor = 3,
		SmodMinor = 9,
		SmodRevision = 9
		)]
	public partial class SCP008 : Plugin
	{
		/// <summary>
		/// The current <see cref="SCP008"/> plugin version
		/// </summary>
		public const string assemblyVersion = "1.7";

		public string TranslationFileName => this.Details.name + "-Translations";

		internal static List<string> infected = new List<string>();

		internal static string patientZero = "";

		internal static Player[] InfectedPlayers
		{
			get
			{
				return plugin.Server.GetPlayers(p=> infected.Contains(p.UserId)).ToArray();
			}
		}
		internal static Player[] InfectedZombies
		{
			get
			{
				return plugin.Server.GetPlayers(Smod2.API.RoleType.SCP_049_2).Where(s => infected.Contains(s.UserId)).ToArray();
			}
		}

		internal static int roundCount = 0;

		internal static SCP008 plugin;

		internal static Queue<WaitForTeleport> waitForTeleport = new Queue<WaitForTeleport>();


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
			cassieAnnouncements = "scp008_cassie_announcements",
			announceRequire049ConfigKey = "scp008_announcement_count049",
			anyDeathCounts = "scp008_anyDeath",
			assist079EXP = "scp008_assist079_experience",
			personalHint = "scp008_hint",
			personalHintDuration = "scp008_hint_duration",
			separateVanillaZombies = "scp008_separate_vanilla_zombies",
			usePatientZero = "scp008_patient_zero",
			startwithPatientZero = "usePatientZeroOnStart";
		#endregion

		public override void OnDisable()
		{
			this.IsEnabled = false;
		}

		public override void OnEnable()
		{
			SCP008.plugin = this;
		}

		public override void Register()
		{
			#region EventRegister
			this.AddEventHandlers(new EventHandlers(this), Smod2.Events.Priority.Low);
			#endregion

			#region CommandRegister
			this.AddCommands(new string[] { "scp008", "scp08", "scp8" }, new Command.EnableDisableCommand(this));
			this.AddCommands(new string[] { "infect" }, new Command.InfectCommand(this));
			this.AddCommands(new string[] { "008help", "scp008help", "scp8help" }, new Command.HelpCommand());
			#endregion

			#region ConfigRegister
			this.AddConfig(new Smod2.Config.ConfigSetting(enableConfigKey, true, true, "Enable/Disable plugin"));
			this.AddConfig(new Smod2.Config.ConfigSetting(canHitTutConfigKey, true, true, "If zombies can hit TUTORIAL players or not"));
			this.AddConfig(new Smod2.Config.ConfigSetting(cassieAnnouncements, true, true, "If CASSIE server announcements are enabled or not"));
			this.AddConfig(new Smod2.Config.ConfigSetting(announceRequire049ConfigKey, false, true, "If server require 049 to be dead for announcement"));
			this.AddConfig(new Smod2.Config.ConfigSetting(separateVanillaZombies, false, true, "If regular zombies made by 049 should not be counted"));
			this.AddConfig(new Smod2.Config.ConfigSetting(usePatientZero, false, true, "Enabled Patient zero mode, which cannot be cured"));
			this.AddConfig(new Smod2.Config.ConfigSetting(startwithPatientZero, false, true, "Spawns a patient zero if 049 does not spawn"));


			this.AddConfig(new Smod2.Config.ConfigSetting(ranksAllowedConfigKey, new string[0], true, "What ranks are allowed to run the commands of the plugin"));
			this.AddConfig(new Smod2.Config.ConfigSetting(rolesCanBeInfectedConfigKey, new int[] { -1 }, true, "What roles can be infected"));
			this.AddConfig(new Smod2.Config.ConfigSetting(anyDeathCounts, false, true, "If any death cause while infected should convert"));

			this.AddConfig(new Smod2.Config.ConfigSetting(damageAmountConfigKey, 1f, true, "Amount of damage per interval."));
			this.AddConfig(new Smod2.Config.ConfigSetting(damageIntervalConfigKey, 2, true, "The interval at which to apply damage."));
			this.AddConfig(new Smod2.Config.ConfigSetting(swingDamageConfigKey, 0, true, "The damage applied on swing."));
			this.AddConfig(new Smod2.Config.ConfigSetting(assist079EXP, 35f, true, "Amount of EXP to award 079 for assisting"));

			this.AddConfig(new Smod2.Config.ConfigSetting(personalHint, true, true, "If people should be notified via a hint on convert"));
			this.AddConfig(new Smod2.Config.ConfigSetting(personalHintDuration, 7f, true, "Personal hint duration"));

			this.AddConfig(new Smod2.Config.ConfigSetting(infectKillChanceConfigKey, 100, true, "Infection Chance on zombie kill"));
			this.AddConfig(new Smod2.Config.ConfigSetting(infectChanceConfigKey, 100, true, "Infection Chance"));
			this.AddConfig(new Smod2.Config.ConfigSetting(cureChanceConfigKey, 100, true, "Cure chance of medpacks"));

			//this.AddConfig(new Smod2.Config.ConfigSetting("scp008_spawn_room", string.Empty, true, "The room ID that scp008 will spawn."));
			#endregion

			this.AddTranslation(new Smod2.Lang.LangSetting("youAre008", "You are now SCP008! Infect others with melee", TranslationFileName));
			this.AddTranslation(new Smod2.Lang.LangSetting("youAreInfected", "You have been infected by SCP008!", TranslationFileName));
			this.AddTranslation(new Smod2.Lang.LangSetting("youAreInfectedZero", "You have been infected by SCP008! You are patient zero.", TranslationFileName));
			this.AddTranslation(new Smod2.Lang.LangSetting("youAreInfectedSCP", "You have been infected by SCP008! Spread it with melee!", TranslationFileName));
			this.AddTranslation(new Smod2.Lang.LangSetting("youAreNotInfected", "You have been cured of SCP008!", TranslationFileName));
			this.AddTranslation(new Smod2.Lang.LangSetting("youAreNotInfectedZero", "As patient zero, you cannot be cured of SCP008!", TranslationFileName));
		}

		public new void Debug(string message)
		{
#if DEBUG
			plugin.Info("[SCP008]: " + message);
#endif
		}
	}
}