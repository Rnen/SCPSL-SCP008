using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SCP008PLUGIN
{
	class EventHandlers : IEventHandlerRoundStart, IEventHandlerRoundEnd, IEventHandlerWaitingForPlayers,
		IEventHandlerPlayerHurt, IEventHandlerPlayerDie, IEventHandlerUpdate, IEventHandlerCheckEscape, IEventHandlerMedicalUse
	{
		private readonly SCP008 plugin;
		private Server Server => PluginManager.Manager.Server;

		bool IsEnabled => plugin.IsEnabled;

		float damageAmount = 2;
		int damageInterval = 1;
		List<int> rolesCanBecomeInfected = new List<int>();

		public EventHandlers(SCP008 plugin)
		{
			this.plugin = plugin;
		}

		#region PlayerSpecific

		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			if (ev.Attacker.PlayerID == ev.Player.PlayerID || !IsEnabled || (plugin.GetConfigBool(SCP008.separateVanillaZombies) && !ev.Attacker.IsInfected()))
				return;

			int damageAmount = (ev.Attacker.PlayerRole.RoleID == Smod2.API.RoleType.SCP_049_2) ? plugin.GetConfigInt(SCP008.swingDamageConfigKey) : 0;
			int infectChance = (ev.Attacker.PlayerRole.RoleID == Smod2.API.RoleType.SCP_049_2) ? plugin.GetConfigInt(SCP008.infectChanceConfigKey) : 0;
			int infectOnKillChance = (ev.Attacker.PlayerRole.RoleID == Smod2.API.RoleType.SCP_049_2 && ev.Damage >= (ev.Player.Health + ev.Player?.ArtificialHealth ?? 0)) ? plugin.GetConfigInt(SCP008.infectKillChanceConfigKey) : 0;

			if (ev.Player.PlayerRole.Team == Smod2.API.TeamType.TUTORIAL && ev.Attacker.PlayerRole.RoleID == Smod2.API.RoleType.SCP_049_2
				&& !plugin.GetConfigBool(SCP008.canHitTutConfigKey))
			{
				ev.Damage = 0f;
				return;
			}

			//Sets damage to config amount if above 0
			if (ev.Attacker.PlayerRole.RoleID == Smod2.API.RoleType.SCP_049_2 && damageAmount > 0)
				ev.Damage = damageAmount;

			//When a zombie damages a player, adds them to list of infected players to damage
			if (IsEnabled && ev.Attacker.PlayerRole.RoleID == Smod2.API.RoleType.SCP_049_2
				&& !SCP008.infected.Contains(ev.Player.UserID)
				&& infectChance > 0
				&& new Random().Next(1, 100) <= infectChance)
			{
				if ((rolesCanBecomeInfected == null || rolesCanBecomeInfected.Count == 0 || rolesCanBecomeInfected.FirstOrDefault() == -1)
					||
					(rolesCanBecomeInfected.Count > 0 && rolesCanBecomeInfected.Contains((int)ev.Player.PlayerRole.RoleID)))
					Utility.Infect(ev.Player);
			}

			if (ev.Attacker.PlayerRole.RoleID == Smod2.API.RoleType.SCP_049_2
				&& ev.Damage >= (ev.Player.Health + ev.Player?.ArtificialHealth ?? 0)
				&& (infectOnKillChance > 99 || new Random().Next(1, 100) <= infectOnKillChance))
			{
				ev.Damage = 0f;
				Utility.ChangeToSCP008(ev.Player);
			}
		}

		public void OnPlayerDie(PlayerDeathEvent ev)
		{
			//If player dies, removes them from infected list
			if (SCP008.infected.Contains(ev.Player.UserID))
			{
				if (plugin.GetConfigBool(SCP008.anyDeathCounts))
				{
					ev.SpawnRagdoll = false;
					Utility.ChangeToSCP008(ev.Player, true);
				}
				else
					SCP008.infected.Remove(ev.Player.UserID);
			}
		}

		public void OnMedicalUse(PlayerMedicalUseEvent ev)
		{
			if (!IsEnabled || ((int)ev.MedicalItem != (int)Smod2.API.ItemType.MEDKIT && (int)ev.MedicalItem != (int)Smod2.API.ItemType.SCP500))
				return;
			int cureChance = plugin.GetConfigInt(SCP008.cureChanceConfigKey);
			//If its enabled in config and infected list contains player and cure chance is more than, cure.
			if (SCP008.infected.Contains(ev.Player.UserID) && cureChance > 0
				&& cureChance >= new Random().Next(1, 100))
			{
				if (ev.Player.IsPatientZero())
				{
					if (plugin.GetConfigBool(SCP008.personalHint))
						ev.Player.ShowHint(plugin.GetTranslation("youAreNotInfectedZero"), plugin.GetConfigFloat(SCP008.personalHintDuration));
					return;
				}
				Utility.CureInfection(ev.Player);
				if (plugin.GetConfigBool(SCP008.personalHint))
					ev.Player.ShowHint(plugin.GetTranslation("youAreNotInfected"), plugin.GetConfigFloat(SCP008.personalHintDuration));
			}
		}

		#endregion

		#region RoundHandlers

		public void OnRoundEnd(RoundEndEvent ev)
		{
			//Empties infected list
			SCP008.infected.Clear();
			//Duh.
			SCP008.roundCount++;
		}

		public void OnRoundStart(RoundStartEvent ev)
		{
			if (!IsEnabled) 
				return;
			//Empties infected list
			SCP008.infected.Clear();

			if (!plugin.SCP008Dead)
				plugin.CanAnnounceDead = true;

			if (ev.Server.GetPlayers(Smod2.API.RoleType.SCP_049).Count < 1 && plugin.GetConfigBool(SCP008.usePatientZero) && plugin.IsEnabledZeroOnStart)
			{
				var rnd = new System.Random();
				var players = Server.GetPlayers(p => p.PlayerRole.Team == TeamType.D_CLASS || p.PlayerRole.Team == TeamType.SCIENTIST);
				if (players.Count < 1)
					return;
				var pl = players[rnd.Next(0, players.Count - 1)];
				Utility.Infect(pl, true);
			}
		}

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			//Reload theese configs on each round restart
			this.damageAmount = plugin.GetConfigFloat(SCP008.damageAmountConfigKey);
			this.damageInterval = plugin.GetConfigInt(SCP008.damageIntervalConfigKey);
			this.rolesCanBecomeInfected = plugin.GetConfigIntList(SCP008.rolesCanBeInfectedConfigKey).ToList();
			this.announcementEnabled = plugin.GetConfigBool(SCP008.cassieAnnouncements);
		}

		#endregion


		DateTime updateTimer = DateTime.Now;
		DateTime announementTimer = DateTime.Now;
		DateTime teleportTimer = DateTime.Now;
		bool announcementEnabled = true;

		public void OnUpdate(UpdateEvent ev)
		{
			if (!IsEnabled)
				return;
			#region Announcements
			if (announcementEnabled && announementTimer < DateTime.Now)
			{
				announementTimer = plugin.CanAnnounceDead ? DateTime.Now.AddSeconds(5) : DateTime.Now.AddSeconds(30);
				if (plugin.CanAnnounceDead && plugin.SCP008Dead)
				{
					plugin.Debug("Before announce: \n" + "  - Can Announce: " + plugin.CanAnnounceDead + "\n" + "  - 008 Exterminated: " + plugin.SCP008Dead);
					plugin.Server.Map.AnnounceScpKill(Smod2.API.RoleType.NONE,"008");
					plugin.CanAnnounceDead = false;
					plugin.Debug("After announce: \n" + "  - Can Announce: " + plugin.CanAnnounceDead + "\n" + "  - 008 Exterminated: " + plugin.SCP008Dead);
				}
			}
			#endregion

			if (updateTimer < DateTime.Now)
			{
				//Sets when the next time this code will run
				updateTimer = DateTime.Now.AddSeconds(damageInterval);

				//If the server isnt empty, run code on all players
				if (Server.NumPlayers > 0)
					foreach (Player player in Server.GetPlayers(p => p.PlayerRole.Team != TeamType.SCP))
					{
						//If the victim is human and the player is in the infected list
						if ((player.PlayerRole.Team != Smod2.API.TeamType.SPECTATOR) && SCP008.infected.Contains(player.UserID))
						{
							//If the damage doesnt kill, deal the damage
							if (damageAmount < Math.Round((player.Health + player?.ArtificialHealth ?? 0)) && player.Health > 1)
								player.Damage("Died to an SCP-008 infection.", damageAmount);
							else 
							{
								//If the damage kills the human, transform
								Utility.ChangeToSCP008(player);
							}
						}
					}
			}

			if (SCP008.waitForTeleport.Count > 0 && teleportTimer < DateTime.UtcNow)
			{
				//Sets when the next time this code will run
				teleportTimer = DateTime.UtcNow.AddSeconds(2);
				if(SCP008.waitForTeleport.Peek().triggerTime <= DateTime.UtcNow)
				{
					var t = SCP008.waitForTeleport.Dequeue();
					var p = Server.GetPlayers(t.UserID).FirstOrDefault();
					if(p != null)
					{
						p.Teleport(t.location);
					}
				}
			}
		}

		public void OnCheckEscape(PlayerCheckEscapeEvent ev) => Utility.CureInfection(ev.Player);

	}
}