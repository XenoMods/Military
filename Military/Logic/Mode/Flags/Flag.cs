using Hazel;
using Military.Helpers;
using Military.NetworkControllers;
using UnityEngine;
using XenoCore.Core;
using XenoCore.Override.Tasks;
using XenoCore.Override.Usable;

namespace Military.Logic.Mode.Flags {
	public class Flag : CustomUsable {
		public override float UsableDistance => 2f;
		public override Sprite UseIcon => PlayerControl.LocalPlayer.Extra().Team.CaptureIcon;

		public readonly int No;
		public readonly Vector2 OriginalPosition;
		private readonly GameObject FlagObject;

		private readonly MapIcon MapIcon;

		public readonly Team Team;

		private PlayerControl _Capturer;

		public PlayerControl Capturer {
			get => _Capturer;
			set {
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
			Recolor(0f, Team.Color, Color.clear);
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

			FlagsPreController.Use(this);
		}

		public void ReadCapturer(MessageReader Reader) {
			Capturer = Reader.ReadNullablePlayer();
		}

		public void WriteCapturer(MessageWriter Writer) {
			Writer.WriteNullablePlayer(Capturer);
		}
	}
}