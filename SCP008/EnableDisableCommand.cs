using Smod2.Commands;

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
			return "scp008 / scp8";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			SCP008.isEnabled = !SCP008.isEnabled;
			return new string[] { "SCP008 plugin set to " + SCP008.isEnabled };
		}
	}
}
