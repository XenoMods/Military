using System.Collections.Generic;
using Military.Helpers;
using Military.Logic.Mode;
using Military.Roles;
using XenoCore.Buttons;
using XenoCore.CustomOptions;

namespace Military.Logic {
	public static class GunController {
		public static readonly CooldownController Cooldown = new CooldownController(
			new CooldownValueProvider());
		private static List<Gun> Guns;
		private static Gun CurrentGun;
		private static int CurrentIndex;

		public static void UpdateForPlayer() {
			var Role = PlayerControl.LocalPlayer.Extra().Role;
			Guns = Role.Guns;
			CurrentIndex = 0;
			UpdateGun();
			Cooldown.LastUsedFromNow(0);
		}

		private static void UpdateGun() {
			CurrentGun = Guns[CurrentIndex];
			ModActions.Gun.Icon = CurrentGun.Icon;
			ModActions.Shoot.Targeter = CurrentGun.Targeter;
			ModActions.Shoot.Icon = CurrentGun.ButtonIcon;
			ModActions.Shoot.TargetOutlineColor = CurrentGun.OutlineColor;
			MilitaryTargeterStatics.KillRange = CurrentGun.Distance;
			MilitaryTargeterStatics.PlayerTeam = PlayerControl.LocalPlayer.Extra().Team;
		}

		public static void NextGun() {
			CurrentIndex++;
			CurrentIndex %= Guns.Count;
			UpdateGun();
		}
		
		public static void PreviousGun() {
			CurrentIndex--;
			if (CurrentIndex < 0) {
				CurrentIndex = Guns.Count - 1;
			}
			UpdateGun();
		}

		public static void SetGun(int Index) {
			if (Index >= Guns.Count) return;
			CurrentIndex = Index;
			UpdateGun();
		}

		private class CooldownValueProvider : INumberProvider {
			public float GetValue() {
				return CurrentGun?.Cooldown ?? 1;
			}
		}

		public static void SetMedPack() {
			var Index = 0;
			
			foreach (var Gun in Guns) {
				if (Gun == ExtraResources.GUN_MEDPACK) {
					SetGun(Index);
					return;
				}

				Index++;
			}
		}

		public static void Shoot() {
			if (!Cooldown.IsReady()) return;
			if (!PlayerControl.LocalPlayer.Extra().IsReady()) return;
			
			var Target = ModActions.Shoot.CurrentTarget;
			if (Target == null) return;
			
			ExtraNetwork.Send(CustomRPC.Shoot, Writer => {
				Writer.Write(Target.PlayerId);
				Writer.Write(CurrentGun.Damage);
			});

			if (Target.Extra().DoDamage(CurrentGun.Damage)) {
				GameMode.Current.HandleKill();
			}
			
			Cooldown.Use();
		}
	}
}