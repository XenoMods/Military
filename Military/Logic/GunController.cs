using System.Collections.Generic;
using Military.Helpers;
using Military.Logic.Mode;
using Military.Roles;
using XenoCore.Buttons;
using XenoCore.Core;
using XenoCore.Utils;

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

			Cooldown.Use();
			GunPreController.Shoot(Target, CurrentGun.Damage);
		}

		public static void DoShoot(PlayerControl From, PlayerControl Target, int Damage) {
			if (Target.Extra().DoDamage(Damage)) {
				GameMode.Current.HandleKill(From, Target);
			}
		}
	}

	public static class GunPreController {
		public static void Shoot(PlayerControl Target, int Damage) {
			if (Game.IsHost()) {
				GunController.DoShoot(PlayerControl.LocalPlayer, Target, Damage);
			} else {
				ShootMessage.INSTANCE.Send(Target, Damage);
			}
		}
	}
	
	internal class ShootMessage : Message {
		public static readonly ShootMessage INSTANCE = new ShootMessage();

		private ShootMessage() {
		}
		
		protected override void Handle() {
			GunController.DoShoot(ReadPlayer(), ReadPlayer(), Reader.ReadInt32());
		}

		public void Send(PlayerControl Target, int Damage) {
			Write(Writer => {
				WriteLocalPlayer();
				WritePlayer(Target);
				Writer.Write(Damage);
			});
		}
	}
}