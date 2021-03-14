using HarmonyLib;
using Military.NetworkControllers;
using UnhollowerBaseLib;

namespace Military {
	[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
	public class AssignRolesPatch {
		public static void Prefix(ref Il2CppReferenceArray<GameData.PlayerInfo> JPGEIBIBJPJ) {
			JPGEIBIBJPJ = new Il2CppReferenceArray<GameData.PlayerInfo>(0);
		}

		public static void Postfix(Il2CppReferenceArray<GameData.PlayerInfo> JPGEIBIBJPJ) {
			if (!AmongUsClient.Instance.AmHost) return;
			TeamsController.AssignTeamsServer();
		}
	}
}