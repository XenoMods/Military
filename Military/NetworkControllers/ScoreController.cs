using System.Collections.Generic;
using System.Text;
using Hazel;
using Military.Logic.Mode;
using XenoCore.Core;
using XenoCore.Locale;
using XenoCore.Utils;

namespace Military.NetworkControllers {
	public static class ScoreController {
		private static readonly Dictionary<Team, int> Points = new Dictionary<Team, int>();
		private static readonly StringBuilder Builder = new StringBuilder();

		public static void Reset() {
			Points.Clear();

			foreach (var Team in TeamsController.GetTeams()) {
				Points.Add(Team, Team.Enable ? 0 : -1);
			}
		}

		public static void AddScore(Team Team, int Score = 1) {
			Points[Team] = Points[Team] + Score;
			Syncronize();
			CheckForWin();
		}

		public static void AddScore(Dictionary<Team, int> ToAdd) {
			foreach (var (Team, Score) in ToAdd) {
				Points[Team] = Points[Team] + Score;
			}

			Syncronize();
			CheckForWin();
		}

		public static void CheckForWin() {
			var Max = (int) Military.MaxPoints.GetValue();

			var Winners = new List<Team>();
			foreach (var (Team, Score) in Points) {
				if (Score >= Max) {
					Winners.Add(Team);
				}
			}

			if (Winners.Count == 0) return;
			
			Team MaxTeam = null;
			var MaxScore = int.MinValue;

			foreach (var (Team, Score) in Points) {
				if (Score < MaxScore) continue;

				MaxScore = Score;
				MaxTeam = Team;
			}

			if (MaxTeam == null) return;
			Win(MaxTeam);
		}

		private static void Win(Team WinnerTeam) {
			WinMessage.INSTANCE.Send(WinnerTeam);
			WinnerTeam.Win();
		}

		public static string Info() {
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

		private static void Syncronize() {
			if (!AmongUsClient.Instance.AmHost) return;

			ScoreSyncMessage.INSTANCE.Send(Points);
		}

		public static void ReadSync(MessageReader Reader) {
			var Count = Reader.ReadInt32();

			for (var i = 0; i < Count; i++) {
				Points[Reader.ReadTeam()] = Reader.ReadInt32();
			}
		}

		public static void RegisterMessages(XenoMod Mod) {
			Mod.RegisterMessage(ScoreSyncMessage.INSTANCE);
			Mod.RegisterMessage(WinMessage.INSTANCE);
		}
	}

	internal class WinMessage : Message {
		public static readonly WinMessage INSTANCE = new WinMessage();

		private WinMessage() {
		}
		
		protected override void Handle() {
			Reader.ReadTeam().Win();
		}

		public void Send(Team WinnerTeam) {
			Write(Writer => {
				Writer.WriteTeam(WinnerTeam);
			});
		}
	}

	internal class ScoreSyncMessage : Message {
		public static readonly ScoreSyncMessage INSTANCE = new ScoreSyncMessage();

		private ScoreSyncMessage() {
		}

		protected override void Handle() {
			ScoreController.ReadSync(Reader);
		}

		public void Send(Dictionary<Team, int> Points) {
			Write(Writer => {
				Writer.Write(Points.Count);

				foreach (var (Team, Score) in Points) {
					Writer.WriteTeam(Team);
					Writer.Write(Score);
				}
			});
		}
	}
}