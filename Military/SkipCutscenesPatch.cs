using HarmonyLib;

namespace Military {
	[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
	public static class BeginGamePatch {
		public static void Postfix(GameStartManager __instance) {
			__instance.countDownTimer = 0;
		}
	}
	
	[HarmonyPatch(typeof(ShhhBehaviour), nameof(ShhhBehaviour.Update))]
	public class ShhhPatch {
		public static void Prefix(ShhhBehaviour __instance) {
			__instance.Delay = 0;
			__instance.Duration = 0;
			__instance.HoldDuration = 0;
			__instance.PulseDuration = 0;
			__instance.TextDuration = 0;
		}
	}
}