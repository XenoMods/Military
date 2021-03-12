using System;
using Hazel;
using XenoCore.Utils;

namespace Military.Helpers {
	public static class ExtraNetwork {
		public static void Send(CustomRPC CustomRPC) {
			Network.Send((byte) CustomRPC);
		}

		public static void Send(CustomRPC CustomRPC, Action<MessageWriter> WriteData) {
			Network.Send((byte) CustomRPC, WriteData);
		}
	}
}