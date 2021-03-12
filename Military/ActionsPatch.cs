using HarmonyLib;
using Military.Roles;

namespace Military {
	[HarmonyPatch(typeof(UseButtonManager), nameof(UseButtonManager.DoClick))]
	public class ActionSecondaryPatch {
		public static void Prefix() {
		}
	}

	[HarmonyPatch(typeof(KillButtonManager), nameof(KillButtonManager.PerformKill))]
	public class ActionPrimaryPatch {
		public static bool Prefix() {
			return ModActions.DoAction(ActionType.SHOOT);
		}
	}
}