using System;
using Microsoft.Xna.Framework.Audio;

namespace Raccoon.Audio {
    public class SoundEffect {
        #region Constructors

        public SoundEffect(string filename) {
            Load(filename);
        }

        #endregion Constructors

        #region Public Static Properties

        public static float MasterVolume { get { return Microsoft.Xna.Framework.Audio.SoundEffect.MasterVolume; } set { Microsoft.Xna.Framework.Audio.SoundEffect.MasterVolume = Util.Math.Clamp(value, 0, 1); } }

        #endregion Public Static Properties

        #region Public Properties

        public string Name { get { return XNASoundEffect.Name; } }
        public TimeSpan Duration { get { return XNASoundEffect.Duration; } }
        public float Volume { get { return Instance.Volume; } set { Instance.Volume = value; } }
        public float Pan { get { return Instance.Pan; } set { Instance.Pan = value; } }
        public float Pitch { get { return Instance.Pitch; } set { Instance.Pitch = value; } }
        public bool IsLooped { get { return Instance.IsLooped; } set { Instance.IsLooped = value; } }

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

        #endregion Public Methods

        #region Protected Methods

        protected void Load(string filename) {
            XNASoundEffect = Game.Instance.Core.Content.Load<Microsoft.Xna.Framework.Audio.SoundEffect>(filename);
            if (XNASoundEffect == null) {
                throw new NullReferenceException($"Sound Effect '{filename}' not found");
            }

            Instance = XNASoundEffect.CreateInstance();
        }

        #endregion Protected Methods
    }
}
