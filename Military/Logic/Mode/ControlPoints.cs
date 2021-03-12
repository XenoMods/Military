using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Military.Component;
using Military.Helpers;
using UnityEngine;
using XenoCore.Locale;
using XenoCore.Override.Map;
using XenoCore.Utils;

namespace Military.Logic.Mode {
	public class ControlPoints : GameMode {
		public static readonly GameMode INSTANCE = new ControlPoints();
		
		public override string Id => "cp";

		private readonly List<ControlPoint> Points = new List<ControlPoint>();

		private DateTime LastUpdated;
		
		protected override void Reset() {
			base.Reset();
			Points.Clear();
		}

		public override void InitMap() {
			base.InitMap();
			LastUpdated = DateTime.Now;

			var Map = PlayerControl.GameOptions.MapId;

			if (CustomMapController.SelectedMap != null) {
				CollectCustom(CustomMapController.SelectedMap);
			} else {
				if (Map == Globals.MAP_SKELD) {
					AddPoint(16.504f, -4.84f);
					AddPoint(9.236f, -12.49f);
					AddPoint(-7.766f, -8.696f);
					AddPoint(-20.577f, -5.781f);
					AddPoint(-8.921f, -4.54f);
					AddPoint(6.49f, -3.716f);
				} else if (Map == Globals.MAP_MIRA) {
					AddPoint(15.34f, -0.088f);
					AddPoint(15.34f, 3.789f);
					AddPoint(8.552f, 12.77f);
					AddPoint(14.8f, 19.78f);
					AddPoint(19.777f, 20.105f);
					AddPoint(17.9f, 23.31f);
				} else if (Map == Globals.MAP_POLUS) {
					AddPoint(3.045f, -12.18f);
					AddPoint(9.728f, -12.51f);
					AddPoint(1.732f, -17.5f);
					AddPoint(2.334f, -24.306f);
					AddPoint(11.407f, -23.46f);
					AddPoint(12.28f, -16.83f);
					AddPoint(20.4f, -12.13f);
					AddPoint(40.13f, -7.86f);
					AddPoint(25.43f, -7.91f);
					AddPoint(33.97f, -10.5f);
					AddPoint(36.13f, -21.88f);
					AddPoint(26.53f, -17.47f);
				}
			}
		}

		private void CollectCustom(CustomMapType SelectedMap) {
			foreach (var Point in SelectedMap.FindComponents<PointComponent>()) {
				AddPoint(Point.Position);
			}
		}

		private void AddPoint(float X, float Y) {
			AddPoint(new Vector2(X, Y));
		}
		
		private void AddPoint(Vector2 Position) {
			Points.Add(new ControlPoint(Points.Count) {
				Position = Position
			});
		}

		public override void Update(float DeltaTime) {
			base.Update(DeltaTime);

			if (!AmongUsClient.Instance.AmHost) return;
			var Difference = DateTime.Now - LastUpdated;
			
			if (Difference.Seconds >= Military.GameModeInterval.GetValue()) {
				var Teams = TeamsController.GetTeams();

				foreach (var Team in Teams) {
					var Score = Points.Count(Point => Team.Compare(Point.CurrentTeam));

					Data.Add(Team, Score);
					
					ExtraNetwork.Send(CustomRPC.AddPoints, Writer => {
						Team.Write(Writer);
						Writer.Write(Score);
					});
				}

				var Winner = Data.CheckWinner();

				if (Winner != null) {
					Points.Clear();
					DoWin(Winner);
				}
				
				LastUpdated = DateTime.Now;
			}
		}

		public override void HandleCapturePoint(int PointId, Team Team) {
			if (PointId >= 0 && PointId < Points.Count) {
				Points[PointId].CurrentTeam = Team;
			}
		}

		public override void HandleAdditionalInfo(StringBuilder Builder) {
			Builder.AppendLine();
			var PointText = LanguageManager.Get("m.point");

			foreach (var Point in Points) {
				var Color = Point.CurrentTeam?.ColorFormat ?? Globals.FORMAT_WHITE;

				Builder.Append(Color).Append(PointText).Append(' ').AppendLine(Point.Name);
			}
		}
	}
}