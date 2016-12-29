using System;
using Microsoft.Xna.Framework.Media;

namespace Raccoon.Audio {
    public class Music {
        #region Private Static Members

        private static float _masterVolume = 1f;

        #endregion Private Static Members

        #region Private Members

        private float _volume = 1f;
        private TimeSpan _currentTime;

        #endregion Private Members

        #region Constructors

        public Music(string filename) {
            Load(filename);
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

        public string Name { get { return XNASong.Name; } }
        public TimeSpan Duration { get { return XNASong.Duration; } }
        public TimeSpan CurrentTime { get { return CurrentMusic == this ? MediaPlayer.PlayPosition : _currentTime; } private set { _currentTime = value; } }
        public bool IsLooped { get; set; }
        public bool IsPlaying { get { return !(IsPaused || IsStopped); } private set { IsPaused = IsStopped = !value; } }
        public bool IsPaused { get; private set; }
        public bool IsStopped { get; private set; } = true;

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
            if (CurrentTime.TotalSeconds > 0) {
                MediaPlayer.Play(XNASong, _currentTime);
            } else {
                MediaPlayer.Play(XNASong);
            }

            IsPlaying = true;
        }

        public void Play(TimeSpan? startPosition) {
            PrepareMediaPlayer();
            MediaPlayer.Play(XNASong, startPosition);
            IsPlaying = true;
        }

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

            CurrentTime = TimeSpan.FromSeconds(MediaPlayer.PlayPosition.TotalSeconds - 1);
            IsPaused = true;
            IsStopped = false;
        }

        public void Stop() {
            if (MediaPlayer.State != MediaState.Playing) {
                MediaPlayer.Stop();
            }

            CurrentTime = TimeSpan.FromSeconds(0);
            IsPaused = false;
            IsStopped = true;
        }

        #endregion Public Methods

        #region Protected Methods

        protected void Load(string filename) {
            XNASong = Game.Instance.Core.Content.Load<Song>(filename);
            if (XNASong == null) throw new NullReferenceException($"Music '{filename}' not found");
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
