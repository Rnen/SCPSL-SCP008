using Smod2.Commands;
using SCP008PLUGIN;
using System;
using Smod2.API;
using System.Collections.Generic;
using System.Linq;

namespace SCP008PLUGIN.Command
{
	/// <summary>
	/// Command for infecting <see cref="Player"/>s
	/// </summary>
	class InfectCommand : ICommandHandler
	{
		private readonly SCP008 plugin;
		private Server Server => plugin.PluginManager.Server;

		public InfectCommand(SCP008 plugin) => this.plugin = plugin;
		public string GetCommandDescription() => "Infects / removes infection";
		public string GetUsage() => "INFECT (PLAYER) [SUBCOMMAND]";

		public string[] SubCommands = new string[] { "+ / add", "infect+ / ++ / zero / patientzero", "cure / remove / -" };

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
				if (args.Length == 0 && sender is Player p && p != null)
					try
					{
						if (SCP008.infected.Contains(p.UserID))
						{
							Utility.CureInfection(p, true);
							return new string[] { "Cured infected " + p.Name };
						}
						else
						{
							Utility.Infect(p);
							return new string[] { "Infected " + p.Name };
						}
					}
					catch (Exception e)
					{
						return new string[] { "Infect command exception " + e };
					}
				else if (args.Length > 0)
				{
					string arg1 = (args.Length > 1 && !string.IsNullOrEmpty(args[1])) ? args[1].ToLower() : "";
					List<Player> players = new List<Player>();
					switch (args[0].ToLower())
					{
						case "all":
						case "*":
							players = Server.GetPlayers(i => i.PlayerRole.Team != TeamType.SPECTATOR || i.PlayerRole.Team != TeamType.NONE);
							break;
						case "list":
						case "infected":
							return new string[] { "Infected Players:", string.Join("\n", SCP008.InfectedPlayers.OrderBy(s => s.Name).Select(i => i.Name).ToList()) };
						case "help":
						case "subcommand":
						case "subcommands":
							return new string[] { GetUsage(), "Availabile subcommands:" + string.Join(" ,", SubCommands) };
						default:
							players = Server.GetPlayers(args[0]);
							break;
					}

					if (players == null || players.Count == 0)
						return new string[] { "No players on the server called " + args[0] };

					string ret = this.plugin.Details.id + " INFECT COMMAND ERROR";

					foreach (Player player in players)
						switch (arg1.ToLower())
						{
							case "+":
							case "add":
								Utility.Infect(player);
								break;
							case "++":
							case "add+":
							case "zero":
							case "patientzero":
								Utility.Infect(player, true);
								return new string[] { $"Made \"{player.Name}\" into Patient Zero!" };
							case "cure":
							case "remove":
							case "-":
								SCP008.patientZero = "";
								Utility.CureInfection(player);
								if (players.Count == 1)
									return new string[] { $"Cured \"{player.Name}\" from SCP008 infection!" };
								ret = $"Cured {players.Count} players from SCP008 infection!";
								break;
							case "":
							default:
								if (SCP008.infected.Contains(player.UserID))
									Utility.CureInfection(player);
								else
									Utility.Infect(player);
								break;
						}
					if (!string.IsNullOrEmpty(arg1))
						return new string[] { $"Used {arg1} on {players.Count} players!" };
					else
						return new string[] { $"Toggled infection on {players.Count} players!" };
				}
				else
					return new[] { GetUsage() };
			}
			else
				return new string[] { "You dont have the required permission to run " + GetUsage() };
		}
	}
}

