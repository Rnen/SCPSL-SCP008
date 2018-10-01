using Smod2.Commands;
using SCP008PLUGIN;
using System;
using Smod2.API;
using System.Collections.Generic;
using System.Linq;

namespace SCP008PLUGIN.Command
{
	class EnableDisableCommand : ICommandHandler
	{
		private SCP008 plugin;
		public EnableDisableCommand(SCP008 plugin)
		{
			this.plugin = plugin;
		}

		public string GetCommandDescription()
		{
			return "Enables / disables " + plugin.Details.name;
		}

		public string GetUsage()
		{
			return "SCP008";
		}

		bool isAllowed(ICommandSender sender)
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
					&& (roleList.Contains(player.GetUserGroup().Name.ToUpper()) || roleList.Contains(player.GetRankName().ToUpper()) ) )
				{
					//Config contained rank
					return true;
				}
				else if (roleList == null || roleList.Count == 0 || (roleList.Count == 1 && string.IsNullOrEmpty(roleList.First()) ) )
					return true; // config was empty
				else
					return false; // config was not empty and didnt contain player role
			}
			else
				return true; // if command is run by server window
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (isAllowed(sender))
			{
				if (args.Length >= 1 && bool.TryParse(args[0], out bool value)) // If the command contains a arguement and it can be parsed as a bool
					SCP008.isEnabled = value;
				else 
					SCP008.isEnabled = !SCP008.isEnabled; //If not just toggle

				//Returning the current state of the static bool
				return new string[] { "SCP008 plugin set to " + SCP008.isEnabled };
			}
			else
				return new string[] { "You dont have the required permission to run " + GetUsage() };
		}
	}
}
