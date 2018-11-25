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
		private Plugin plugin;
		private Server server;

		int damageAmount = 2, 
			damageInterval = 1;
		List<int> rolesCanBecomeInfected = new List<int>();

		public EventHandlers(Plugin plugin)
		{
			this.plugin = plugin;
			this.server = plugin.pluginManager.Server;
		}

		#region PlayerSpecific

		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			if (ev.Attacker.PlayerId == ev.Player.PlayerId) return;

			int damageAmount = plugin.GetConfigInt(SCP008.swingDamageConfigKey);
			int infectChance = plugin.GetConfigInt(SCP008.infectChanceConfigKey);

			bool canHitTut = plugin.GetConfigBool(SCP008.canHitTutConfigKey);
			if (ev.Player.TeamRole.Team == Smod2.API.Team.TUTORIAL && ev.Attacker.TeamRole.Role == Role.ZOMBIE && !canHitTut) { ev.Damage = 0f; return; }

			//Sets damage to config amount if above 0
			if (ev.Attacker.TeamRole.Role == Role.SCP_049_2 && damageAmount > 0)
				ev.Damage = damageAmount;
			
			//When a zombie damages a player, adds them to list of infected players to damage
			if (SCP008.isEnabled && ev.Attacker.TeamRole.Role == Role.SCP_049_2
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
			if(ev.Attacker.TeamRole.Role == Role.SCP_049_2
				&& plugin.GetConfigBool(SCP008.zombieKillInfectsConfigKey)
				&& ev.Damage >= ev.Player.GetHealth())
			{
				ev.Damage = 0f;
				if(SCP008.playersToDamage.Contains(ev.Player.SteamId))
					SCP008.playersToDamage.Remove(ev.Player.SteamId);
				Vector pos = ev.Player.GetPosition();
				ev.Player.ChangeRole(Role.SCP_049_2, spawnTeleport: false);
				ev.Player.Teleport(pos);
				SCP008.canAnnounce = true;
			}
		}

		public void OnPlayerDie(PlayerDeathEvent ev)
		{
			//If settings require 049 to die, counts 049, if not returns false
			bool scp049alive = (plugin.GetConfigBool(SCP008.announceRequire049ConfigKey)) ? server.GetPlayers().Where(p => p.TeamRole.Role == Role.SCP_049).Count() > 0 : false;
			//If player dies, removes them from infected list
			if (SCP008.playersToDamage.Contains(ev.Player.SteamId))
				SCP008.playersToDamage.Remove(ev.Player.SteamId);
			//If no player is infected and no player is a zombie, and 049 is dead (if required by config), announce
			if (SCP008.playersToDamage.Count() < 1 && server.GetPlayers().Where(p => p.TeamRole.Role == Role.SCP_049_2).Count() < 1 && !scp049alive)
				if(SCP008.canAnnounce)
					plugin.Server.Map.AnnounceScpKill("008", ev.Killer);
		}

		public void OnMedkitUse(PlayerMedkitUseEvent ev)
		{
			int cureChance = plugin.GetConfigInt(SCP008.cureChanceConfigKey);
			//If its enabled in config and infected list contains player and cure chance is more than, cure.
			if (plugin.GetConfigBool(SCP008.cureEnabledConfigKey)
				&& SCP008.playersToDamage.Contains(ev.Player.SteamId) 
				&& cureChance > 0 && plugin.GetConfigInt(SCP008.cureChanceConfigKey) >= new Random().Next(1,100))
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
			//Empties infected list
			SCP008.playersToDamage.Clear();
			SCP008.canAnnounce = (server.GetPlayers().Where(p => p.TeamRole.Role == Role.SCP_049).Count() > 0 && plugin.GetConfigBool(SCP008.announceRequire049ConfigKey))
				|| server.GetPlayers().Where(p => p.TeamRole.Role == Role.SCP_049_2).Count() > 0;
			/* Poof's untested code
			string RoomID = plugin.GetConfigString("scp008_spawn_room");
			if (!string.IsNullOrEmpty(RoomID))
				foreach(Player p in ev.Server.GetPlayers().Where(p => p.TeamRole.Role == Role.SCP_049_2))
					plugin.pluginManager.CommandManager.CallCommand(server, "tproom", new string[] { p.PlayerId.ToString(), RoomID });
			*/
		}

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			//Checks enabled config on initial start
			if (SCP008.roundCount == 0)
				SCP008.isEnabled = plugin.GetConfigBool(SCP008.enableConfigKey);

			//Reload theese configs on each round restart
			this.damageAmount = plugin.GetConfigInt(SCP008.damageAmountConfigKey);
			this.damageInterval = plugin.GetConfigInt(SCP008.damageIntervalConfigKey);
			this.rolesCanBecomeInfected = plugin.GetConfigIntList(SCP008.rolesCanBeInfectedConfigKey).ToList();
		}

		#endregion


		DateTime updateTimer = DateTime.Now;

		public void OnUpdate(UpdateEvent ev)
		{
			if (SCP008.isEnabled && updateTimer < DateTime.Now)
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
								SCP008.playersToDamage.Remove(p.SteamId);
								Vector pos = p.GetPosition();
								p.ChangeRole(Role.SCP_049_2, spawnTeleport: false);
								p.Teleport(pos);
								SCP008.canAnnounce = true;
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
	}
}