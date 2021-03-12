using System.Collections.Generic;
using System.Text;
using Military.Helpers;
using UnityEngine;
using XenoCore.Locale;
using XenoCore.Override.Map;
using XenoCore.Utils;

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
		public readonly PointsData Data = new PointsData();

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
			Data.Reset();
		}

		public virtual void InitMap() {
		}

		public virtual void Update(float DeltaTime) {
			HudManager.Instance.TaskText.Text = Data.Info();
		}

		protected void DoWin(Team Team) {
			ExtraNetwork.Send(CustomRPC.Winner, Writer => {
				// ReSharper disable once ConvertClosureToMethodGroup
				Team.Write(Writer);
			});

			Team.Win();
		}

		protected void AddPointToLocalPlayer() {
			var Team = PlayerControl.LocalPlayer.Extra().Team;
			ExtraNetwork.Send(CustomRPC.AddPoints, Writer => {
				Team.Write(Writer);
				Writer.Write((int) 1);
			});

			if (Team.AddPoint()) {
				DoWin(Team);
			}
		}

		public virtual void HandleKill() {
		}

		public virtual void HandleCapturePoint(int PointId, Team Team) {
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

	public class PointsData {
		public readonly Dictionary<Team, int> Points = new Dictionary<Team, int>();
		private readonly StringBuilder Builder = new StringBuilder();

		public void Reset() {
			Points.Clear();

			foreach (var Team in TeamsController.GetTeams()) {
				Points.Add(Team, Team.Enable ? 0 : -1);
			}
		}

		public bool Add(Team Team, int Count = 1) {
			if (GameMode.Current == CaptureTheFlag.INSTANCE) {
				ExtraResources.SND_FLAG_CAPTURED.PlayGlobally(AudioManager.EffectsScale(1.5f));
			}

			Points[Team] = Points[Team] + Count;
			return Points[Team] >= (int) Military.MaxPoints.GetValue();
		}

		public Team CheckWinner() {
			var Max = (int) Military.MaxPoints.GetValue();

			var Winners = new List<Team>();
			foreach (var (Team, Score) in Points) {
				if (Score >= Max) {
					Winners.Add(Team);
				}
			}

			if (Winners.Count == 0) {
				return null;
			} else {
				Team MaxTeam = null;
				var MaxScore = int.MinValue;

				foreach (var (Team, Score) in Points) {
					if (Score < MaxScore) continue;

					MaxScore = Score;
					MaxTeam = Team;
				}

				return MaxTeam;
			}
		}

		public string Info() {
			var Max = (int) Military.MaxPoints.GetValue();

			Builder.Clear();
			Builder.AppendLine(LanguageManager.Get("m.points"));

			foreach (var (Team, Value) in Points) {
				if (Value == -1) continue;

				Builder.Append(Team.ColorFormat);
				Builder.Append(LanguageManager.Get($"m.team.{Team.Name}.which"));
				Builder.Append(": ").Append($"{Value} / {Max}").AppendLine(Globals.FORMAT_WHITE);
			}

			GameMode.Current.HandleAdditionalInfo(Builder);

			return Builder.ToString();
		}
	}
}