using HarmonyLib;
using Military.Logic;
using Military.Logic.Mode;
using Military.Roles;
using UnityEngine;
using XenoCore.Utils;

namespace Military {
	[HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.Method_24))]
	public class GameOptionsData_ToHudString {
		public static void Postfix(ref string __result) {
			DestroyableSingleton<HudManager>.Instance.GameSettings.scale = 0.475f;
		}
	}

	[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
	public class HudUpdateManager {
		private static readonly Color NO_PROTECTION = new Color(1f, 1f, 1f, 1f);
		private static readonly Color PROTECTION = new Color(1f, 1f, 1f, 0.5f);

		public static void Postfix(HudManager __instance) {
			if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started) return;

			if (!__instance.Chat.isActiveAndEnabled) {
				__instance.Chat.SetVisible(true);
			}

			var Scroll = Input.GetAxis("Mouse ScrollWheel");
			if (Scroll < 0) {
				GunController.PreviousGun();
			} else if (Scroll > 0) {
				GunController.NextGun();
			}

			var LocalPlayer = PlayerControl.LocalPlayer;
			var Dead = LocalPlayer.Data.IsDead;

			KeyboardController.Update();

			var UseButtonActiveEnabled = __instance.UseButton != null
			                             && __instance.UseButton.isActiveAndEnabled;
			PlayerTools.CalculateClosest(PlayerControl.LocalPlayer);

			foreach (var Role in Role.ROLES) {
				Role.PreUpdate(__instance, UseButtonActiveEnabled, Dead);
			}

			foreach (var Role in Role.ROLES) {
				Role.PostUpdate(__instance, UseButtonActiveEnabled, Dead);
			}

			__instance.ReportButton.gameObject.SetActive(false);
			GameMode.Current?.Update(Time.deltaTime);

			ModActions.Update();

			foreach (var Control in PlayerControl.AllPlayerControls) {
				var Extra = Control.Extra();
				if (Extra.Team == null) continue;

				var Protection = !Extra.IsReady();
				var Color = Protection ? PROTECTION : NO_PROTECTION;

				Control.nameText.Color = Protection
					? Extra.Team.ProtectionColor
					: Extra.Team.Color;

				Extra.SetHealthBarActive(!Protection);
				Control.myRend.color = Color;
				Control.HatRenderer.color = Color;

				Control.CurrentPet.rend.color = Color;
				if (Control.CurrentPet.shadowRend != null) {
					Control.CurrentPet.shadowRend.color = Color;
				}

				Control.MyPhysics.Skin.layer.color = Color;

				Extra.Update();
			}
		}
	}
}