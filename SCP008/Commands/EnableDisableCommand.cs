﻿using Smod2.Commands;
using SCP008PLUGIN;
using System;
using Smod2.API;
using System.Collections.Generic;
using System.Linq;

namespace SCP008PLUGIN.Command
{
	/// <summary>
	/// Command for enabling/disabling <see cref="SCP008"/>
	/// </summary>
	public class EnableDisableCommand : ICommandHandler
	{
		private readonly SCP008 plugin;

		public EnableDisableCommand(SCP008 plugin) => this.plugin = plugin;
		public string GetCommandDescription() => "Enables or disables " + plugin.Details.name;
		public string GetUsage() => "SCP008 (SUBCOMMAND)";

		public string[] SubCommands = new string[] { "(bool / enable / disable)", "zero / patientzero", "help / git" };

		private bool IsAllowed(ICommandSender sender)
		{
			//Checking if the ICommandSender is a player and setting the player variable if it is
			Player player = (sender is Player) ? sender as Player : null;

			//Checks if its a player, if not run the command as usual
			if (player != null)
			{
				//Making a list of all roles in config, converted to UPPERCASE
				string[] configList = plugin.GetConfigList(SCP008.ranksAllowedConfigKey);
				List<string> roleList = (configList != null && configList.Length > 0) ?
					configList.Select(role => role.ToUpper()).ToList() : new List<string>();

				//Checks if there is any entries, if empty, let anyone use it
				if (roleList != null && roleList.Count > 0
					&& (roleList.Contains(player.GetUserGroup().Name.ToUpper()) || roleList.Contains(player.GetRankName().ToUpper())))
				{
					//Config contained rank
					return true;
				}
				else if (roleList == null || roleList.Count == 0 || (roleList.Count == 1 && string.IsNullOrEmpty(roleList.First())))
					return true; // config was empty
				else
					return false; // config was not empty and didnt contain player role
			}
			else
				return true; // if command is run by server window
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (IsAllowed(sender))
			{
				if (args.Length > 0)
				{
					if (Utility.TryParseCommandBool(args[0], out bool b))
					{
						plugin.IsEnabled = b;
						return new string[] { "SCP008 plugin set to " + plugin.IsEnabled };
					}

					switch (args[0].ToUpper())
					{
						case "GIT":
						case "GITHUB":
							return plugin.PluginManager.CommandManager.CallCommand(sender, "008help", args);
						case "HELP":
							return new string[] { GetUsage(), "Subcommands: " + string.Join(" ,", SubCommands) };
						case "V":
						case "VERSION":
							return new string[] { "Plugin Version: " + plugin.Details.version, "SMod Version: " + Smod2.PluginManager.GetSmodVersion() };
						case "ZERO":
						case "PATIENTZERO":
							if(args.Length > 1 && Utility.TryParseCommandBool(args[1], out bool b2))
							{
								plugin.IsEnabledZeroOnStart = b2;
								return new string[] { "SCP008 PatientZero mode set to " + plugin.IsEnabledZeroOnStart};
							}
							else
							{
								plugin.IsEnabledZeroOnStart = !plugin.IsEnabledZeroOnStart;
								return new string[] { "SCP008 PatientZero mode set to " + plugin.IsEnabledZeroOnStart };
							}
					}
					return new string[] { GetUsage(), "Subcommands: " + string.Join(" ,", SubCommands) };
				}
				else
					plugin.IsEnabled = !plugin.IsEnabled; //If not just toggle

				//Returning the current state of the static bool
				return new string[] { "SCP008 plugin set to " + plugin.IsEnabled };
			}
			else
				return new string[] { "You dont have the required permission to run " + GetUsage() };
		}
	}
}
