using System.Linq;
using HarmonyLib;

namespace Military.Logic {
	[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
	public static class TasksPatch {
		public static bool Prefix(ShipStatus __instance) {
			__instance.numScans = 0;
			AssignTaskIndexes(__instance);
			foreach (var Player in PlayerControl.AllPlayerControls) {
				var Tasks = CustomTasksController.GetTasks();

				var TaskIds = Tasks.Select(Task => (byte) Task.Index).ToArray();
				GameData.Instance.RpcSetTasks(Player.PlayerId, TaskIds);				
			}
			__instance.enabled = true;

			return false;
		}

		private static void AssignTaskIndexes(ShipStatus Status) {
			var num = 0;
			foreach (var Task in Status.CommonTasks) {
				Task.Index = num++;
			}

			foreach (var Task in Status.LongTasks) {
				Task.Index = num++;
			}

			foreach (var Task in Status.NormalTasks) {
				Task.Index = num++;
			}
		}
	}
}