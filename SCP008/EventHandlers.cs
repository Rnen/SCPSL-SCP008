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

		bool IsEnabled => plugin.GetIsEnabled();

		int damageAmount = 2, 
			damageInterval = 1;
		List<int> rolesCanBecomeInfected = new List<int>();

		public EventHandlers(SCP008 plugin)
		{
			this.plugin = plugin;
		}

		#region PlayerSpecific

		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			if (ev.Attacker.PlayerId == ev.Player.PlayerId || !IsEnabled) return;

			int damageAmount = (ev.Attacker.TeamRole.Role == Smod2.API.RoleType.SCP_049_2) ? plugin.GetConfigInt(SCP008.swingDamageConfigKey) : 0;
			int infectChance = (ev.Attacker.TeamRole.Role == Smod2.API.RoleType.SCP_049_2) ? plugin.GetConfigInt(SCP008.infectChanceConfigKey) : 0;
			int infectOnKillChance = (ev.Attacker.TeamRole.Role == Smod2.API.RoleType.SCP_049_2 && ev.Damage >= (ev.Player.HP + ev.Player?.AHP ?? 0)) ? plugin.GetConfigInt(SCP008.infectKillChanceConfigKey) : 0;

			if (ev.Player.TeamRole.Team == Smod2.API.TeamType.TUTORIAL && ev.Attacker.TeamRole.Role == Smod2.API.RoleType.SCP_049_2
				&& !plugin.GetConfigBool(SCP008.canHitTutConfigKey))
			{ ev.Damage = 0f; return; }

			//Sets damage to config amount if above 0
			if (ev.Attacker.TeamRole.Role == Smod2.API.RoleType.SCP_049_2 && damageAmount > 0)
				ev.Damage = damageAmount;
			
			//When a zombie damages a player, adds them to list of infected players to damage
			if (IsEnabled && ev.Attacker.TeamRole.Role == Smod2.API.RoleType.SCP_049_2
				&& !SCP008.playersToDamage.Contains(ev.Player.UserId)
				&& infectChance > 0
				&& new Random().Next(1, 100) <= infectChance)
			{
				if( (rolesCanBecomeInfected == null || rolesCanBecomeInfected.Count == 0 || rolesCanBecomeInfected.FirstOrDefault() == -1) 
					|| 
					(rolesCanBecomeInfected.Count > 0 && rolesCanBecomeInfected.Contains((int)ev.Player.TeamRole.Role)))
					Infect(ev.Player);
			}

			if (ev.Attacker.TeamRole.Role == Smod2.API.RoleType.SCP_049_2
				&& ev.Damage >= (ev.Player.HP + ev.Player?.AHP ?? 0)
				&& (infectOnKillChance > 99 || new Random().Next(1, 100) <= infectOnKillChance))
			{
				ev.Damage = 0f;
				ChangeToSCP008(ev.Player);
			}
		}

		public void OnPlayerDie(PlayerDeathEvent ev)
		{
			//If player dies, removes them from infected list
			if (plugin.GetConfigBool(SCP008.anyDeathCounts))
			{
				ev.SpawnRagdoll = false;
				ChangeToSCP008(ev.Player);
			}
			else if (SCP008.playersToDamage.Contains(ev.Player.UserId))
				SCP008.playersToDamage.Remove(ev.Player.UserId);
		}

		public void OnMedicalUse(PlayerMedicalUseEvent ev)
		{
			if (!IsEnabled || (ev.MedicalItem != Smod2.API.ItemType.MEDKIT && ev.MedicalItem != Smod2.API.ItemType.SCP500)) 
				return;
			int cureChance = plugin.GetConfigInt(SCP008.cureChanceConfigKey);
			//If its enabled in config and infected list contains player and cure chance is more than, cure.
			if (SCP008.playersToDamage.Contains(ev.Player.UserId)
				&& cureChance > 0
				&& cureChance >= new Random().Next(1, 100))
				SCP008.playersToDamage.Remove(ev.Player.UserId);
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
				if (Server.GetPlayers().Count > 0)
					foreach(Player p in Server.GetPlayers())
					{
						//If the victim is human and the player is in the infected list
						if ((p.TeamRole.Team != Smod2.API.TeamType.SCP && p.TeamRole.Team != Smod2.API.TeamType.SPECTATOR) && SCP008.playersToDamage.Contains(p.UserId))
						{
							//If the damage doesnt kill, deal the damage
							if (damageAmount < (p.HP + p?.AHP ?? 0))
								p.Damage(damageAmount, DamageType.SCP_049_2);
							else if (damageAmount >= (p.HP + p?.AHP ?? 0))
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
			if (SCP008.playersToDamage.Contains(ev.Player.UserId))
				SCP008.playersToDamage.Remove(ev.Player.UserId);
		}

		internal void Infect(Player player)
		{
			if (player == null || SCP008.playersToDamage.Contains(player.UserId))
				return;
			SCP008.playersToDamage.Add(player.UserId);
			player.PersonalBroadcast((uint)plugin.GetConfigInt(SCP008.personalBroadcastDuration), plugin.GetTranslation("youAreInfected"), false);
		}

		internal void ChangeToSCP008(Player player)
		{
			foreach (var scp in Scp079PlayerScript.instances)
				scp.AddExperience(plugin.GetConfigFloat(SCP008.assist079EXP));
			if (SCP008.playersToDamage.Contains(player.UserId))
				SCP008.playersToDamage.Remove(player.UserId);
			Vector pos = player.GetPosition();
			player.ChangeRole(Smod2.API.RoleType.SCP_049_2, spawnTeleport: false, spawnProtect: false);
			player.Teleport(pos);
			plugin.SetCanAnnounce(true);
			plugin.Debug("Changed " + player.Name + " to SCP-008");
			player.PersonalBroadcast((uint)plugin.GetConfigInt(SCP008.personalBroadcastDuration), plugin.GetTranslation("youAre008"), false);
		}
	}
}