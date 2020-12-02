
namespace Raccoon.Graphics.ScreenEffects {
    public abstract class ScreenFadeEffect : ScreenEffect {
        #region Constructors

        public ScreenFadeEffect(FadeTarget fade, uint cycleDuration, int repeatTimes = 0, bool pingpong = false) : base(cycleDuration, repeatTimes, pingpong) {
            Fade = fade;
        }

        #endregion Constructors

        #region Public Properties

        public FadeTarget Fade { get; private set; }

        #endregion Public Properties

        /*
        public void FadeIn() {
            Fade = ScreenFadeTarget.In;
            Play();
        }

        public void FadeOut() {
            Fade = ScreenFadeTarget.Out;
            Play();
        }
        */

        /*
        protected abstract void BeginFadeIn();
        protected abstract void EndFadeIn();
        protected abstract void BeginFadeOut();
        protected abstract void EndFadeOut();
        */
    }
}
