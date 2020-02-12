using System.IO;
using Microsoft.Xna.Framework.Media;

namespace Raccoon.Audio {
    public class Music : IAsset {
        #region Private Static Members

        private static float _masterVolume = 1f;

        #endregion Private Static Members

        #region Private Members

        private float _volume = 1f;
        private System.TimeSpan _currentTime;

        #endregion Private Members

        #region Constructors

        public Music(string filename) {
            if (string.IsNullOrEmpty(filename)) {
                throw new System.ArgumentException($"Invalid music filename '{filename}'");
            }

            Filename = filename;
            if (filename.Contains(".") && !Filename.Contains(Game.Instance.ContentDirectory)) {
                Filename = Path.Combine(Game.Instance.ContentDirectory, Filename);
            }

            Load();
        }

        #endregion Constructors

        #region Public Static Properties

        public static bool IsMuted { get { return MediaPlayer.IsMuted; } set { MediaPlayer.IsMuted = value; } }

        public static float MasterVolume {
            get {
                return _masterVolume;
            }

            set {
                _masterVolume = value;
                if (CurrentMusic != null) {
                    CurrentMusic.Volume = CurrentMusic.Volume * _masterVolume;
                }
            }
        }

        #endregion Public Static Properties

        #region Public Properties

        public string Name { get; set; } = "Music";
        public string Filename { get; private set; }
        public System.TimeSpan Duration { get { return XNASong.Duration; } }
        public System.TimeSpan CurrentTime { get { return CurrentMusic == this ? MediaPlayer.PlayPosition : _currentTime; } private set { _currentTime = value; } }
        public bool IsLooped { get; set; }
        public bool IsPlaying { get { return !(IsPaused || IsStopped); } private set { IsPaused = IsStopped = !value; } }
        public bool IsPaused { get; private set; }
        public bool IsStopped { get; private set; } = true;
        public bool IsDisposed { get; private set; }

        public float Volume {
            get {
                return _volume;
            }

            set {
                _volume = value;
                if (CurrentMusic == this) {
                    MediaPlayer.Volume = _volume * MasterVolume;
                }
            }
        }

        #endregion Public Properties

        #region Internal Static Properties

        internal static Music CurrentMusic { get; set; }

        #endregion Internal Static Properties

        #region Internal Properties

        internal Song XNASong { get; private set; }

        #endregion Internal Properties

        #region Public Static Methods

        public static void ToggleMute() {
            IsMuted = !IsMuted;
        }

        #endregion Public Static Methods

        #region Public Methods

        public void Play() {
            PrepareMediaPlayer();

            CurrentTime = System.TimeSpan.FromSeconds(0);
            MediaPlayer.Play(XNASong);

            IsPlaying = true;
        }

        /*
        public void Play(TimeSpan? startPosition) {
            PrepareMediaPlayer();
            //! MediaPlayer.Play(XNASong, startPosition);
            MediaPlayer.Play(XNASong);
            IsPlaying = true;
        }
        */

        public void Resume() {
            if (CurrentMusic == this) {
                MediaPlayer.Resume();
                IsPlaying = true;
            } else {
                Play();
            }
        }

        public void Pause() {
            if (MediaPlayer.State != MediaState.Paused) {
                MediaPlayer.Stop();
            }

            CurrentTime = System.TimeSpan.FromSeconds(MediaPlayer.PlayPosition.TotalSeconds - 1);
            IsPaused = true;
            IsStopped = false;
        }

        public void Stop() {
            if (MediaPlayer.State != MediaState.Playing) {
                MediaPlayer.Stop();
            }

            CurrentTime = System.TimeSpan.FromSeconds(0);
            IsPaused = false;
            IsStopped = true;
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

            if (IsPlaying) {
                Stop();
            }

            if (XNASong != null) {
                XNASong.Dispose();
                XNASong = null;
            }

            IsDisposed = true;
        }

        #endregion Public Methods

        #region Protected Methods

        protected void Load() {
            XNASong = Game.Instance.XNAGameWrapper.Content.Load<Song>(Filename);

            if (XNASong == null) {
                throw new System.NullReferenceException($"Music '{Filename}' not found");
            }

            Name = XNASong.Name;
        }

        #endregion Protected Methods

        #region Internal Methods

        internal void PrepareMediaPlayer() {
            if (CurrentMusic == null) {
                CurrentMusic = this;
            } else if (CurrentMusic != this) {
                CurrentMusic.Pause();
                CurrentMusic = this;
            }

            MediaPlayer.IsRepeating = IsLooped;
            MediaPlayer.Volume = Volume * MasterVolume;
        }

        #endregion Internal Methods
    }
}
