using System.Collections.Generic;
using System.Linq;
using Military.NetworkControllers;
using XenoCore.Commands;
using XenoCore.Core;
using XenoCore.Locale;

namespace Military.Commands {
	public class TeamAffinityCommand : ICommand {
		public string Name() => "team";
		public string Usage() => $"/team [{string.Join('|', MakeTeams())}]";

		private static IEnumerable<string> MakeTeams() {
			return TeamsController.GetTeams().Select(Team => Team.Name);
		}

		public void Run(ChatCallback Callback, List<string> Args) {
			if (Args.Count == 0) {
				this.UsageError(Callback, "Укажите название команды!");
				return;
			}

			var TeamName = Args[0];
			var Team = TeamsController.GetByName(TeamName);

			if (Team == null) {
				this.UsageError(Callback, $"Команда {TeamName} не найдена!");
				return;
			}

			var Whom = LanguageManager.Get($"m.team.{TeamName}.whom");
			Callback.Send($"Вы успешно привязаны к команде {Whom}!");

			TeamAffinityController.SetAffinity(PlayerControl.LocalPlayer, Team);
			TeamAffinityMessage.INSTANCE.Send(Team);
		}
	}

	internal class TeamAffinityMessage : Message {
		public static readonly TeamAffinityMessage INSTANCE = new TeamAffinityMessage();

		private TeamAffinityMessage() {
		}

		protected override void Handle() {
			TeamAffinityController.SetAffinity(ReadPlayer(), Reader.ReadTeam());
		}

		public void Send(Team Team) {
			Write(Writer => {
				WriteLocalPlayer();
				Writer.WriteTeam(Team);
			});
		}
	}
}