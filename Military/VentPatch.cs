using System;
using System.Collections.Generic;
using HarmonyLib;
using Military.Logic.Mode;
using UnityEngine;

namespace Military {
	[HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
	public static class VentPatch {
		public static readonly IDictionary<byte, DateTime> AllVentTimes =
			new Dictionary<byte, DateTime>();

		public static void SetLastVent(byte player) {
			if (AllVentTimes.ContainsKey(player)) {
				AllVentTimes[player] = DateTime.UtcNow;
			} else {
				AllVentTimes.Add(player, DateTime.UtcNow);
			}
		}

		public static DateTime GetLastVent(byte player) {
			return AllVentTimes.ContainsKey(player)
				? AllVentTimes[player]
				: new DateTime(0);
		}

		public static bool Prefix(Vent __instance, ref float __result, [HarmonyArgument(0)] GameData.PlayerInfo pc,
			[HarmonyArgument(1)] out bool CanUse, [HarmonyArgument(2)] out bool CouldUse) {
			var Num = float.MaxValue;
			var LocalPlayer = pc.Object;

			CouldUse = Military.Vents.GetValue() && !LocalPlayer.Data.IsDead;
			if (!GameMode.Current.CanVent(LocalPlayer)) {
				CouldUse = false;
			}
			CanUse = CouldUse;
			
			if ((DateTime.UtcNow - GetLastVent(pc.Object.PlayerId)).TotalMilliseconds > 800) {
				Num = Vector2.Distance(LocalPlayer.GetTruePosition(), __instance.transform.position);
				CanUse &= Num <= __instance.UsableDistance;
			}

			__result = Num;
			return false;
		}
	}

	[HarmonyPatch(typeof(Vent), "Method_38")]
	public static class VentEnterPatch {
		public static void Postfix(PlayerControl NMEAPOJFNKA) {
			VentPatch.SetLastVent(NMEAPOJFNKA.PlayerId);
		}
	}

	[HarmonyPatch(typeof(Vent), "Method_1")]
	public static class VentExitPatch {
		public static void Postfix(PlayerControl NMEAPOJFNKA) {
			VentPatch.SetLastVent(NMEAPOJFNKA.PlayerId);
		}
	}
}