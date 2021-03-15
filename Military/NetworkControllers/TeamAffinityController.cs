using System.Collections.Generic;
using Hazel;
using XenoCore.Core;
using XenoCore.Utils;

namespace Military.NetworkControllers {
	public static class TeamAffinityController {
		private static readonly Dictionary<Team, List<byte>> Affinities =
			new Dictionary<Team, List<byte>>();

		public static void Init() {
			foreach (var Team in TeamsController.GetTeams()) {
				Affinities[Team] = new List<byte>();
			}
		}

		public static void Reset() {
			foreach (var (_, Players) in Affinities) {
				Players.Clear();
			}
			
			Synchronize();
		}

		private static void RemovePlayer(byte PlayerId) {
			foreach (var (_, Players) in Affinities) {
				Players.Remove(PlayerId);
			}
			
			Synchronize();
		}
		
		public static void SetAffinity(PlayerControl Player, Team Team) {
			var PlayerId = Player.PlayerId;
			
			RemovePlayer(PlayerId);
			Affinities[Team].Add(PlayerId);
			
			Synchronize();
		}

		public static Dictionary<Team, List<byte>> GetAffinities() {
			return Affinities;
		}

		public static void TeamDisabled(Team Team) {
			Affinities[Team].Clear();
			Synchronize();
		}
		
		public static void RegisterMessages(XenoMod Mod) {
			Mod.RegisterMessage(TeamAffinitySyncMessage.INSTANCE);
		}

		private static void Synchronize() {
			if (!Game.IsHost()) return;
			
			TeamAffinitySyncMessage.INSTANCE.Send(Affinities);
		}

		public static void ReadSync(MessageReader Reader) {
			var Count = Reader.ReadInt32();

			for (var i = 0; i < Count; i++) {
				var Team = Reader.ReadTeam();
				Affinities[Team].Clear();
				var PlayersCount = Reader.ReadInt32();

				for (var x = 0; x < PlayersCount; x++) {
					Affinities[Team].Add(Reader.ReadByte());
				}
			}
		}
	}
	
	internal class TeamAffinitySyncMessage : Message {
		public static readonly TeamAffinitySyncMessage INSTANCE = new TeamAffinitySyncMessage();

		private TeamAffinitySyncMessage() {
		}

		protected override void Handle() {
			TeamAffinityController.ReadSync(Reader);
		}

		public void Send(Dictionary<Team, List<byte>> Affinities) {
			Write(Writer => {
				Writer.Write(Affinities.Count);

				foreach (var (Team, Players) in Affinities) {
					Writer.WriteTeam(Team);
					Writer.Write(Players.Count);
					
					foreach (var Player in Players) {
						Writer.Write(Player);
					}
				}
			});
		}
	}
}