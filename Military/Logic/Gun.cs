using Military.Helpers;
using UnityEngine;
using XenoCore.Buttons.Strategy;
using XenoCore.Utils;

namespace Military.Logic {
	public class Gun {
		public readonly Sprite Icon;
		public readonly int Damage;
		public readonly int Cooldown;
		public readonly float Distance;
		public readonly Sprite ButtonIcon;
		public readonly ButtonTargeter Targeter;
		public readonly Color OutlineColor;

		public Gun(BundleDefinition Bundle, string Name, int Damage, int Cooldown, float Distance)
			: this(Bundle, Name, Damage, Cooldown, Distance, ExtraResources.SHOOT,
				StandardGunTargeter.INSTANCE, Palette.ImpostorRed) {
		}

		public Gun(BundleDefinition Bundle, string Name, int Damage, int Cooldown, float Distance,
			Sprite ButtonIcon, ButtonTargeter Targeter, Color OutlineColor) {
			this.Damage = Damage;
			this.Cooldown = Cooldown;
			this.Distance = Distance;
			this.Targeter = Targeter;
			this.OutlineColor = OutlineColor;
			this.ButtonIcon = ButtonIcon;
			Icon = Bundle.Sprite(Name);
		}
	}
}