using System.Collections.Generic;
using System.Linq;

namespace Military.NetworkControllers {
	public static class TeamAffinityController {
		private static readonly Dictionary<Team, List<byte>> Affinities =
			new Dictionary<Team, List<byte>>();

		private static Team DefaultTeam;

		public static void Init() {
			foreach (var Team in TeamsController.GetTeams()) {
				Affinities[Team] = new List<byte>();
			}

			DefaultTeam = TeamsController.GetTeams().First();
		}

		public static void Reset() {
			foreach (var (_, Players) in Affinities) {
				Players.Clear();
			}
		}

		private static void RemovePlayer(byte PlayerId) {
			foreach (var (_, Players) in Affinities) {
				Players.Remove(PlayerId);
			}
		}
		
		public static void SetAffinity(PlayerControl Player, Team Team) {
			var PlayerId = Player.PlayerId;
			
			RemovePlayer(PlayerId);
			Affinities[Team].Add(PlayerId);
		}

		private static bool ContainsPlayer(PlayerControl Player) {
			foreach (var (_, Players) in Affinities) {
				if (Players.Contains(Player.PlayerId)) return true;
			}

			return false;
		}

		public static void FillEmpty() {
			foreach (var Control in PlayerControl.AllPlayerControls) {
				if (ContainsPlayer(Control)) continue;
				
				SetAffinity(Control, DefaultTeam);
			}
		}

		public static Dictionary<Team, List<byte>> GetAffinities() {
			return Affinities;
		}
	}
}