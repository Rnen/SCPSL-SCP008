using Smod2.Commands;
using SCP008PLUGIN;
using System;
using Smod2.API;
using System.Collections.Generic;
using System.Linq;

namespace SCP008PLUGIN.Command
{
	class TemplateCommand : ICommandHandler
	{
		private SCP008 plugin;
		public TemplateCommand(SCP008 plugin) => this.plugin = plugin;

		public string GetCommandDescription() => "This is a template";

		public string GetUsage() => "TEMPLATECOMMAND";

		bool IsAllowed(ICommandSender sender)
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
				else if (roleList == null || roleList.Count == 0)
					return true; // config was empty
				else
					return false;
			}
			else
				return true;
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (IsAllowed(sender))
			{
				//If is allowed to do stuff
				// DoStuff();
				return new string[] { "Ran " + GetUsage() + " command!" };
			}
			else
				return new string[] { "You dont have the required permission to run " + GetUsage() };
		}
	}
}
