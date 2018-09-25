using Smod2;
using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using System;

namespace SCP008PLUGIN
{
	class EventHandlers : IEventHandlerRoundStart, IEventHandlerRoundEnd, IEventHandlerWaitingForPlayers, IEventHandlerPlayerHurt, IEventHandlerPlayerDie, IEventHandlerMedkitUse, IEventHandlerUpdate
	{
		private Plugin plugin;
		private Server server;

		public EventHandlers(Plugin plugin)
		{
			this.plugin = plugin;
			this.server = plugin.pluginManager.Server;
		}

		#region PlayerSpecific

		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			int damageAmount = ConfigManager.Manager.Config.GetIntValue("scp008_swing_damage", 0, true);

			if (ev.Attacker.TeamRole.Role == Role.SCP_049_2 && damageAmount > 0)
				ev.Damage = damageAmount;
			if (SCP008.isEnabled && ev.Attacker.TeamRole.Role == Role.SCP_049_2 && !SCP008.playersToDamage.Contains(ev.Player.SteamId))
				SCP008.playersToDamage.Add(ev.Player.SteamId);
		}

		public void OnPlayerDie(PlayerDeathEvent ev)
		{
			if (SCP008.playersToDamage.Contains(ev.Player.SteamId))
				SCP008.playersToDamage.Remove(ev.Player.SteamId);
		}

		public void OnMedkitUse(PlayerMedkitUseEvent ev)
		{
			if (SCP008.playersToDamage.Contains(ev.Player.SteamId))
				SCP008.playersToDamage.Remove(ev.Player.SteamId);
		}

		#endregion

		#region RoundHandlers

		public void OnRoundEnd(RoundEndEvent ev)
		{
			SCP008.playersToDamage.Clear();
		}

		public void OnRoundStart(RoundStartEvent ev)
		{
			SCP008.playersToDamage.Clear();
		}

		public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
		{
			SCP008.isEnabled = ConfigManager.Manager.Config.GetBoolValue("SCP008_enabled", true);
		}

		#endregion

		int damageAmount = ConfigManager.Manager.Config.GetIntValue("SCP008_damage_amount", 1, true),
				damageInterval = ConfigManager.Manager.Config.GetIntValue("SCP008_damage_interval", 2, true);

		DateTime updateTimer = DateTime.Now;

		public void OnUpdate(UpdateEvent ev)
		{
			if (SCP008.isEnabled && updateTimer < DateTime.Now)
			{
				updateTimer.AddSeconds(damageInterval);
				if (server.GetPlayers().Count > 0)
					server.GetPlayers().ForEach(p =>
					{
						if ((p.TeamRole.Team != Team.SCP && p.TeamRole.Team != Team.SPECTATOR) && SCP008.playersToDamage.Contains(p.SteamId))
						{
							if (damageAmount < p.GetHealth())
								p.Damage(damageAmount, DamageType.RAGDOLLLESS);
							else if (damageAmount >= p.GetHealth())
							{
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