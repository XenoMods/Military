using HarmonyLib;
using Military.NetworkControllers;
using UnhollowerBaseLib;
using XenoCore.Utils;

namespace Military {
	[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
	public class AssignRolesPatch {
		public static void Prefix(ref Il2CppReferenceArray<GameData.PlayerInfo> JPGEIBIBJPJ) {
			JPGEIBIBJPJ = new Il2CppReferenceArray<GameData.PlayerInfo>(0);
			
			if (!Game.IsHost()) return;
			TeamsController.AssignTeamsServer();
		}
	}
}