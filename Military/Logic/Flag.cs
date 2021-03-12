using Military.Helpers;
using Military.Logic.Mode;
using UnityEngine;
using XenoCore.Override.Tasks;
using XenoCore.Override.Usable;
using XenoCore.Utils;

namespace Military.Logic {
	public class Flag : CustomUsable {
		public override float UsableDistance => 2f;
		public override Sprite UseIcon => PlayerControl.LocalPlayer.Extra().Team.CaptureIcon;

		public readonly int No;
		public readonly Vector2 OriginalPosition;
		private readonly GameObject FlagObject;

		private readonly MapIcon MapIcon;

		public readonly Team Team;

		private PlayerControl _Capturer;
		public bool PreventSound;

		public PlayerControl Capturer {
			get => _Capturer;
			set {
				if (_Capturer != value && !PreventSound) {
					if (value != null) {
						ExtraResources.SND_FLAG_TAKEN.PlayGlobally(AudioManager.EffectsScale(1.5f));
					} else {
						ExtraResources.SND_FLAG_LOST.PlayGlobally(AudioManager.EffectsScale(1.5f));
					}
				}
				
				_Capturer = value;
				UpdateFlagPosition();
				UpdateMapIcon();
			}
		}

		public Flag(int No, Team Team, Vector2 OriginalPosition) {
			this.No = No;
			this.Team = Team;
			this.OriginalPosition = OriginalPosition;
			
			MapIcon = TasksOverlay.Add(new MapIcon {
				Position = OriginalPosition,
				Color = Team.Color,
				Icon = ExtraResources.FLAG_ICON
			});

			FlagObject = Object.Instantiate(ExtraResources.FLAG);
			if (!Team.Enable) {
				FlagObject.SetActive(false);
			}
			Capturer = null;
			Attach();
		}

		private void UpdateFlagPosition() {
			if (_Capturer != null) {
				FlagObject.transform.SetParent(_Capturer.MyPhysics.Skin.transform);
				FlagObject.transform.localPosition = new Vector2(0.4f, 1.3f);
			} else {
				FlagObject.transform.SetParent(null);
				FlagObject.transform.position = OriginalPosition;
			}
		}

		private void UpdateMapIcon() {
			MapIcon.Pulse = _Capturer != null;
		}

		public void Attach() {
			UsableController.CreateNew(FlagObject, this);
		}

		public override void OnImageMaterialSet() {
			if (ImageMaterial != null) {
				ImageRenderer.color = Team.Color;
			}
		}

		public override void SetOutline(bool on, bool mainTarget) {
			Recolor(0f,
				Team.Color,
				Color.clear);
		}

		public override float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse) {
			if (!Team.Enable || Capturer != null) {
				canUse = false;
				couldUse = false;
				return 0f;
			}

			return base.CanUse(pc, out canUse, out couldUse);
		}

		public override void Use() {
			if (Capturer != null) return;

			var Extra = PlayerControl.LocalPlayer.Extra();
			if (!Extra.IsReady()) return;

			var PlayerTeam = Extra.Team;
			if (Team.Compare(PlayerTeam)) {
				var CarryingFlag = CaptureTheFlag.INSTANCE
					.WhatFlagPlayerCaptured(PlayerControl.LocalPlayer);

				if (CarryingFlag != null) {
					ExtraNetwork.Send(CustomRPC.FlagPoint, Writer => {
						Writer.Write(No);
						Writer.Write(CarryingFlag.No);
						Writer.Write(PlayerControl.LocalPlayer.PlayerId);
					});
					FlagCaptureController.FlagPoint(No,
						CarryingFlag.No, PlayerControl.LocalPlayer.PlayerId);
				}

				return;
			}

			if (CaptureTheFlag.INSTANCE.WhatFlagPlayerCaptured() != null) {
				return;
			}

			if (AmongUsClient.Instance.AmHost) {
				this.SetCapturer(PlayerControl.LocalPlayer.PlayerId);
			} else {
				ExtraNetwork.Send(CustomRPC.RequestCaptureFlag, Writer => {
					Writer.Write(No);
					Writer.Write(PlayerControl.LocalPlayer.PlayerId);
				});
			}
		}
	}

	public static class FlagCaptureController {
		public static void FlagPoint(int DestinationFlagId, int CarryingFlagId, byte PlayerId) {
			if (AmongUsClient.Instance.AmHost) {
				var CarryingFlag = CaptureTheFlag.INSTANCE.GetFlagById(CarryingFlagId);
				CarryingFlag.SetCapturer(-1, true);

				CaptureTheFlag.INSTANCE.OnFlagCaptured(DestinationFlagId);
			}
		}

		public static void RequestCaptureFlag(int FlagId, int PlayerId) {
			if (AmongUsClient.Instance.AmHost) {
				CaptureTheFlag.INSTANCE.GetFlagById(FlagId)
					.SetCapturer(PlayerId);
			}
		}

		public static void SetCapturer(this Flag Flag, int PlayerId, bool NoSound = false) {
			if (AmongUsClient.Instance.AmHost) {
				ExtraNetwork.Send(CustomRPC.CaptureFlag, Writer => {
					Writer.Write(Flag.No);
					Writer.Write(PlayerId);
					Writer.Write(NoSound);
				});
			}

			if (NoSound) {
				Flag.PreventSound = true;
			}
			Flag.Capturer = PlayerId != -1
				? PlayerTools.GetPlayerById((byte) PlayerId)
				: null;
			Flag.PreventSound = false;
		}
	}
}