using System.Collections.Generic;
using System.Linq;
using System.Text;
using Military.Component;
using Military.NetworkControllers;
using UnityEngine;
using XenoCore.Locale;
using XenoCore.Override.Map;
using XenoCore.Utils;

namespace Military.Logic.Mode.Flags {
	public class CaptureTheFlag : GameMode {
		public static readonly CaptureTheFlag INSTANCE = new CaptureTheFlag();
		
		public override string Id => "ctf";

		protected override void Reset() {
			base.Reset();
			FlagsController.Reset();
		}

		public override void InitMap() {
			base.InitMap();
			var Map = PlayerControl.GameOptions.MapId;
			var EnabledTeams = TeamsController.GetTeams()
				.Where(Team => Team.Enable).ToList();

			if (CustomMapController.SelectedMap != null) {
				CollectCustom(CustomMapController.SelectedMap, EnabledTeams);
			} else {
				if (Map == Globals.MAP_SKELD) {
					AddSkeld(EnabledTeams);
				} else if (Map == Globals.MAP_MIRA) {
					AddMira(EnabledTeams);
				} else if (Map == Globals.MAP_POLUS) {
					AddPolus(EnabledTeams);
				}
			}
		}

		private void CollectCustom(CustomMapType MapType, IReadOnlyList<Team> EnabledTeams) {
			var TeamIndex = 0;
			var Count = EnabledTeams.Count;
			
			foreach (var Flag in MapType.FindComponents<FlagComponent>()) {
				var Valid = (Flag.For2 && Count == 2)
				            || (Flag.For3 && Count == 3)
				            || (Flag.For4 && Count == 4);

				if (!Valid) continue;
				AddFlag(Flag.Position, EnabledTeams[TeamIndex]);
				TeamIndex++;
			}
		}

		private void AddSkeld(IReadOnlyList<Team> EnabledTeams) {
			if (EnabledTeams.Count == 4) {
				// Upper engine
				AddFlag(-17.75803f, 2.317879f, EnabledTeams[0]);
				// Lower engine
				AddFlag(-17.66763f, -13.52541f, EnabledTeams[2]);
				// Shields
				AddFlag(8.238161f, -14.54787f, EnabledTeams[3]);
				// Weapons
				AddFlag(10.39382f, 2.032258f, EnabledTeams[1]);
			} else if (EnabledTeams.Count == 2) {
				// Reactor
				AddFlag(-20.48403f, -5.603765f, EnabledTeams[0]);
				// Navigation
				AddFlag(16.79413f, -4.897246f, EnabledTeams[1]);
			} else if (EnabledTeams.Count == 3) {
				// Reactor
				AddFlag(-20.48403f, -5.603765f, EnabledTeams[0]);
				// Navigation
				AddFlag(16.79413f, -4.897246f, EnabledTeams[1]);
				// Storage
				AddFlag(-2.005786f, -17.11224f, EnabledTeams[2]);
			}
		}
		
		private void AddMira(IReadOnlyList<Team> EnabledTeams) {
			if (EnabledTeams.Count == 4) {
				// Start
				AddFlag(-3.583058f, 2.859829f, EnabledTeams[0]);
				// Reactor
				AddFlag(8.487282f, 12.49906f, EnabledTeams[1]);
				// GreenHouse
				AddFlag(17.84238f, 23.15125f, EnabledTeams[2]);
				// Balcony
				AddFlag(21.92661f, -2.247389f, EnabledTeams[3]);
			} else if (EnabledTeams.Count == 2) {
				// Reactor
				AddFlag(8.487282f, 12.49906f, EnabledTeams[0]);
				// Balcony
				AddFlag(21.92661f, -2.247389f, EnabledTeams[1]);
			} else if (EnabledTeams.Count == 3) {
				// Reactor
				AddFlag(8.487282f, 12.49906f, EnabledTeams[0]);
				// Balcony
				AddFlag(21.92661f, -2.247389f, EnabledTeams[1]);
				// GreenHouse
				AddFlag(17.84238f, 23.15125f, EnabledTeams[2]);
			}
		}
		
		private void AddPolus(IReadOnlyList<Team> EnabledTeams) {
			if (EnabledTeams.Count == 4) {
				// Storage
				AddFlag(19.52026f, -11.70811f, EnabledTeams[0]);
				// Oxygen
				AddFlag(1.779697f, -17.46756f, EnabledTeams[1]);
				// Office
				AddFlag(20.87975f, -22.40034f, EnabledTeams[2]);
				// Scan
				AddFlag(34.90693f, -10.36624f, EnabledTeams[3]);
			} else if (EnabledTeams.Count == 2) {
				// Oxygen
				AddFlag(1.779697f, -17.46756f, EnabledTeams[0]);
				// Scan
				AddFlag(34.90693f, -10.36624f, EnabledTeams[1]);
			} else if (EnabledTeams.Count == 3) {
				// Oxygen
				AddFlag(1.779697f, -17.46756f, EnabledTeams[0]);
				// Office
				AddFlag(20.87975f, -22.40034f, EnabledTeams[1]);
				// Scan
				AddFlag(34.90693f, -10.36624f, EnabledTeams[2]);
			}
		}

		private static void AddFlag(float X, float Y, Team Team) {
			AddFlag(new Vector2(X, Y), Team);
		}
		
		private static void AddFlag(Vector2 Position, Team Team) {
			FlagsController.AddFlag(Position, Team);
		}

		public override Vector2 HandleSpawnLocation(Vector2 Source, PlayerControl Player) {
			return FlagsController.GetSpawn(Player) ?? Source;
		}

		public override void HandlePlayerRespawn(PlayerControl Player) {
			var NewSpawn = FlagsController.GetSpawn(Player);
			if (NewSpawn.HasValue) {
				Player.NetTransform.SnapTo(NewSpawn.Value);
			}

			FlagsController.DropAll(Player);
		}

		public override void HandleAdditionalInfo(StringBuilder Builder) {
			Builder.AppendLine();
			var FlagText = LanguageManager.Get("m.flag");

			foreach (var Flag in FlagsController.GetFlags()) {
				if (!Flag.Team.Enable) continue;
				
				var Color = Flag.Team.ColorFormat ?? Globals.FORMAT_WHITE;
				var Capturer = Flag.Capturer;

				Builder.Append(Color).Append(FlagText);
				
				if (Capturer == null) {
					Builder.AppendLine();
					continue;
				}

				var CapturerColor = Capturer.Extra().Team.ColorFormat;
				Builder.Append(Globals.FORMAT_WHITE).Append(" - ").Append(CapturerColor)
					.AppendLine(Capturer.Data.PlayerName);
			}
		}

		public override bool CanVent(PlayerControl Player) {
			if (Military.FlagCapturerVents.GetValue()) {
				return true;
			} else {
				return FlagsController.GetCarryingFlag(Player) == null;
			}
		}
	}
}