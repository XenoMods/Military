using System;
using System.Collections.Generic;
using System.Linq;
using Military.Logic;
using Military.NetworkControllers;
using UnityEngine;
using XenoCore.Buttons;
using XenoCore.CustomOptions;
using XenoCore.Locale;
using XenoCore.Utils;

namespace Military.Roles {
	public abstract class Role {
		public static readonly OptionGroup GROUP_ENABLE = Team.GROUP_TEAMS.Down();
		public static readonly OptionGroup GROUP_COOLDOWN = GROUP_ENABLE.Down();

		// Global
		public readonly string Id;
		protected string Prefix => $"m.{Id}";

		public Color Color { get; }
		public virtual List<Gun> Guns { get; }

		// Settings
		private readonly CustomToggleOption _Enable;
		private readonly CustomNumberOption _Health;
		
		public bool Enable => _Enable.GetValue();
		public int Health => (int) _Health.GetValue();

		// Hard runtime
		public byte RoleNo { get; private set; }

		// Runtime
		private readonly HashSet<PlayerControl> Players = new HashSet<PlayerControl>();

		// Actions
		protected CooldownController Cooldown { get; private set; }

		public static readonly List<Role> ROLES = new List<Role>();

		protected Role(string Id, Color RoleColor) {
			this.Id = Id;
			Color = RoleColor;

			var RoleHexColor = $"[{RoleColor.ToHexRGBA()}]";
			var Arguments = new Dictionary<string, Func<string>> {
				{"%c", () => RoleHexColor},
				{"%w", () => LanguageManager.Get($"{Prefix}.whom")}
			};

			_Enable = MakeRoleToggle(Id, "enable", Arguments, GROUP_ENABLE);

			var RoleTitle = CustomOption.AddTitle("m.role.title");
			RoleTitle.LocalizationArguments = new Dictionary<string, Func<string>> {
				{"%c", () => RoleHexColor},
				{"%n", () => LanguageManager.Get(Prefix)},
				{"%r", () => Globals.FORMAT_WHITE},
			};

			_Health = MakeNumber("health", 20, 1, 200, 1);
		}

		private static CustomToggleOption MakeRoleToggle(string Id, string Type,
			Dictionary<string, Func<string>> Arguments, OptionGroup Group) {
			var Result = CustomOption.AddToggle($"m.{Id}.{Type}",
				$"m.role.{Type}", false);
			Result.LocalizationArguments = Arguments;
			Result.Group = Group;
			return Result;
		}

		#region HELPERS

		protected void CreateCooldown() {
			Cooldown = CooldownController.FromOption(Prefix);
		}

		protected void ClearTasks() {
			foreach (var Player in Players) {
				var ToRemove = new List<PlayerTask>();

				foreach (var Task in Player.myTasks) {
					if (!Task.TaskType.IsSabotage()) {
						ToRemove.Add(Task);
					}
				}

				foreach (var Task in ToRemove) {
					Player.RemoveTask(Task);
				}				
			}
		}

		protected static void SetupOwnIntroTeam(IntroCutscene.CoBegin__d Cutscene) {
			var Team = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
			Team.Add(PlayerControl.LocalPlayer);
			Cutscene.yourTeam = Team;
		}

		#endregion

		#region SETTINGS

		protected string MakeOptionId(string Name) {
			return $"{Prefix}.{Name}";
		}

		protected CustomToggleOption MakeToggle(string Name, bool Value) {
			return CustomOption.AddToggle(MakeOptionId(Name), Value);
		}

		protected CustomNumberOption MakeNumber(string Name, float Value, float Min,
			float Max, float Increment) {
			return CustomOption.AddNumber(MakeOptionId(Name), Value, Min, Max, Increment);
		}

		#endregion

		#region RESET

		public void Reset() {
			Players.Clear();
			Cooldown?.Reset();

			ResetRuntime();
		}

		protected abstract void ResetRuntime();

		#endregion

		#region EVENTS

		protected virtual void InitForLocalPlayer() {
		}

		public virtual void PreUpdate(HudManager Manager, bool UseEnabled, bool Dead) {
		}

		public virtual void PostUpdate(HudManager Manager, bool UseEnabled, bool Dead) {
		}

		public virtual void DoAction(ActionType Type, bool Dead, ref bool Acted) {
		}

		public virtual void ShowInfectedMap(MapBehaviour Map) {
		}

		public virtual void UpdateMap(MapBehaviour Map) {
		}

		public virtual void Intro(IntroCutscene.CoBegin__d Cutscene) {
			Cooldown?.UpdateForIntro(Cutscene);
		}

		public virtual void OnLocalDie(PlayerControl Victim, DeathReason Reason) {
		}

		public virtual void UpdateTasksVisual(HudManager Manager) {
		}

		#endregion

		public static void ResetAll() {
			foreach (var SomeRole in ROLES) {
				SomeRole.Reset();
			}
		}

		public static void Init() {
			ROLES.Add(UberSoldierRole.INSTANCE);

			for (var No = 0; No < ROLES.Count; No++) {
				ROLES[No].RoleNo = (byte) No;
			}
		}
		
		public void AddPlayer(PlayerControl Player) {
			Players.Add(Player);
			Player.Extra().SetRole(this);
		}

		public static void RespawnAll() {
			foreach (var SomeRole in ROLES) {
				SomeRole.Respawn();
			}
		}

		private void Respawn() {
			foreach (var ExtraData in Players.Select(Player => Player.Extra())) {
				ExtraData.ResetHealth();
			}
		}

		public static void FixIfNoEnabled() {
			if (!ROLES.Any(Role => Role.Enable)) {
				ROLES[0]._Enable.SetValue(true);
			}
		}
	}
}