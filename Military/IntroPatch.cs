using System.Linq;
using HarmonyLib;
using Military.Logic;
using Military.Logic.Mode;
using XenoCore.Locale;
using XenoCore.Utils;

namespace Military {
	[HarmonyPatch(typeof(IntroCutscene.CoBegin__d), nameof(IntroCutscene.CoBegin__d.MoveNext))]
	public class IntroCutscenePath {
		public static void Prefix(IntroCutscene.CoBegin__d __instance) {
			var PlayerTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
			var Team = PlayerControl.LocalPlayer.Extra().Team;
			
			PlayerTeam.Add(PlayerControl.LocalPlayer);
			foreach (var Player in Team.Players
				.Where(Player => Player.PlayerId != PlayerControl.LocalPlayer.PlayerId)) {
				PlayerTeam.Add(Player);
			}
			
			__instance.yourTeam = PlayerTeam;
		}

		public static void Postfix(IntroCutscene.CoBegin__d __instance) {
			var Extra = PlayerControl.LocalPlayer.Extra();
			var Team = Extra.Team;
			var Role = Extra.Role;

			var TeamName = LanguageManager.Get($"m.team.{Team.Name}.prefix");
			var RoleName = LanguageManager.Get($"m.{Role.Id}");

			var PersonText = LanguageManager.Get("m.person")
				.Replace("%t", TeamName)
				.Replace("%r", RoleName);
			var ModeBrief = LanguageManager.Get($"m.gamemode.{GameMode.Current.Id}.desc")
				.Replace("%c", $"[{Team.Color.ToHexRGBA()}]")
				.Replace("%whom", LanguageManager.Get($"m.team.{Team.Name}.whom"))
				.Replace("%which", LanguageManager.Get($"m.team.{Team.Name}.which"))
				.Replace("%r", Globals.FORMAT_WHITE);

			__instance.__this.Title.Text = PersonText;
			__instance.__this.Title.Color = Team.Color;
			__instance.__this.ImpostorText.Text = ModeBrief;
			__instance.__this.BackgroundBar.material.color = Team.Color;
			
			HudManager.Instance.Chat.SetVisible(true);

			Roles.Role.RespawnAll();
			GunController.UpdateForPlayer();
		}
	}
}