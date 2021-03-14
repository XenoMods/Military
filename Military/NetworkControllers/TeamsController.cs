using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using Military.Helpers;
using Military.Logic;
using Military.Roles;
using UnityEngine;
using XenoCore.Core;
using XenoCore.CustomOptions;
using XenoCore.Locale;
using XenoCore.Utils;

namespace Military.NetworkControllers {
	public static class TeamsController {
		private static readonly List<Team> Teams = new List<Team>();

		public static void Init() {
			CustomOption.AddTitle("").Group = Team.GROUP_TEAMS;
			CustomOption.AddTitle(LanguageManager.Get("m.teams")).Group = Team.GROUP_TEAMS;
			
			Teams.Add(new Team(Color.red, "Red", ExtraResources.CAPTURE_RED));
			Teams.Add(new Team(Color.blue, "Blue", ExtraResources.CAPTURE_BLUE));
			Teams.Add(new Team(Color.green, "Green", ExtraResources.CAPTURE_GREEN));
			Teams.Add(new Team(Color.yellow, "Yellow", ExtraResources.CAPTURE_YELLOW));
			CustomOption.AddTitle("").Group = Team.GROUP_TEAMS;
			CustomOption.AddTitle(LanguageManager.Get("m.roles")).Group = Team.GROUP_TEAMS;
		}

		public static List<Team> GetTeams() {
			return Teams;
		}

		public static void ResetAll() {
			foreach (var Team in Teams) {
				Team.Players.Clear();
			}
		}

		public static void AssignTeamsClient(MessageReader Reader) {
			var Count = Reader.ReadInt32();

			for (var Index = 0; Index < Count; Index++) {
				Teams[Reader.ReadInt32()].ComplexRead(Reader);
			}
		}

		public static void AssignTeamsServer() {
			var Players = PlayerControl.AllPlayerControls.ToArray().Shuffle();
			var TeamIndex = 0;
			var EnabledTeams = Teams.Where(Team => Team.Enable).ToList();

			if (EnabledTeams.Count == 0) {
				throw new Exception("No teams enabled");
			}

			foreach (var Player in Players) {
				var CurrentTeam = EnabledTeams[TeamIndex];
				TeamIndex = (TeamIndex + 1) % EnabledTeams.Count;

				CurrentTeam.Players.Add(Player);
				Player.Extra().SetTeam(CurrentTeam);
			}

			foreach (var Team in Teams) {
				Team.AssignRoles();
			}

			AssignTeamsAndRolesMessage.INSTANCE.Send(Teams);
		}
		
		public static Team ReadTeam(this MessageReader Reader) {
			var Id = Reader.ReadInt32();
			return Id == -1 ? null : Teams[Id];
		}

		public static void WriteTeam(this MessageWriter Writer, Team Team) {
			Writer.Write(Team?.TeamId ?? -1);
		}

		public static void RegisterMessages(XenoMod Mod) {
		}
	}

	public class Team {
		public static readonly GameObject NONE_POINT_PREFAB = ExtraResources.POINT_NONE;
		public static readonly OptionGroup GROUP_TEAMS = Military.ER_GROUP.Down();
		
		// Settings
		private readonly CustomToggleOption _Enable;
		
		public bool Enable => _Enable.GetValue();
		
		private static int CurrentTeamId;
		public readonly int TeamId;
		
		public readonly Color Color;
		public readonly Color ProtectionColor;
		public readonly string Name;
		public readonly List<PlayerControl> Players = new List<PlayerControl>();
		public readonly HealthBar HealthBar;
		public readonly Sprite CaptureIcon;
		public readonly string ColorFormat;

		public Team(Color Color, string Name, Sprite CaptureIcon) {
			TeamId = CurrentTeamId;
			CurrentTeamId++;
			
			this.Color = Color;
			ProtectionColor = new Color(Color.r, Color.g, Color.b, 0.5f);
			HealthBar = new HealthBar(ExtraResources.BUNDLE, Name);
			Name = Name.ToLowerInvariant();
			this.Name = Name;
			this.CaptureIcon = CaptureIcon;

			ColorFormat = $"[{Color.ToHexRGBA()}]";
			var Arguments = new Dictionary<string, Func<string>> {
				{"%c", () => ColorFormat},
				{"%w", () => LanguageManager.Get($"m.team.{Name}.whom")}
			};
			_Enable = MakeTeamToggle(Name, "enable", Arguments, GROUP_TEAMS);
		}

		public bool Compare(Team Team) {
			return Team != null && TeamId == Team.TeamId;
		}
		
		private static CustomToggleOption MakeTeamToggle(string Id, string Type,
			Dictionary<string, Func<string>> Arguments, OptionGroup Group) {
			var Result = CustomOption.AddToggle($"m.team.{Id}.{Type}",
				$"m.team.{Type}", false);
			Result.LocalizationArguments = Arguments;
			Result.Group = Group;
			return Result;
		}

		public void AssignRoles() {
			var Roles = Role.ROLES.Where(Role => Role.Enable).ToList();

			foreach (var Player in Players) {
				Roles.RandomItem().AddPlayer(Player);
			}
		}

		public void ComplexSend(MessageWriter Writer) {
			Writer.Write(TeamId);

			Writer.Write(Players.Count);
			foreach (var Player in Players) {
				Player.Extra().Send(Writer);
			}
		}

		public void ComplexRead(MessageReader Reader) {
			var PlayersCount = Reader.ReadInt32();

			for (var Index = 0; Index < PlayersCount; Index++) {
				var Player = ExtraData.Read(Reader);
				Player.Extra().SetTeam(this);
				Players.Add(Player);
			}
		}

		public void Win() {
			ScoreController.Reset();
			EndGameCentral.WinnerTeam = this;

			ShipStatus.Instance.enabled = false;
			
			AmongUsClient.Instance.allClients.Clear();
			AmongUsClient.Instance.GameState = InnerNetClient.GameStates.Ended;
			AmongUsClient.Instance.OnGameEnd(GameOverReason.ImpostorByKill,
				false);
			
			var val = AmongUsClient.Instance.StartEndGame();
			val.Write((byte) GameOverReason.ImpostorByKill);
			val.Write(false);
			AmongUsClient.Instance.FinishEndGame(val);
		}
		
		public static void FixIfNoEnabled() {
			if (!TeamsController.GetTeams().Any(Role => Role.Enable)) {
				TeamsController.GetTeams().First()._Enable.SetValue(true);
			}
		}
	}
}