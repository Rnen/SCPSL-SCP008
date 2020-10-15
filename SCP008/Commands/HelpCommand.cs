using Smod2.Commands;
using SCP008PLUGIN;
using System;
using Smod2.API;
using System.Collections.Generic;
using System.Linq;

namespace SCP008PLUGIN.Command
{
	/// <summary>
	/// Command template for the <see cref="SCP008"/> <see cref="Smod2.Plugin"/>
	/// </summary>
	class HelpCommand : ICommandHandler
	{
		public string GetCommandDescription() => "Opens the SCP-008 GitHub page";

		public string GetUsage() => "scp008help";

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (sender is Player p && p != null)
				return new string[] { "This command is only for use in server window!" };
			else
			{
				try
				{
					System.Diagnostics.Process.Start("https://github.com/Rnen/SCP008");
					return new string[] { "Opening browser..." };
				}
				catch { return new string[] { "Could not open browser!" }; }
			}
		}
	}
}
