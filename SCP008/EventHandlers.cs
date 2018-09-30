using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using System;

namespace SCP008PLUGIN
{
	class EventHandlers : IEventHandlerRoundStart, IEventHandlerRoundEnd, IEventHandlerWaitingForPlayers,
		IEventHandlerPlayerHurt, IEventHandlerPlayerDie, IEventHandlerMedkitUse, IEventHandlerUpdate
	{
		private Plugin plugin;
		private Server server;

		int damageAmount, 
			damageInterval;

		public EventHandlers(Plugin plugin)
		{
			this.plugin = plugin;
			this.server = plugin.pluginManager.Server;
			this.damageAmount = plugin.GetConfigInt(SCP008.damageAmountConfigKey);
			this.damageInterval = plugin.GetConfigInt(SCP008.damageIntervalConfigKey);
		}

		#region PlayerSpecific

		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			int damageAmount = plugin.GetConfigInt(SCP008.swingDamageConfigKey);
			int infectChance = plugin.GetConfigInt(SCP008.infectChanceConfigKey);

			//Sets damage to config amount if above 0
			if (ev.Attacker.TeamRole.Role == Role.SCP_049_2 && damageAmount > 0)
				ev.Damage = damageAmount;
			
			//When a zombie damages a player, adds them to list of infected players to damage
			if (SCP008.isEnabled && ev.Attacker.TeamRole.Role == Role.SCP_049_2
				&& !SCP008.playersToDamage.Contains(ev.Player.SteamId)
				&& new Random().Next(1, 100) >= infectChance)
			{
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
				ev.Player.ChangeRole(Role.SCP_049_2, true, false);
				ev.Player.Teleport(pos);
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
			//If its enabled in config and infected list contains player and cure chance is more than, cure.
			if (plugin.GetConfigBool(SCP008.cureEnabledConfigKey)
				&& SCP008.playersToDamage.Contains(ev.Player.SteamId) 
				&& plugin.GetConfigInt(SCP008.cureChanceConfigKey) >= new Random().Next(1,100))
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

			/* Untested
            ev.Server.GetPlayers().ForEach(p =>
            {
                string RoomID = ConfigManager.Manager.Config.GetStringValue("scp008_spawn_room", string.Empty, true);
                if (p.TeamRole.Role == Role.SCP_049_2 && RoomID != string.Empty)
                {
                    plugin.pluginManager.CommandManager.CallCommand(server, "tproom", new string[] { p.PlayerId.ToString(), RoomID });
                }
            });
            */
		}

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			//Checks enabled config on initial start
			if (SCP008.roundCount == 0)
				SCP008.isEnabled = plugin.GetConfigBool(SCP008.enableConfigKey);
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
					server.GetPlayers().ForEach(p =>
					{
						//If the victim is human and the player is in the infected list
						if ((p.TeamRole.Team != Team.SCP && p.TeamRole.Team != Team.SPECTATOR) && SCP008.playersToDamage.Contains(p.SteamId))
						{
							//If the damage doesnt kill, deal the damage
							if (damageAmount < p.GetHealth())
								p.Damage(damageAmount, DamageType.RAGDOLLLESS);
							else if (damageAmount >= p.GetHealth())
							{
								//If the damage kills the human, transform
								SCP008.playersToDamage.Remove(p.SteamId);
								Vector pos = p.GetPosition();
								p.ChangeRole(Role.SCP_049_2, true, false);
								p.Teleport(pos);
							}
						}
					});
			}
		}

	}
}