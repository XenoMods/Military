using System;
using System.Collections.Generic;
using HarmonyLib;
using Military.Helpers;
using Military.NetworkControllers;
using UnityEngine;
using XenoCore.Utils;
using Object = UnityEngine.Object;

namespace Military {
	public static class TeamAffinityTabController {
		public static readonly Dictionary<Team, ColorChip> Chips = new Dictionary<Team, ColorChip>();
	}

	[HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.OnEnable))]
	public static class TeamAffinityTabOnEnablePatch {
		public static void Postfix(PlayerTab __instance) {
			TeamAffinityTabController.Chips.Clear();
			var Teams = TeamsController.GetTeams();
			
			for (var Index = 0; Index < Teams.Count; Index++) {
				var XPos = __instance.XRange.Lerp(Index % 3 / 2f);
				var YPos = -3.25f - (Index / 3) * 0.75f;
				var colorChip = Object.Instantiate(__instance.ColorTabPrefab,
					__instance.transform, true);
				colorChip.transform.localPosition = new Vector3(XPos, YPos, -1f);
				var Team = Teams[Index];
				colorChip.Button.OnClick.AddListener(new Action(() => {
					if (!Team.Enable) return;
					TeamAffinityController.PreSetAffinity(Team);
				}));
				colorChip.Inner.FrontLayer.sprite = ExtraResources.TEAM_ICON;
				colorChip.Inner.color = Team.Color;
				TeamAffinityTabController.Chips.Add(Team, colorChip);
			}
		}
	}
	
	[HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.OnDisable))]
	public static class TeamAffinityTabOnDisablePatch {
		public static void Postfix() {
			foreach (var (_, Chip) in TeamAffinityTabController.Chips) {
				Object.Destroy(Chip.gameObject);
			}
			
			TeamAffinityTabController.Chips.Clear();
		}
	}

	[HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.Update))]
	public static class TeamAffinityTabUpdatePatch {
		public static void Postfix() {
			foreach (var (Team, Chip) in TeamAffinityTabController.Chips) {
				Chip.InUseForeground.SetActive(!Team.Enable);
			}
		}
	}
}