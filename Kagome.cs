using InnoMedia;
using InnoVault;
using InnoVault.GameSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using static Kagome.Kagome;

namespace Kagome
{
	public class Kagome : Mod
	{
        public static Kagome Instance => (Kagome)ModLoader.GetMod("Kagome");
        public override void Load() => KagomeVideo.LoadData();
        public override void Unload() => KagomeVideo.UnLoadData();
    }

    internal class KagomeConfig : ModConfig
    {
        public static KagomeConfig Instance { get; private set; }

        public override ConfigScope Mode => ConfigScope.ClientSide;

        public override void OnLoaded() => Instance = this;

        [BackgroundColor(45, 175, 225, 255)]
        [DefaultValue(false)]
        public bool YuanShenQD { get; set; }
    }

    internal class KagomeVideo : MenuOverride, IUpdateAudio
    {
        private static IList<Texture2D> texture2Ds = [];
        private static int frame;
        private static int frameCount;
        private static int videoSoundID = -1;
        private static int origMusicSlotID = -1;
        private static float sengs = 0;
        private const float transitionFrame = 30f;
        public static void LoadData() {
            if (VaultUtils.isServer) {
                return;
            }

            if (KagomeConfig.Instance.YuanShenQD) {
                Main.QueueMainThreadAction(() => {
                    using Stream video = Instance.GetFileStream("YuanShenQD.mp4", true);
                    texture2Ds = MediaUtils.GetTexturesFromVideo(video);
                });

                MusicLoader.AddMusic(Instance, "YuanShenQD_Sound");
                videoSoundID = MusicLoader.GetMusicSlot(Instance, "YuanShenQD_Sound");
                if (videoSoundID == -1) {
                    throw new System.Exception("videoSoundID Bad Loaden");
                }
            }
            else {
                Main.QueueMainThreadAction(() => {
                    using Stream video = Instance.GetFileStream("Opening_1.mp4", true);
                    texture2Ds = MediaUtils.GetTexturesFromVideo(video);
                });

                MusicLoader.AddMusic(Instance, "Opening_sound");
                videoSoundID = MusicLoader.GetMusicSlot(Instance, "Opening_sound");
                if (videoSoundID == -1) {
                    throw new System.Exception("videoSoundID Bad Loaden");
                }
            }
        }

        public static void UnLoadData() {
            DisposeTexs();
            texture2Ds.Clear();
            sengs = 0;
            frame = 0;
            frameCount = 0;
            videoSoundID = -1;
        }

        private static void DisposeTexs() {
            foreach (var tex in texture2Ds) {
                if (tex == null || tex.IsDisposed) {
                    continue;
                }
                tex.Dispose();
            }
        }

        void IUpdateAudio.DecideMusic() {
            if (!Main.gameMenu || !VaultLoad.LoadenContent) {
                return;
            }

            if (frame < texture2Ds.Count) {
                if (Main.newMusic != videoSoundID) {
                    origMusicSlotID = Main.newMusic;
                }
                Main.newMusic = videoSoundID;

                for (int i = 0; i < Main.musicFade.Length; i++) {
                    if (i == videoSoundID) {
                        Main.musicFade[i] = 1f;
                        continue;
                    }
                    Main.musicFade[i] = 0;
                }
            }
            else if (origMusicSlotID != -1) {
                Main.newMusic = origMusicSlotID;

                for (int i = 0; i < Main.musicFade.Length; i++) {
                    if (i == origMusicSlotID) {
                        Main.musicFade[i] = 1f;
                        continue;
                    }
                    Main.musicFade[i] = 0;
                }
            }
        }

        public override void PostDrawMenu(GameTime gameTime) {
            if (KagomeConfig.Instance.YuanShenQD) {
                if (frame > texture2Ds.Count - 1) {
                    DisposeTexs();//放完后释放
                    return;
                }
                Main.spriteBatch.Draw(texture2Ds[frame], new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White * sengs);
            }
        }

        public override bool? DrawMenu(GameTime gameTime) {
            if (!VaultLoad.LoadenContent || frame >= texture2Ds.Count) {
                return base.DrawMenu(gameTime);
            }

            if (KagomeConfig.Instance.YuanShenQD) {
                if (frame <= transitionFrame) {
                    sengs = frame / transitionFrame;
                }
                if (++frameCount > 1) {
                    frame++;
                    frameCount = 0;

                    if (frame > texture2Ds.Count - (transitionFrame + 1)) {
                        sengs = (texture2Ds.Count - frame) / transitionFrame;
                    }
                }

                if (sengs < 1f) {
                    return true;
                }
            }
            else {
                if (++frameCount > 1) {
                    frame++;
                    frameCount = 0;
                    if (frame > texture2Ds.Count - 1) {
                        DisposeTexs();//放完后释放
                        return false;
                    }
                }
                Main.spriteBatch.Draw(texture2Ds[frame], new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
            }
            
            return false;
        }
    }
}
