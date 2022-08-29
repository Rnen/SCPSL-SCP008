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
		private DateTime _next049Check = new DateTime();
		private bool _is049required;
		bool Is049Required
		{
			get
			{
				if (_next049Check < DateTime.Now)
				{
					_is049required = this.GetConfigBool(SCP008.announceRequire049ConfigKey);
					_next049Check = DateTime.Now.AddSeconds(15);
				}
				return _is049required;
			}
		}

		/// <summary>
		/// Checks if all sources of SCP-008 has beeen exterminated
		/// </summary>
		internal bool SCP008Dead
		{
			get
			{
				return (infected.Count < 2 &&  Is049Required) || infected.Count < 1;
			}
		}

		private bool _canAnnounce = false;
		private DateTime _lastCanAnnounceConfigCheck = new DateTime();
		private bool _lastConfigCanAnnounce;
		internal bool CanAnnounceDead
		{
			get
			{
				if (_lastCanAnnounceConfigCheck == null || _lastCanAnnounceConfigCheck < DateTime.Now)
				{
					_lastConfigCanAnnounce = this.GetConfigBool(cassieAnnouncements);
					_lastCanAnnounceConfigCheck = DateTime.Now.AddSeconds(15);
				}
				return _lastConfigCanAnnounce && _canAnnounce;
			}
			set
			{
				plugin.Debug("CanAnnounce set to: " + value);
				if (!this.GetConfigBool(cassieAnnouncements))
					plugin.Debug("Announcement(s) disabled in config!");
				_canAnnounce = value;
			}
		}

		#region General_Plugin_Configs
		private bool _changedEnabledState = false;

		private DateTime _nextCheck = new DateTime();
		private bool _isEnabled = true;

		public bool IsEnabled
		{
			get
			{
				if (_nextCheck < DateTime.Now && !_changedEnabledState)
				{
					_nextCheck = DateTime.Now.AddSeconds(30);
					_isEnabled = this.GetConfigBool(enableConfigKey);
				}
				return _isEnabled;
			}
			set
			{
				_isEnabled = value;
				_changedEnabledState = true;
			}
		}

		internal static bool PersonalBroadcastConfig => plugin.GetConfigBool(SCP008.personalHint);

		private bool _changedEnabledStateZero = false;

		private DateTime _nextCheckZero = new DateTime();
		private bool _isEnabledZeroOnStart = true;

		public bool IsEnabledZeroOnStart
		{
			get
			{
				if (_nextCheckZero < DateTime.Now && !_changedEnabledStateZero)
				{
					_nextCheckZero = DateTime.Now.AddSeconds(30);
					_isEnabledZeroOnStart = this.GetConfigBool(startwithPatientZero);
				}
				return _isEnabledZeroOnStart;
			}
			set
			{
				_isEnabledZeroOnStart = value;
				_changedEnabledStateZero = true;
			}
		}

		#endregion
	}
}
