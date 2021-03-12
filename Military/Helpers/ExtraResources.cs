using System.Reflection;
using Military.Logic;
using Reactor.Extensions;
using Reactor.Unstrip;
using UnityEngine;
using XenoCore.Utils;

namespace Military.Helpers {
	public static class ExtraResources {
		public static readonly ResourceLoader LOADER = new ResourceLoader("Military.Resources.",
			Assembly.GetExecutingAssembly());
		
		// Comic Sans MS 20
		public static readonly Sprite SHOOT = LOADER.Sprite("Shoot.png");
		public static readonly Sprite HEALING = LOADER.Sprite("Healing.png");
		public static readonly Sprite CAPTURE_RED = LOADER.Sprite("CaptureRed.png");
		public static readonly Sprite CAPTURE_GREEN = LOADER.Sprite("CaptureGreen.png");
		public static readonly Sprite CAPTURE_BLUE = LOADER.Sprite("CaptureBlue.png");
		public static readonly Sprite CAPTURE_YELLOW = LOADER.Sprite("CaptureYellow.png");

		public static readonly Sprite FLAG_ICON = LOADER.Sprite("FlagIcon.png");
		public static readonly Sprite POINT_ICON = LOADER.Sprite("PointIcon.png");

		public static readonly BundleDefinition BUNDLE = LOADER.Bundle("military");

		public static readonly Gun GUN_MEDPACK = new Gun(BUNDLE, "MedPack",
			-10, 5, 1.5f, HEALING, HealingGunTargeter.INSTANCE,
			Color.green);
		public static readonly Gun GUN_CHAINSAW = new Gun(BUNDLE, "Chainsaw",
			20, 8, 1f);
		public static readonly Gun GUN_LASER_RIFLE = new Gun(BUNDLE, "LaserRifle",
			9, 5, 3.5f);
		public static readonly Gun GUN_KNIFE = new Gun(BUNDLE, "Knife",
			5, 1, 1f);
		public static readonly Gun GUN_MP43 = new Gun(BUNDLE, "Mp43",
			5, 2, 3f);
		public static readonly Gun GUN_GERMAN_PISTOL = new Gun(BUNDLE, "GermanPistol",
			10, 5, 2f);

		public static readonly GameObject BLOOD = BUNDLE.Object("Blood");

		public static readonly GameObject POINT_RED = BUNDLE.Object("RedPoint");
		public static readonly GameObject POINT_GREEN = BUNDLE.Object("GreenPoint");
		public static readonly GameObject POINT_BLUE = BUNDLE.Object("BluePoint");
		public static readonly GameObject POINT_YELLOW = BUNDLE.Object("YellowPoint");
		public static readonly GameObject POINT_NONE = BUNDLE.Object("NonePoint");

		public static readonly GameObject FLAG = BUNDLE.Object("Flag");

		public static readonly AudioClip SND_POINT_ALLY = BUNDLE.Audio("PointAlly");
		public static readonly AudioClip SND_POINT_ENEMY = BUNDLE.Audio("PointEnemy");
		public static readonly AudioClip SND_FLAG_CAPTURED = BUNDLE.Audio("FlagCaptured");
		public static readonly AudioClip SND_FLAG_LOST = BUNDLE.Audio("FlagLost");
		public static readonly AudioClip SND_FLAG_TAKEN = BUNDLE.Audio("FlagTaken");

		public static readonly AudioClip SND_HEAL = BUNDLE.Audio("Heal");

		public static readonly GameObject MAP_DUST = BUNDLE.Object("MapDust");
		public static readonly GameObject MAP_CASTLE = BUNDLE.Object("MapCastle");

		public static readonly Sprite MINIMAP_CASTLE = BUNDLE.Sprite("MiniMapCastle");
	}
}