namespace Military.Logic.Mode {
	public class TeamDeathMatch : GameMode {
		public static readonly GameMode INSTANCE = new TeamDeathMatch();

		public override string Id => "tdm";

		public override void HandleKill() {
			AddPointToLocalPlayer();
		}
	}
}