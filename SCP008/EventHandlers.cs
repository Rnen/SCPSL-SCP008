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
		IEventHandlerPlayerHurt, IEventHandlerPlayerDie, IEventHandlerMedkitUse, IEventHandlerUpdate, IEventHandlerCheckEscape
	{
		private SCP008 plugin;
		private Server server;

		bool IsEnabled => plugin.GetIsEnabled();

		int damageAmount = 2, 
			damageInterval = 1;
		List<int> rolesCanBecomeInfected = new List<int>();

		public EventHandlers(SCP008 plugin)
		{
			this.plugin = plugin;
			this.server = plugin.pluginManager.Server;
		}

		#region PlayerSpecific

		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			if (ev.Attacker.PlayerId == ev.Player.PlayerId || !IsEnabled) return;

			int damageAmount = (ev.Attacker.TeamRole.Role == Role.ZOMBIE) ? plugin.GetConfigInt(SCP008.swingDamageConfigKey) : 0;
			int infectChance = (ev.Attacker.TeamRole.Role == Role.ZOMBIE) ? plugin.GetConfigInt(SCP008.infectChanceConfigKey) : 0;
			int infectOnKillChance = (ev.Attacker.TeamRole.Role == Role.ZOMBIE && ev.Damage >= ev.Player.GetHealth()) ? plugin.GetConfigInt(SCP008.infectKillChanceConfigKey) : 0;

			if (ev.Player.TeamRole.Team == Smod2.API.Team.TUTORIAL && ev.Attacker.TeamRole.Role == Role.ZOMBIE
				&& !plugin.GetConfigBool(SCP008.canHitTutConfigKey))
			{ ev.Damage = 0f; return; }

			//Sets damage to config amount if above 0
			if (ev.Attacker.TeamRole.Role == Role.ZOMBIE && damageAmount > 0)
				ev.Damage = damageAmount;
			
			//When a zombie damages a player, adds them to list of infected players to damage
			if (IsEnabled && ev.Attacker.TeamRole.Role == Role.ZOMBIE
				&& !SCP008.playersToDamage.Contains(ev.Player.SteamId)
				&& infectChance > 0
				&& new Random().Next(1, 100) <= infectChance)
			{
				if( (rolesCanBecomeInfected == null || rolesCanBecomeInfected.Count == 0 || rolesCanBecomeInfected.FirstOrDefault() == -1) 
					|| 
					(rolesCanBecomeInfected.Count > 0 && rolesCanBecomeInfected.Contains((int)ev.Player.TeamRole.Role)))
					SCP008.playersToDamage.Add(ev.Player.SteamId);
			}

			//If a zombie kills a person with "zombiekill_infect" set to true, transform
			if(ev.Attacker.TeamRole.Role == Role.ZOMBIE
				&& plugin.GetConfigBool(SCP008.zombieKillInfectsConfigKey)
				&& ev.Damage >= ev.Player.GetHealth()
				&& new Random().Next(1, 100) <= infectOnKillChance)
			{
				ev.Damage = 0f;
				ChangeToSCP008(ev.Player);
			}
		}

		public void OnPlayerDie(PlayerDeathEvent ev)
		{
			//If player dies, removes them from infected list
			if (SCP008.playersToDamage.Contains(ev.Player.SteamId))
				SCP008.playersToDamage.Remove(ev.Player.SteamId);
		}

		public void OnMedkitUse(PlayerMedkitUseEvent ev)
		{
			if (!IsEnabled) return;
			bool cureEnabled = plugin.GetConfigBool(SCP008.cureEnabledConfigKey);
			int cureChance = (cureEnabled) ? plugin.GetConfigInt(SCP008.cureChanceConfigKey) : 0;
			//If its enabled in config and infected list contains player and cure chance is more than, cure.
			if (cureEnabled
				&& SCP008.playersToDamage.Contains(ev.Player.SteamId) 
				&& cureChance > 0 && cureChance >= new Random().Next(1,100))
				SCP008.playersToDamage.Remove(ev.Player.SteamId);
		}

		#endregion

		#region RoundHandlers

		public void OnRoundEnd(RoundEndEvent ev)
		{
			//Empties infected list
			SCP008.playersToDamage.Clear();
			//Duh.
			SCP008.roundCount++;
		}

		public void OnRoundStart(RoundStartEvent ev)
		{
			if (!IsEnabled) return;
			//Empties infected list
			SCP008.playersToDamage.Clear();

			if (!plugin.SCP008Dead())
				plugin.SetCanAnnounce(true);
			/* Poof's untested code
			string RoomID = plugin.GetConfigString("scp008_spawn_room");
			if (!string.IsNullOrEmpty(RoomID))
				foreach(Player p in ev.Server.GetPlayers().Where(p => p.TeamRole.Role == Role.SCP_049_2))
					plugin.pluginManager.CommandManager.CallCommand(server, "tproom", new string[] { p.PlayerId.ToString(), RoomID });
			*/
		}

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			//Reload theese configs on each round restart
			this.damageAmount = plugin.GetConfigInt(SCP008.damageAmountConfigKey);
			this.damageInterval = plugin.GetConfigInt(SCP008.damageIntervalConfigKey);
			this.rolesCanBecomeInfected = plugin.GetConfigIntList(SCP008.rolesCanBeInfectedConfigKey).ToList();
		}

		#endregion


		DateTime updateTimer = DateTime.Now;
		DateTime announementTimer = DateTime.Now;

		public void OnUpdate(UpdateEvent ev)
		{
			if (!IsEnabled) return;
			if(announementTimer < DateTime.Now)
			{
				announementTimer = plugin.GetCanAnnounce() ? DateTime.Now.AddSeconds(5) : DateTime.Now.AddSeconds(30);
				if(plugin.GetCanAnnounce() && plugin.SCP008Dead())
				{
					plugin.Debug("Before announce: \n" + "  - Can Announce: " + plugin.GetCanAnnounce() + "\n" + "  - 008 Exterminated: " + plugin.SCP008Dead());
					if(plugin.GetConfigBool(SCP008.announementsenabled))
						plugin.Server.Map.AnnounceScpKill("008");
					plugin.SetCanAnnounce(false);
					plugin.Debug("After announce: \n" + "  - Can Announce: " + plugin.GetCanAnnounce() + "\n" + "  - 008 Exterminated: " + plugin.SCP008Dead());
				}
			}

			if (updateTimer < DateTime.Now)
			{
				//Sets when the next time this code will run
				updateTimer = DateTime.Now.AddSeconds(damageInterval);

				//If the server isnt empty, run code on all players
				if (server.GetPlayers().Count > 0)
					foreach(Player p in server.GetPlayers())
					{
						//If the victim is human and the player is in the infected list
						if ((p.TeamRole.Team != Smod2.API.Team.SCP && p.TeamRole.Team != Smod2.API.Team.SPECTATOR) && SCP008.playersToDamage.Contains(p.SteamId))
						{
							//If the damage doesnt kill, deal the damage
							if (damageAmount < p.GetHealth())
								p.Damage(damageAmount, DamageType.SCP_049_2);
							else if (damageAmount >= p.GetHealth())
							{
								//If the damage kills the human, transform
								ChangeToSCP008(p);
							}
						}
					}
			}
		}

		public void OnCheckEscape(PlayerCheckEscapeEvent ev)
		{
			if (SCP008.playersToDamage.Contains(ev.Player.SteamId))
				SCP008.playersToDamage.Remove(ev.Player.SteamId);
		}

		internal void ChangeToSCP008(Player player)
		{
			if (SCP008.playersToDamage.Contains(player.SteamId))
				SCP008.playersToDamage.Remove(player.SteamId);
			Vector pos = player.GetPosition();
			player.ChangeRole(Role.SCP_049_2, spawnTeleport: false);
			player.Teleport(pos);
			plugin.SetCanAnnounce(true);
			plugin.Debug("Changed " + player.Name + " to SCP-008");
		}

	}
}