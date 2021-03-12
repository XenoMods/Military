using System.Collections.Generic;
using Military.Helpers;
using Military.Logic;
using UnityEngine;

namespace Military.Roles {
	public sealed class UberSoldierRole : Role {
		public static readonly UberSoldierRole INSTANCE = new UberSoldierRole();

		public override List<Gun> Guns => new List<Gun> {
			ExtraResources.GUN_KNIFE,
			ExtraResources.GUN_CHAINSAW,
			ExtraResources.GUN_MP43,
			ExtraResources.GUN_LASER_RIFLE,
			ExtraResources.GUN_MEDPACK,
		};

		// Runtime

		// 47572b
		private UberSoldierRole() : base("uber",
			new Color(71f / 255f, 87f / 255f, 43f / 255f, 1)) {
		}

		protected override void ResetRuntime() {
		}
	}
}