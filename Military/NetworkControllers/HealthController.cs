using System;
using System.Collections.Generic;
using Hazel;
using Military.Helpers;
using Military.Logic;
using Military.Logic.Mode;
using Military.Roles;
using UnityEngine;
using XenoCore.Buttons;
using XenoCore.Core;
using XenoCore.Utils;

namespace Military.NetworkControllers {
	public static class HealthController {
		private static readonly Dictionary<byte, ExtraData> ExtraData
			= new Dictionary<byte, ExtraData>();

		public static void ResetAll() {
			ExtraData.Clear();
		}

		public static ExtraData Extra(this PlayerControl Player) {
			if (!ExtraData.ContainsKey(Player.PlayerId)) {
				ExtraData.Add(Player.PlayerId, new ExtraData(Player));
			}
			
			return ExtraData[Player.PlayerId];
		}
		
		public static void RegisterMessages(XenoMod Mod) {
			Mod.RegisterMessage(NetworkControllers.ExtraData.DamageMessage.INSTANCE);
			Mod.RegisterMessage(NetworkControllers.ExtraData.RespawnMessage.INSTANCE);
		}
	}

	public class ExtraData {
		public readonly PlayerControl Player;
		public int MaxHealth { get; private set; }
		public int Health { get; private set; }
		public Role Role { get; private set; }
		public Team Team { get; private set; }
		
		public ExtraState State { get; private set; }
		private CooldownController RespawnCooldown { get; }

		private HealthBar HealthBar;

		public ExtraData(PlayerControl Player) {
			this.Player = Player;
			RespawnCooldown = new CooldownController(new RespawnCooldownProvider());
		}

		public bool IsReady() {
			return State == ExtraState.READY;
		}

		public void SetHealthBarActive(bool Active) {
			HealthBar?.SetActive(Active);
		}

		public void ResetHealth() {
			Health = MaxHealth;
			UpdateHealth();
		}

		private void UpdateHealth() {
			HealthBar?.SetHealth(MaxHealth == 0 ? 0 : (float) Health / MaxHealth);
		}

		public void Send(MessageWriter Writer) {
			Writer.Write(Player.PlayerId);
			Writer.Write(Role.RoleNo);
		}

		public static PlayerControl Read(MessageReader Reader) {
			var Player = PlayerTools.GetPlayerById(Reader.ReadByte());
			var RoleNo = Reader.ReadByte();

			if (Player == null) {
				throw new Exception("Player is null at ExtraController.Read");
			}
			Role.ROLES[RoleNo].AddPlayer(Player);

			return Player;
		}

		public void SetTeam(Team NewTeam) {
			Team = NewTeam;
			
			HealthBar?.Destroy();
			HealthBar = Team.HealthBar.Build(Player);
			Respawn();
		}

		public void SetRole(Role NewRole) {
			Role = NewRole;
			MaxHealth = Role.Health;
			Respawn();
		}

		private void SetState(ExtraState NewState) {
			if (NewState == ExtraState.SPAWNING) {
				GameMode.Current?.HandlePlayerRespawn(Player);
				
				RespawnCooldown.LastUsedFromNow(0);
				Player.moveable = false;
			} else if (NewState == ExtraState.READY) {
				try {
					Player.MyPhysics.ResetAnim();
				} catch (Exception) {
					// Ignored
					// Временное решение
				}

				ResetHealth();
				
				Player.moveable = true;
			}
			
			State = NewState;
		}

		public void Respawn() {
			SetState(ExtraState.SPAWNING);
		}

		private void DamageSfx(int Damage) {
			if (Damage >= 0) {
				Player.KillSfx.PlayPositioned(Player.GetTruePosition(),
					AudioManager.EffectsScale(0.8f));
				ExtraResources.BLOOD.OneTimeAnimate(Player.transform);
			} else {
				ExtraResources.SND_HEAL.PlayPositioned(Player.GetTruePosition(),
					AudioManager.EffectsScale(0.8f));
			}
		}

		public bool DoDamage(int Damage) {
			Health -= Damage;

			if (Health > MaxHealth) {
				Health = MaxHealth;
			}
			
			DamageMessage.INSTANCE.Send(this, Damage);
			DamageSfx(Damage);
			UpdateHealth();

			if (Health <= 0) {
				RespawnMessage.INSTANCE.Send(this);
				Respawn();
				return true;
			}

			return false;
		}

		public void Update() {
			switch (State) {
				case ExtraState.SPAWNING: {
					Player.moveable = false;
					try {
						Player.MyPhysics.body.velocity = Vector2.zero;
					} catch (Exception) {
						// ignored
						// Временное решение
					}

					if (RespawnCooldown.IsReady()) {
						SetState(ExtraState.READY);
					}
					break;
				}
				case ExtraState.READY: {
					break;
				}
			}
		}
		
		public enum ExtraState {
			SPAWNING,
			READY,
		}

		private class RespawnCooldownProvider : INumberProvider {
			public float GetValue() {
				return Military.RespawnTime.GetValue();
			}
		}

		internal class DamageMessage : Message {
			public static readonly DamageMessage INSTANCE = new DamageMessage();

			private DamageMessage() {
			}
		
			protected override void Handle() {
				var Data = ReadPlayer().Extra();
				Data.Health = Reader.ReadInt32();
				Data.UpdateHealth();
				Data.DamageSfx(Reader.ReadInt32());
			}

			public void Send(ExtraData Data, int Damage) {
				Write(Writer => {
					WritePlayer(Data.Player);
					Writer.Write(Data.Health);
					Writer.Write(Damage);
				});
			}
		}
		
		internal class RespawnMessage : Message {
			public static readonly RespawnMessage INSTANCE = new RespawnMessage();

			private RespawnMessage() {
			}
		
			protected override void Handle() {
				ReadPlayer().Extra().Respawn();
			}

			public void Send(ExtraData Data) {
				Write(Writer => {
					WritePlayer(Data.Player);
				});
			}
		}
	}
}