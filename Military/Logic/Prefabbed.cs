using UnityEngine;

namespace Military.Logic {
	public abstract class Prefabbed {
		protected readonly GameObject Prefab;
		protected readonly Transform Transform;

		protected Prefabbed(GameObject Prefab) {
			this.Prefab = Prefab;
			Transform = Prefab.transform;
		}
		
		public void Destroy() {
			Object.Destroy(Prefab);
		}
	}
}