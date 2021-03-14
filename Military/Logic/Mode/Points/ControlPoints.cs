using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Military.Component;
using Military.NetworkControllers;
using UnityEngine;
using XenoCore.Locale;
using XenoCore.Override.Map;
using XenoCore.Utils;

namespace Military.Logic.Mode.Points {
	public class ControlPoints : GameMode {
		public static readonly GameMode INSTANCE = new ControlPoints();
		
		public override string Id => "cp";

		protected override void Reset() {
			base.Reset();
			PointsController.Reset();
		}

		public override void InitMap() {
			base.InitMap();
			PointsController.ResetTime();

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

		private static void AddPoint(float X, float Y) {
			PointsController.AddPoint(new Vector2(X, Y));
		}
		
		private static void AddPoint(Vector2 Position) {
			PointsController.AddPoint(Position);
		}

		public override void Update(float DeltaTime) {
			base.Update(DeltaTime);
			PointsController.Update(DeltaTime);
		}

		public override void HandleAdditionalInfo(StringBuilder Builder) {
			Builder.AppendLine();
			var PointText = LanguageManager.Get("m.point");

			foreach (var Point in PointsController.GetPoints()) {
				var Color = Point.CurrentTeam?.ColorFormat ?? Globals.FORMAT_WHITE;

				Builder.Append(Color).Append(PointText).Append(' ').AppendLine(Point.Name);
			}
		}
	}
}