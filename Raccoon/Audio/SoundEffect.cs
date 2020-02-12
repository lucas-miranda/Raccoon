using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace Raccoon.Audio {
    public class SoundEffect : IAsset {
        #region Constructors

        public SoundEffect(string filename) {
            if (string.IsNullOrEmpty(filename)) {
                throw new System.ArgumentException($"Invalid sound effect filename '{filename}'");
            }

            Filename = filename;
            if (filename.Contains(".")) {
                Filename = Path.Combine(Game.Instance.ContentDirectory, Filename);
            }

            Load();
        }

        #endregion Constructors

        #region Public Static Properties

        public static float MasterVolume { get { return Microsoft.Xna.Framework.Audio.SoundEffect.MasterVolume; } set { Microsoft.Xna.Framework.Audio.SoundEffect.MasterVolume = Util.Math.Clamp(value, 0, 1); } }

        #endregion Public Static Properties

        #region Public Properties

        public string Name { get; set; } = "SoundEffect";
        public string Filename { get; private set; }
        public System.TimeSpan Duration { get { return XNASoundEffect.Duration; } }
        public float Volume { get { return Instance.Volume; } set { Instance.Volume = value; } }
        public float Pan { get { return Instance.Pan; } set { Instance.Pan = value; } }
        public float Pitch { get { return Instance.Pitch; } set { Instance.Pitch = value; } }
        public bool IsLooped { get { return Instance.IsLooped; } set { Instance.IsLooped = value; } }
        public bool IsDisposed { get; private set; }

        #endregion Public Properties

        #region Internal Properties

        internal Microsoft.Xna.Framework.Audio.SoundEffect XNASoundEffect { get; private set; }
        internal SoundEffectInstance Instance { get; private set; }

        #endregion Internal Properties

        #region Public Methods

        public void Play() {
            Stop();
            Instance.Play();
        }

        public void Resume() {
            Instance.Resume();
        }

        public void Pause() {
            Instance.Pause();
        }

        public void Stop() {
            Instance.Stop();
        }

        public void Stop(bool immediate) {
            Instance.Stop(immediate);
        }

        public void Reload() {
            throw new System.NotImplementedException();
        }

        public void Reload(Stream assetStream) {
            throw new System.NotImplementedException();
        }

        public void Dispose() {
            if (IsDisposed) {
                return;
            }

            if (XNASoundEffect != null) {
                Stop(immediate: true);
                Instance.Dispose();
                XNASoundEffect.Dispose();
            }

            IsDisposed = true;
        }

        #endregion Public Methods

        #region Protected Methods

        protected void Load() {
            XNASoundEffect = Game.Instance.XNAGameWrapper.Content.Load<Microsoft.Xna.Framework.Audio.SoundEffect>(Filename);
            if (XNASoundEffect == null) {
                throw new System.NullReferenceException($"Sound Effect '{Filename}' not found");
            }

            Instance = XNASoundEffect.CreateInstance();
            Name = XNASoundEffect.Name;
        }

        #endregion Protected Methods
    }
}
