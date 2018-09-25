﻿using Smod2;
using Smod2.Attributes;
using Smod2.EventHandlers;
using Smod2.Events;
using System.Collections.Generic;
using System.Linq;

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
			this.Info("Example Plugin has loaded :)");
			this.Info("Config value: " + this.GetConfigString("test"));
		}

		public override void Register()
		{
			// Register Events
			this.AddEventHandlers(new EventHandlers(this));
			// Register Commands
			this.AddCommands(new string[] { "scp008", "scp8" }, new SCP008PLUGIN.Command.EnableDisableCommand(this));
			// Register config settings
			this.AddConfig(new Smod2.Config.ConfigSetting("scp008_enabled", true, Smod2.Config.SettingType.BOOL, true, "test"));
			this.AddConfig(new Smod2.Config.ConfigSetting("scp008_damage_amount", 1, Smod2.Config.SettingType.NUMERIC, true, "test"));
			this.AddConfig(new Smod2.Config.ConfigSetting("scp008_damage_interval", 2, Smod2.Config.SettingType.NUMERIC, true, "test"));
		}
	}
}