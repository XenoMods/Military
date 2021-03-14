using Military.NetworkControllers;
using XenoCore.Buttons.Strategy;
using XenoCore.Utils;

namespace Military.Logic {
	public static class MilitaryTargeterStatics {
		public static Team PlayerTeam;
		public static float KillRange;
	}

	public static class CommonGunTargeterUtil {
		public static PlayerControl GetTarget(bool Active, bool Friendly) {
			if (!Active || PlayerControl.LocalPlayer.inVent) return null;
			
			PlayerTools.CalculateClosest(PlayerControl.LocalPlayer,
				out var ClosestPlayer, out var ClosestDistance,
				SomePlayer => {
					var Extra = SomePlayer.Extra();

					return (Friendly
						       ? Extra.Team == MilitaryTargeterStatics.PlayerTeam
						       : Extra.Team != MilitaryTargeterStatics.PlayerTeam)
					       && Extra.IsReady();
				});
				
			return ClosestDistance < MilitaryTargeterStatics.KillRange
				? ClosestPlayer
				: null;
		}
	}
	
	public class StandardGunTargeter : ButtonTargeter {
		public static readonly StandardGunTargeter INSTANCE = new StandardGunTargeter();

		private StandardGunTargeter() {
		}

		public PlayerControl GetTarget(bool Active) {
			return CommonGunTargeterUtil.GetTarget(Active, false);
		}
	}
	
	public class HealingGunTargeter : ButtonTargeter {
		public static readonly HealingGunTargeter INSTANCE = new HealingGunTargeter();

		private HealingGunTargeter() {
		}

		public PlayerControl GetTarget(bool Active) {
			return CommonGunTargeterUtil.GetTarget(Active, true);
		}
	}
}