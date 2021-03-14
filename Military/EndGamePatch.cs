using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Military.NetworkControllers;
using XenoCore.Locale;
using XenoCore.Override.Map;

namespace Military {
	public static class EndGameCentral {
		public static PlayerControl LocalPlayer;
		public static Team WinnerTeam;

		public static void ResetAll() {
			LocalPlayer = PlayerControl.LocalPlayer;
		}
	}
	
	[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
	public class SingleStartGamePatch {
		public static void Postfix(GameStartManager __instance) {
			__instance.MinPlayers = 1;
			__instance.StartButton.color = Palette.EnabledColor;
		}
	}
	
	[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.IsGameOverDueToDeath))]
	public class NoGameOverDueToDeathPatch {
		public static void Postfix(out bool __result) {
			__result = false;
		}
	}
	
	[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CheckEndCriteria))]
	public class CustomEndCriteriaPatch {
		public static bool Prefix(ShipStatus __instance) {
			return false;
		}
	}

	[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.Start))]
	public static class EndGameStartPatch {
		public static void Postfix(EndGameManager __instance) {
			// foreach (var Role in Role.ROLES) {
			// Role.EndGame(__instance);
			// }
		}
	}

	[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
	public static class EndGameSetPatch {
		private static void AddWinner(PlayerControl Player) {
			TempData.winners.Add(new WinningPlayerData(Player.Data));
		}

		private static void AddWinners(IReadOnlyCollection<PlayerControl> Players) {
			var Ordered = new List<PlayerControl>();
			var LocalPlayerId = EndGameCentral.LocalPlayer.Data.PlayerId;
			Ordered.AddRange(Players.Where(Player => Player.Data.PlayerId == LocalPlayerId));
			Ordered.AddRange(Players.Where(Player => Player.Data.PlayerId != LocalPlayerId));

			foreach (var Player in Ordered) {
				AddWinner(Player);
			}
		}

		public static bool Prefix(EndGameManager __instance) {
			TempData.winners.Clear();
			AddWinners(EndGameCentral.WinnerTeam.Players);

			return true;
		}

		public static void Postfix(EndGameManager __instance) {
			var IsWin = EndGameCentral.WinnerTeam.Players
				.Any(Player => Player.PlayerId == EndGameCentral.LocalPlayer.PlayerId);
			
			__instance.WinText.Text = IsWin ? XenoLang.VICTORY.Get() : XenoLang.DEFEAT.Get();
			__instance.WinText.Color = IsWin ? Palette.CrewmateBlue : Palette.ImpostorRed;
			__instance.BackgroundBar.material.color = EndGameCentral.WinnerTeam.Color;

			CustomMapController.SelectedMap = null;
		}
	}
}