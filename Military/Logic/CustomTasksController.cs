using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Military.Logic {
	public static class CustomTasksController {
		private static readonly List<NormalPlayerTask> Tasks = new List<NormalPlayerTask>();
		private static int CurrentTaskIndex = 200;
		private static bool Initialized;

		public static void ResetAll() {
			if (Initialized) return;

			// var Task = BuildTask(new Vector2(0f, 0f));
			// Tasks.Add(Task);

			Initialized = true;
		}

		public static List<NormalPlayerTask> GetTasks() {
			if (!Initialized) {
				ResetAll();
			}
			
			return Tasks;
		}
		
		private static NormalPlayerTask BuildTask(Vector2 Position) {
			var Task = new GameObject().AddComponent<NormalPlayerTask>();
			Task.transform.position = Position;
			Task.Index = CurrentTaskIndex;
			Task.MaxStep = 1;
			Task.taskStep = 0;
			CurrentTaskIndex++;
			return Task;
		}

		public static NormalPlayerTask GetById(byte TaskId) {
			return Tasks.FirstOrDefault(Task => Task.Index == TaskId);
		}
	}
}