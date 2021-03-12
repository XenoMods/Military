using Hazel;
using Military.Logic;
using Military.Logic.Mode;
using XenoCore.Network;
using XenoCore.Utils;

namespace Military {
	public enum CustomRPC : byte {
		AssignTeamsAndRoles = 44,
		Shoot = 45,
		Winner = 46,
		CapturePoint = 47,
		AddPoints = 48,
		RequestCaptureFlag = 49,
		CaptureFlag = 50,
		FlagPoint = 51,
	}

	public class RPCExtraRoles : RPCListener {
		public void Handle(byte PacketId, MessageReader Reader) {
			switch (PacketId) {
				case (byte) CustomRPC.AssignTeamsAndRoles: {
					TeamsController.AssignTeamsClient(Reader);
					break;
				}
				case (byte) CustomRPC.Shoot: {
					var PlayerId = Reader.ReadByte();
					var Damage = Reader.ReadInt32();
					var Player = PlayerTools.GetPlayerById(PlayerId);
					if (Player == null) return;

					Player.Extra().DoDamage(Damage);

					break;
				}
				case (byte) CustomRPC.Winner: {
					Team.Read(Reader).Win();
					break;
				}
				case (byte) CustomRPC.CapturePoint: {
					GameMode.Current.HandleCapturePoint(Reader.ReadInt32(),
						Team.Read(Reader));
					break;
				}
				case (byte) CustomRPC.AddPoints: {
					Team.Read(Reader).AddPoint(Reader.ReadInt32());
					break;
				}
				case (byte) CustomRPC.RequestCaptureFlag: {
					FlagCaptureController.RequestCaptureFlag(Reader.ReadInt32(),
						Reader.ReadByte());
					break;
				}
				case (byte) CustomRPC.CaptureFlag: {
					CaptureTheFlag.INSTANCE.GetFlagById(Reader.ReadInt32())
						.SetCapturer(Reader.ReadInt32(), Reader.ReadBoolean());
					break;
				}
				case (byte) CustomRPC.FlagPoint: {
					FlagCaptureController.FlagPoint(Reader.ReadInt32(),
						Reader.ReadInt32(),
						Reader.ReadByte());
					break;
				}
			}
		}
	}
}