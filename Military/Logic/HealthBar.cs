using UnityEngine;
using XenoCore.Utils;

namespace Military.Logic {
	public class HealthBar : Prefabbed {
		private static readonly int HEALTH = Shader.PropertyToID("_Health");
		private readonly SpriteRenderer HealthRenderer;

		public HealthBar(BundleDefinition Bundle, string TeamName)
			: this(Bundle.Object($"{TeamName}Bar")) {
		}
		
		public HealthBar(GameObject Prefab) : base(Prefab) {
			HealthRenderer = Prefab.transform.GetChild(0).GetComponent<SpriteRenderer>();
		}

		public void SetActive(bool Active) {
			if (Prefab == null) return;
			
			Prefab.SetActive(Active);
		}

		public void SetHealth(float Health) {
			if (HealthRenderer == null) return;
			
			HealthRenderer.material.SetFloat(HEALTH, Health);
		}

		public HealthBar Build(PlayerControl Player) {
			var Instance = Object.Instantiate(Prefab, Player.nameText.transform);
			Instance.transform.localPosition = new Vector3(0, 0.15f, 0f);
			return new HealthBar(Instance);
		}
	}
}