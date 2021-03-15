using System.Collections.Generic;
using Military.Helpers;
using Military.Logic;
using UnityEngine;
using XenoCore.Buttons;
using XenoCore.Buttons.Strategy;
using XenoCore.Events;

namespace Military.Roles {
	public enum ActionType {
		SHOOT,
		GUN,
		GUN_SET_1,
		GUN_SET_2,
		GUN_SET_3,
		GUN_SET_4,
		GUN_SET_5,
		GUN_SET_6,
		GUN_SET_7,
		GUN_SET_8,
		GUN_SET_9,
		MEDPACK_SET,
	}
	
	public static class ModActions {
		// public static GunOverlay GunOverlay;
		public static UniversalButton Shoot;
		public static UniversalButton Gun;

		public static void Init() {
			EventsController.HUD_START.Register(() => {
				// GunOverlay = ExtraResources.GUN_OVERLAY.Build();
				Shoot = UniversalButton.FromKillButton();
				Gun = Make(new Vector2(0.5f, 0), ActionType.GUN);
			});
		}

		private static UniversalButton Make(Vector2 Offset, ActionType Type) {
			return UniversalButton.Create(Offset, () => DoAction(Type));
		}

		public static void ResetAll() {
			Shoot.Reset();
			Gun.Reset();

			Shoot.Visible = true;
			Shoot.Active = true;
			Shoot.Icon = ExtraResources.SHOOT;
			Shoot.Saturator = ActiveKDAndTargetSaturator.INSTANCE;
			Shoot.Targeter = StandardGunTargeter.INSTANCE;
			Shoot.Cooldown = GunController.Cooldown;

			Gun.Visible = true;
			Gun.Active = true;
			Gun.Saturator = ActiveSaturator.INSTANCE;
		}

		public static void Update() {
			Shoot.Update();
			Gun.Update();
		}

		public static List<KeyboardAction> MakeActions() {
			return new List<KeyboardAction> {
				new KeyboardAction(KeyCode.Q, ActionType.SHOOT),
				new KeyboardAction(KeyCode.F, ActionType.GUN),
				new KeyboardAction(KeyCode.Alpha1, ActionType.GUN_SET_1),
				new KeyboardAction(KeyCode.Alpha2, ActionType.GUN_SET_2),
				new KeyboardAction(KeyCode.Alpha3, ActionType.GUN_SET_3),
				new KeyboardAction(KeyCode.Alpha4, ActionType.GUN_SET_4),
				new KeyboardAction(KeyCode.Alpha5, ActionType.GUN_SET_5),
				new KeyboardAction(KeyCode.Alpha6, ActionType.GUN_SET_6),
				new KeyboardAction(KeyCode.Alpha7, ActionType.GUN_SET_7),
				new KeyboardAction(KeyCode.Alpha8, ActionType.GUN_SET_8),
				new KeyboardAction(KeyCode.Alpha9, ActionType.GUN_SET_9),
				new KeyboardAction(KeyCode.Z, ActionType.MEDPACK_SET),
			};
		}

		public static bool DoAction(ActionType Type) {
			if (!PlayerControl.LocalPlayer.moveable) return false;
			if (HudManager.Instance.Chat != null
			    && HudManager.Instance.Chat.IsOpen) return false;
			
			switch (Type) {
				case ActionType.SHOOT: {
					GunController.Shoot();
					break;
				}
				case ActionType.GUN: {
					GunController.NextGun();
					break;
				}
				case ActionType.GUN_SET_1: {
					GunController.SetGun(0);
					break;
				}
				case ActionType.GUN_SET_2: {
					GunController.SetGun(1);
					break;
				}
				case ActionType.GUN_SET_3: {
					GunController.SetGun(2);
					break;
				}
				case ActionType.GUN_SET_4: {
					GunController.SetGun(3);
					break;
				}
				case ActionType.GUN_SET_5: {
					GunController.SetGun(4);
					break;
				}
				case ActionType.GUN_SET_6: {
					GunController.SetGun(5);
					break;
				}
				case ActionType.GUN_SET_7: {
					GunController.SetGun(6);
					break;
				}
				case ActionType.GUN_SET_8: {
					GunController.SetGun(7);
					break;
				}
				case ActionType.GUN_SET_9: {
					GunController.SetGun(8);
					break;
				}
				case ActionType.MEDPACK_SET: {
					GunController.SetMedPack();
					break;
				}
			}

			return true;
		}
	}

	public static class KeyboardController {
		private static readonly List<KeyboardAction> Actions = ModActions.MakeActions();
		
		public static void Update() {
			var IsImpostor = PlayerControl.LocalPlayer.Data.IsImpostor;
			
			foreach (var Action in Actions) {
				if (!(IsImpostor && Action.Type == ActionType.SHOOT)
				    && !Action.Last && Input.GetKeyDown(Action.Key)) {
					ModActions.DoAction(Action.Type);
				}

				Action.Last = Input.GetKeyUp(Action.Key);
			}
		}
	}

	public class KeyboardAction {
		public readonly KeyCode Key;
		public readonly ActionType Type;
		public bool Last;

		public KeyboardAction(KeyCode Key, ActionType Type) {
			this.Key = Key;
			this.Type = Type;
		}
	}
}