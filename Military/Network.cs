using System.Collections.Generic;
using Military.NetworkControllers;
using XenoCore.Core;

namespace Military {
	internal class AssignTeamsAndRolesMessage : Message {
		public static readonly AssignTeamsAndRolesMessage INSTANCE = new AssignTeamsAndRolesMessage();

		private AssignTeamsAndRolesMessage() {
		}
		
		protected override void Handle() {
			TeamsController.AssignTeamsClient(Reader);
		}

		public void Send(List<Team> Teams) {
			Write(Writer => {
				Writer.Write(Teams.Count);

				foreach (var Team in Teams) {
					Team.ComplexSend(Writer);
				}
			});
		}
	}
}