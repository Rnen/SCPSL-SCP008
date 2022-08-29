using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Smod2.API;

namespace SCP008PLUGIN
{
	internal static class ExtentionMethods
	{
		internal static void SCP008Infect(this Player player) => Utility.Infect(player);

		internal static void SCP008Cure(this Player player) => Utility.CureInfection(player);

		internal static bool IsInfected(this Player player) => SCP008.infected.Contains(player.UserID);

		internal static bool IsPatientZero(this Player player) => player.IsInfected() && SCP008.patientZero == player.UserID;

	}
}
