using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using Military.Component;
using Military.Helpers;
using Military.Logic;
using Military.Logic.Mode;
using Military.Roles;
using Reactor;
using UnityEngine;
using XenoCore;
using XenoCore.CustomOptions;
using XenoCore.Events;
using XenoCore.Locale;
using XenoCore.Network;
using XenoCore.Override;
using XenoCore.Override.Map;
using XenoCore.Override.Map.Components;
using XenoCore.Override.Tasks;
using XenoCore.Utils;

namespace Military {
	[BepInPlugin(Id)]
	[BepInProcess(Globals.PROCESS)]
	[BepInDependency(ReactorPlugin.Id)]
	[BepInDependency(XenoPlugin.Id)]
	// ReSharper disable once ClassNeverInstantiated.Global
	public class Military : BasePlugin {
		public const string Id = "com.mishin870.military";
		public static readonly string Version = "1.0.0";

		public Harmony Harmony { get; } = new Harmony(Id);

		public static readonly OptionGroup ER_GROUP = CustomOption.DEFAULT_GROUP.Up(20);

		private static readonly TitleOption Dummy = CustomOption.AddTitle("");
		public static readonly CustomToggleOption Vents = CustomOption.AddToggle("m.vents",
			true);
		public static readonly CustomNumberOption RespawnTime = CustomOption
			.AddNumber("m.cooldown.respawn", 5, 1, 100, 1);
		public static readonly CustomStringOption GameMode = CustomOption.AddString("m.gamemode",
			MakeGameModes());
		public static readonly CustomNumberOption MaxPoints = CustomOption
			.AddNumber("m.gamemode.max", 20, 1, 100, 1);
		public static readonly CustomNumberOption GameModeInterval = CustomOption
			.AddNumber("m.gamemode.interval", 10, 1, 100, 1);
		public static readonly CustomNumberOption ControlPointUseInterval = CustomOption
			.AddNumber("m.gamemode.cp.interval", 5, 1, 100, 1);
		public static readonly CustomToggleOption FlagCapturerVents = CustomOption
			.AddToggle("m.gamemode.ctf.vents", true);

		private static string[] MakeGameModes() {
			return Logic.Mode.GameMode.GetKeys().Select(Key => $"m.gamemode.{Key}").ToArray();
		}

		public override void Load() {
			Harmony.PatchAll();

			Dummy.Group = ER_GROUP;
			Vents.Group = ER_GROUP;
			GameMode.Group = ER_GROUP;
			RespawnTime.Group = ER_GROUP;
			MaxPoints.Group = ER_GROUP;
			GameModeInterval.Group = ER_GROUP;
			ControlPointUseInterval.Group = ER_GROUP;
			FlagCapturerVents.Group = ER_GROUP;

			TasksOverlay.Enable();
			
			LanguageManager.Load(Assembly.GetExecutingAssembly(), "Military.Lang.");
			Role.Init();
			VersionsList.Add("Military", Version, true);

			TeamsController.Init();
			HandleRpcPatch.AddListener(new RPCExtraRoles());

			RegisterComponents();
			RegisterListeners();
			RegisterCustomMaps();
		}

		private static void RegisterComponents() {
			PseudoComponentsRegistry.Register(new FlagComponentBuilder());
			PseudoComponentsRegistry.Register(new PointComponentBuilder());
		}

		[SuppressMessage("ReSharper", "ConvertClosureToMethodGroup")]
		private static void RegisterListeners() {
			EventsController.RESET_ALL.Register(() => {
				CustomTasksController.ResetAll();
				Team.FixIfNoEnabled();
				Role.FixIfNoEnabled();

				TeamsController.ResetAll();
				ModActions.ResetAll();
				ExtraController.ResetAll();
				EndGameCentral.ResetAll();
				Logic.Mode.GameMode.ResetAll();
			});
			EventsController.GAME_STARTED.Register(() => {
				DoorsController.DestroyDoors();
			});
			EventsController.MAP_INIT.Register(() => {
				Logic.Mode.GameMode.Current.InitMap();
			});
		}

		private static void RegisterCustomMaps() {
			CustomMapController.Register(new CustomMapType {
				Name = "castle",
				CameraShakeAmount = 0f,
				CameraShakePeriod = 0f,
				MapPrefab = ExtraResources.MAP_CASTLE,
				MiniMap = ExtraResources.MINIMAP_CASTLE,
				MapScale = 7.65f,
				MapOffset = new Vector2(-0.85f, 1.2f),
			});
			
			CustomMapController.Register(new CustomMapType {
				Name = "dust",
				CameraShakeAmount = 0.0f,
				CameraShakePeriod = 0.0f,
				MapPrefab = ExtraResources.MAP_DUST,
			});

			CustomMapController.AddSpawnModifier(new GameModeSpawnModifier());
		}
	}
}