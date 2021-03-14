using System.Collections.Generic;
using System.Text;
using Military.Logic.Mode.Flags;
using Military.Logic.Mode.Points;
using Military.Logic.Mode.TDM;
using Military.NetworkControllers;
using UnityEngine;
using XenoCore.Override.Map;

namespace Military.Logic.Mode {
	public class GameMode {
		private static readonly Dictionary<string, GameMode> GameModes
			= new Dictionary<string, GameMode>();

		private static readonly Dictionary<string, GameMode> SearchGameModes
			= new Dictionary<string, GameMode>();

		static GameMode() {
			Register(TeamDeathMatch.INSTANCE);
			Register(ControlPoints.INSTANCE);
			Register(CaptureTheFlag.INSTANCE);
		}

		private static void Register(GameMode Mode) {
			GameModes.Add(Mode.Id, Mode);
			SearchGameModes.Add($"m.gamemode.{Mode.Id}", Mode);
		}

		public static GameMode Current { get; private set; }

		public virtual string Id { get; }

		public static GameMode GetById(string Id) {
			return SearchGameModes[Id];
		}

		public static Dictionary<string, GameMode>.KeyCollection GetKeys() {
			return GameModes.Keys;
		}

		public static void ResetAll() {
			Current = GetById(Military.GameMode.GetText());
			Current.Reset();
		}

		protected virtual void Reset() {
			ScoreController.Reset();
		}

		public virtual void InitMap() {
		}

		public virtual void Update(float DeltaTime) {
			HudManager.Instance.TaskText.Text = ScoreController.Info();
		}

		public virtual void HandleKill(PlayerControl From, PlayerControl Target) {
		}

		public virtual void HandleAdditionalInfo(StringBuilder Builder) {
		}

		public virtual void HandlePlayerRespawn(PlayerControl Player) {
			if (ShipStatus.Instance != null) {
				Player.NetTransform.SnapTo(ShipStatus.Instance
					.GetSpawnLocation(Player.PlayerId,
						PlayerControl.AllPlayerControls.Count, false));
			}
		}

		public virtual Vector2 HandleSpawnLocation(Vector2 Source, PlayerControl Player) {
			return Source;
		}

		public virtual bool CanVent(PlayerControl Player) {
			return true;
		}
	}
	
	public class GameModeSpawnModifier : ISpawnLocationModifier {
		public Vector2 Modify(Vector2 Source, PlayerControl Player) {
			return GameMode.Current.HandleSpawnLocation(Source, Player);
		}
	}
}