using Military.NetworkControllers;

namespace Military.Logic.Mode.TDM {
	public class TeamDeathMatch : GameMode {
		public static readonly GameMode INSTANCE = new TeamDeathMatch();

		public override string Id => "tdm";

		public override void HandleKill(PlayerControl From, PlayerControl Target) {
			base.HandleKill(From, Target);
			
			ScoreController.AddScore(From.Extra().Team);
		}
	}
}