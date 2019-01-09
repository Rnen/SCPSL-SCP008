using Smod2;
using Smod2.API;
using Smod2.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCP008PLUGIN
{
	public partial class SCP008
	{
		DateTime _next049Check = new DateTime();
		bool _is049required;
		bool Get049Required()
		{
			if (_next049Check < DateTime.Now)
			{
				_is049required = this.GetConfigBool(SCP008.announceRequire049ConfigKey);
				_next049Check = DateTime.Now.AddSeconds(15);
			}
			return _is049required;
		}
		/// <summary>
		/// Checks if all sources of SCP-008 has beeen exterminated
		/// </summary>
		internal bool SCP008Dead()
		{
			bool scp049alive = (Get049Required()) ?
				plugin.Server.GetPlayers().Where(p => p.TeamRole.Role == Role.SCP_049).Count() > 0 : false;
			bool scp008alive = SCP008.playersToDamage.Count() < 1 &&
				this.Server.GetPlayers().Where(p => p.TeamRole.Role == Role.SCP_049_2).Count() < 1 && !scp049alive;
			return !scp008alive;
		}

		private bool CanAnnounce = false;
		private DateTime _lastCanAnnounceConfigCheck = new DateTime();
		private bool _lastConfigCanAnnounce;
		internal bool GetCanAnnounce()
		{
			if (_lastCanAnnounceConfigCheck == null || _lastCanAnnounceConfigCheck < DateTime.Now)
			{
				_lastConfigCanAnnounce = this.GetConfigBool(announementsenabled);
				_lastCanAnnounceConfigCheck = DateTime.Now.AddSeconds(15);
			}
			return _lastConfigCanAnnounce && CanAnnounce;
		}
		internal void SetCanAnnounce(bool can_announce)
		{
			plugin.Debug("CanAnnounce set to: " + can_announce);
			if (!this.GetConfigBool(announementsenabled))
				plugin.Debug("Announcement(s) disabled in config!");
			CanAnnounce = can_announce;
		}

		#region General_Plugin_Configs
		private static bool _changedEnabledState = false;

		private static DateTime nextCheck = new DateTime();
		private static bool IsEnabled = true;

		public bool GetIsEnabled()
		{
			if (nextCheck < DateTime.Now && !_changedEnabledState)
			{
				nextCheck = DateTime.Now.AddSeconds(30);
				IsEnabled = this.GetConfigBool(enableConfigKey);
			}
			return IsEnabled;
		}
		/// <summary>
		/// Sets the current state of <see cref="SCP008"/>
		/// <para>Also disables further config checks for this</para>
		/// </summary>
		public void SetIsEnabled(bool value)
		{
			IsEnabled = value;
			_changedEnabledState = true;
		}
		#endregion
	}
}
