using System;
using Military.Helpers;
using UnityEngine;
using XenoCore.Override.Tasks;
using XenoCore.Override.Usable;
using XenoCore.Utils;
using Object = UnityEngine.Object;

namespace Military.Logic {
	public class ControlPoint : CustomUsable {
		public override float UsableDistance => 2f;
		public override Sprite UseIcon => PlayerControl.LocalPlayer.Extra().Team.CaptureIcon;

		private readonly int No;
		private readonly GameObject PointObject;
		private readonly MapIcon MapIcon;
		
		private Team _CurrentTeam;

		public Team CurrentTeam {
			get => _CurrentTeam;
			set {
				if (_CurrentTeam != value && value != null) {
					if (PlayerControl.LocalPlayer.Extra().Team.Compare(value)) {
						ExtraResources.SND_POINT_ALLY.PlayGlobally(AudioManager.EffectsScale(1.5f));
					} else {
						ExtraResources.SND_POINT_ENEMY.PlayGlobally(AudioManager.EffectsScale(1.5f));
					}
				}
				
				if (ImageMaterial != null) {
					ImageRenderer.color = value?.Color ?? Color.white;
				}

				_CurrentTeam = value;
				UpdateMapIcon();
			}
		}

		private Vector2 _Position;

		public Vector2 Position {
			get => _Position;
			set {
				PointObject.transform.position = value;
				_Position = value;
				UpdateMapIcon();
			}
		}

		private static readonly string[] Names = new[] {
			"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R"
		};

		public readonly string Name;

		public static DateTime LastUsed;
		
		public override float PercentCooldown =>
			1f - Math.Min((float) (DateTime.Now - LastUsed).TotalMilliseconds
			              / (Military.ControlPointUseInterval.GetValue() * 1000), 1f);

		public ControlPoint(int No) {
			LastUsed = DateTime.Now;

			this.No = No;
			Name = Names[No];

			MapIcon = TasksOverlay.Add(new MapIcon {
				Position = _Position,
				Color = _CurrentTeam?.Color ?? Color.white,
				Icon = ExtraResources.POINT_ICON,
			});
			
			PointObject = Object.Instantiate(Team.NONE_POINT_PREFAB);
			Position = Vector2.zero;
			CurrentTeam = null;
			Attach();
		}

		public void Attach() {
			UsableController.CreateNew(PointObject, this);
		}

		private void UpdateMapIcon() {
			MapIcon.Position = _Position;
			MapIcon.Color = _CurrentTeam?.Color ?? Color.white;
		}

		public override void OnImageMaterialSet() {
			CurrentTeam = CurrentTeam;
		}

		public override void SetOutline(bool on, bool mainTarget) {
			Recolor(@on ? 0.5f : 0f,
				_CurrentTeam?.Color ?? Color.gray,
				Color.clear);
		}

		private bool IsReady() {
			return (DateTime.Now - LastUsed).Seconds >= Military.ControlPointUseInterval.GetValue();
		}

		public override void Use() {
			if (!IsReady() || !PlayerControl.LocalPlayer.Extra().IsReady()) {
				return;
			}
			
			LastUsed = DateTime.Now;

			CurrentTeam = PlayerControl.LocalPlayer.Extra().Team;

			ExtraNetwork.Send(CustomRPC.CapturePoint, Writer => {
				Writer.Write(No);
				CurrentTeam.Write(Writer);
			});
		}
	}
}