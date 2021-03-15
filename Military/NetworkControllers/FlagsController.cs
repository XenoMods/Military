using System.Collections.Generic;
using System.Linq;
using Hazel;
using Military.Logic.Mode.Flags;
using UnityEngine;
using XenoCore.Core;
using XenoCore.Utils;

namespace Military.NetworkControllers {
	public static class FlagsController {
		private static readonly List<Flag> Flags = new List<Flag>();
		private static readonly Dictionary<Team, Flag> TeamFlags = new Dictionary<Team, Flag>();

		public static void Reset() {
			Flags.Clear();
			TeamFlags.Clear();
		}
		
		public static void AddFlag(Vector2 Position, Team Team) {
			var Flag = new Flag(Flags.Count, Team, Position);
			
			Flags.Add(Flag);
			TeamFlags.Add(Flag.Team, Flag);
		}

		public static void Use(Flag Flag, PlayerControl Player) {
			var Extra = Player.Extra();
			if (!Extra.IsReady()) return;

			var PlayerTeam = Extra.Team;
			var CarryingFlag = GetCarryingFlag(Player);
			// Игрок нажал по флагу своего цвета
			if (Flag.Team.Compare(PlayerTeam)) {
				// Если игрок тащит на себе чужой флаг
				if (CarryingFlag != null) {
					SoundsController.FLAG_CAPTURED.BroadcastGlobally();
					CarryingFlag.Capturer = null;
					ScoreController.AddScore(PlayerTeam);
					Synchronize();
				}
				
				return;
			}

			if (CarryingFlag != null) {
				return;
			}

			SoundsController.FLAG_TAKEN.BroadcastGlobally();
			Flag.Capturer = Player;

			Synchronize();
		}

		public static void DropAll(PlayerControl Player) {
			foreach (var Flag in Flags.Where(Flag => Player.Compare(Flag.Capturer))) {
				SoundsController.FLAG_LOST.BroadcastGlobally();
				Flag.Capturer = null;
			}
			
			Synchronize();
		}

		public static Vector2? GetSpawn(PlayerControl Player) {
			var Team = Player.Extra().Team;
			if (Team == null || !TeamFlags.ContainsKey(Team)) return null;
			
			return TeamFlags[Team]
				.OriginalPosition + new Vector2(0f, 0.3636f);
		}

		public static Flag GetCarryingFlag(PlayerControl Player) {
			return Flags.FirstOrDefault(Flag => Player.Compare(Flag.Capturer));
		}

		public static IEnumerable<Flag> GetFlags() {
			return Flags;
		}

		public static Flag ReadFlag(this MessageReader Reader) {
			return Flags[Reader.ReadInt32()];
		}

		public static void WriteFlag(this MessageWriter Writer, Flag Flag) {
			Writer.Write(Flag.No);
		}

		private static void Synchronize() {
			if (!AmongUsClient.Instance.AmHost) return;
			
			FlagsSyncMessage.INSTANCE.Send(Flags);
		}

		public static void ReadSync(MessageReader Reader) {
			var Count = Reader.ReadInt32();

			for (var i = 0; i < Count; i++) {
				var Flag = Reader.ReadFlag();
				Flag.ReadCapturer(Reader);
			}
		}

		public static void RegisterMessages(XenoMod Mod) {
			Mod.RegisterMessage(FlagUseMessage.INSTANCE);
			Mod.RegisterMessage(FlagsSyncMessage.INSTANCE);
		}
	}

	public static class FlagsPreController {
		public static void Use(Flag Flag) {
			if (Game.IsHost()) {
				FlagsController.Use(Flag, PlayerControl.LocalPlayer);
			} else {
				FlagUseMessage.INSTANCE.Send(Flag);
			}
		}
	}
	
	internal class FlagUseMessage : Message {
		public static readonly FlagUseMessage INSTANCE = new FlagUseMessage();

		private FlagUseMessage() {
		}

		protected override void Handle() {
			FlagsController.Use(Reader.ReadFlag(), ReadPlayer());
		}

		public void Send(Flag Flag) {
			Write(Writer => {
				Writer.WriteFlag(Flag);
				WriteLocalPlayer();
			});
		}
	}
	
	internal class FlagsSyncMessage : Message {
		public static readonly FlagsSyncMessage INSTANCE = new FlagsSyncMessage();

		private FlagsSyncMessage() {
		}

		protected override void Handle() {
			FlagsController.ReadSync(Reader);
		}

		public void Send(List<Flag> Flags) {
			Write(Writer => {
				Writer.Write(Flags.Count);

				foreach (var Flag in Flags) {
					Writer.WriteFlag(Flag);
					Flag.WriteCapturer(Writer);
				}
			});
		}
	}
}