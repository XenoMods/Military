using System.Collections.Generic;
using Hazel;
using Military.Helpers;
using UnityEngine;
using XenoCore.Core;
using XenoCore.Utils;

namespace Military.NetworkControllers {
	public static class SoundsController {
		public static readonly NetworkSound FLAG_CAPTURED = new NetworkSound(
			ExtraResources.SND_FLAG_CAPTURED, 1.5f);

		public static readonly NetworkSound FLAG_TAKEN = new NetworkSound(
			ExtraResources.SND_FLAG_TAKEN, 1.5f);

		public static readonly NetworkSound FLAG_LOST = new NetworkSound(
			ExtraResources.SND_FLAG_LOST, 1.5f);

		private static readonly List<NetworkSound> Sounds = new List<NetworkSound> {
			FLAG_CAPTURED,
			FLAG_TAKEN,
			FLAG_LOST
		};

		public static void RegisterMessages(XenoMod Mod) {
			Mod.RegisterMessage(NetworkSound.NetworkSoundMessage.INSTANCE);
		}

		public static NetworkSound ReadSound(this MessageReader Reader) {
			return Sounds[Reader.ReadInt32()];
		}

		public static void WriteSound(this MessageWriter Writer, NetworkSound Sound) {
			Writer.Write(Sound.Id);
		}

		public class NetworkSound {
			private static int CurrentId;

			public readonly int Id;
			private readonly AudioClip Clip;
			private readonly float Scale;
			private readonly bool IsEffect;

			public NetworkSound(AudioClip Clip, float Scale, bool IsEffect = true) {
				this.Clip = Clip;
				this.Scale = Scale;
				this.IsEffect = IsEffect;

				Id = CurrentId;
				CurrentId++;
			}

			private void PlayGlobally() {
				Clip.PlayGlobally(IsEffect
					? AudioManager.EffectsScale(Scale)
					: AudioManager.MusicScale(Scale));
			}

			public void BroadcastGlobally() {
				NetworkSoundMessage.INSTANCE.Send(this);
				PlayGlobally();
			}
			
			internal class NetworkSoundMessage : Message {
				public static readonly NetworkSoundMessage INSTANCE = new NetworkSoundMessage();

				private NetworkSoundMessage() {
				}

				protected override void Handle() {
					Reader.ReadSound().PlayGlobally();
				}

				public void Send(SoundsController.NetworkSound Sound) {
					Write(Writer => { Writer.WriteSound(Sound); });
				}
			}
		}
	}
}