using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Smod2.API;

namespace SCP008PLUGIN
{
	internal struct WaitForTeleport
	{
		internal string userID;
		internal Vector location;
		internal DateTime triggerTime;
	}
	public static class Utility
	{
		internal static void Infect(Player player, bool isPatientZero = false)
		{
			if (isPatientZero)
				SCP008.patientZero = player.UserId;
			if (player == null || player.IsInfected() || player.TeamRole.Team == TeamType.NONE)
				return;
			SCP008.infected.Add(player.UserId);
			if (SCP008.PersonalBroadcastConfig)
				if (player.TeamRole.Team == TeamType.SCP)
					player.ShowHint(SCP008.plugin.GetTranslation("youAreInfectedSCP"), (float)SCP008.plugin.GetConfigFloat(SCP008.personalHintDuration));
				else
				{
					if (SCP008.plugin.GetConfigBool(SCP008.usePatientZero) && isPatientZero)
						player.ShowHint(SCP008.plugin.GetTranslation("youAreInfectedZero"), SCP008.plugin.GetConfigFloat(SCP008.personalHintDuration));
					else
						player.ShowHint(SCP008.plugin.GetTranslation("youAreInfected"), SCP008.plugin.GetConfigFloat(SCP008.personalHintDuration));
				}
		}

		internal static void CureInfection(Player player, bool forceCure = false)
		{
			if (player == null || !SCP008.infected.Contains(player.UserId))
				return;
			if (SCP008.plugin.GetConfigBool(SCP008.usePatientZero) && player.IsPatientZero() && !forceCure)
			{
				if (SCP008.PersonalBroadcastConfig)
					player.ShowHint(SCP008.plugin.GetTranslation("youAreInfectedZero"), SCP008.plugin.GetConfigFloat(SCP008.personalHintDuration));
				return;
			}
			SCP008.infected.Remove(player.UserId);
			SCP008.patientZero = "";
			if (SCP008.PersonalBroadcastConfig)
				player.ShowHint(SCP008.plugin.GetTranslation("youAreNotInfected"), SCP008.plugin.GetConfigFloat(SCP008.personalHintDuration));
		}

		internal static void ChangeToSCP008(Player player, bool isDead = false)
		{
			foreach (var scp in Scp079PlayerScript.instances)
				scp.AddExperience(SCP008.plugin.GetConfigFloat(SCP008.assist079EXP));
			if (!isDead && player.TeamRole.Team != TeamType.SPECTATOR)
			{
				SCP008.waitForTeleport.Enqueue(new WaitForTeleport { userID = player.UserId, location = player.GetPosition(), triggerTime = DateTime.UtcNow.AddSeconds(1) });
				SCP008.plugin.Debug("Enqueued player " + player.Name + " for teleport");
				player.ChangeRole(Smod2.API.RoleType.SCP_049_2, spawnTeleport: false, spawnProtect: false, removeHandcuffs: true);
			}
			else
				player.ChangeRole(Smod2.API.RoleType.SCP_049_2, spawnTeleport: false, spawnProtect: false, removeHandcuffs: true);
			SCP008.plugin.CanAnnounceDead = true;
			SCP008.plugin.Debug("Changed " + player.Name + " to SCP-008");
			if (SCP008.plugin.GetConfigBool(SCP008.personalHint))
				player.ShowHint(SCP008.plugin.GetTranslation("youAre008"), SCP008.plugin.GetConfigFloat(SCP008.personalHintDuration));
			RoundSummary.changed_into_zombies++;
		}

		internal static bool TryParseCommandBool(string input, out bool output)
		{
			if (bool.TryParse(input, out output))
				return true;
			switch (input.ToLower().Trim())
			{
				case "enable":
				case "true":
				case "1":
				case "on":
					output = true;
					return true;
				case "disable":
				case "false":
				case "0":
				case "off":
					output = false;
					return true;
				default:
					break;
			}
			return false;
		}
	}
}
