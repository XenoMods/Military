using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using Military.Logic.Mode.Points;
using UnityEngine;
using XenoCore.Core;
using XenoCore.Utils;

namespace Military.NetworkControllers {
	public static class PointsController {
		private static readonly List<ControlPoint> Points = new List<ControlPoint>();
		private static DateTime LastUpdated;
		
		public static void Reset() {
			Points.Clear();
		}

		public static void ResetTime() {
			LastUpdated = DateTime.Now;
		}

		public static void AddPoint(Vector2 Position) {
			Points.Add(new ControlPoint(Points.Count) {
				Position = Position
			});
		}

		public static void Update(float DeltaTime) {
			if (!Game.IsHost()) return;
			var Difference = DateTime.Now - LastUpdated;

			if (!(Difference.Seconds >= Military.GameModeInterval.GetValue())) return;
			var Teams = TeamsController.GetTeams();
			ScoreController.AddScore(Teams.ToDictionary(Team => Team,
				Team => Points.Count(Point => Team.Compare(Point.CurrentTeam))));

			LastUpdated = DateTime.Now;
		}

		public static void Use(ControlPoint Point, PlayerControl Player) {
			var Extra = Player.Extra();
			if (!Extra.IsReady()) return;

			Point.CurrentTeam = Extra.Team;
			Synchronize();
		}

		public static IEnumerable<ControlPoint> GetPoints() {
			return Points;
		}

		public static ControlPoint ReadPoint(this MessageReader Reader) {
			return Points[Reader.ReadInt32()];
		}

		public static void WritePoint(this MessageWriter Writer, ControlPoint Point) {
			Writer.Write(Point.No);
		}

		private static void Synchronize() {
			if (!AmongUsClient.Instance.AmHost) return;
			
			PointsSyncMessage.INSTANCE.Send(Points);
		}

		public static void ReadSync(MessageReader Reader) {
			var Count = Reader.ReadInt32();

			for (var i = 0; i < Count; i++) {
				var Point = Reader.ReadPoint();
				Point.ReadTeam(Reader);
			}
		}

		public static void RegisterMessages(XenoMod Mod) {
			Mod.RegisterMessage(PointUseMessage.INSTANCE);
			Mod.RegisterMessage(PointsSyncMessage.INSTANCE);
		}
	}

	public static class PointsPreController {
		public static void Use(ControlPoint Point) {
			if (Game.IsHost()) {
				PointsController.Use(Point, PlayerControl.LocalPlayer);
			} else {
				PointUseMessage.INSTANCE.Send(Point);
			}
		}
	}
	
	internal class PointUseMessage : Message {
		public static readonly PointUseMessage INSTANCE = new PointUseMessage();

		private PointUseMessage() {
		}

		protected override void Handle() {
			PointsController.Use(Reader.ReadPoint(), ReadPlayer());
		}

		public void Send(ControlPoint Point) {
			Write(Writer => {
				Writer.WritePoint(Point);
				WriteLocalPlayer();
			});
		}
	}
	
	internal class PointsSyncMessage : Message {
		public static readonly PointsSyncMessage INSTANCE = new PointsSyncMessage();

		private PointsSyncMessage() {
		}

		protected override void Handle() {
			PointsController.ReadSync(Reader);
		}

		public void Send(List<ControlPoint> Points) {
			Write(Writer => {
				Writer.Write(Points.Count);

				foreach (var Point in Points) {
					Writer.WritePoint(Point);
					Point.WriteTeam(Writer);
				}
			});
		}
	}
}