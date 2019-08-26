namespace Raccoon.Graphics.Transitions {
    public abstract class SceneTransition : Graphic {
        #region Public Members

        public delegate void CallbackHandler();
        public event CallbackHandler OnStart;
        public event CallbackHandler OnEnd;

        #endregion Public Members

        #region Constructors

        protected SceneTransition(Graphic endGraphic) {
            EndGraphic = endGraphic;
            ElapsedTime = 0f;
        }

        #endregion Constructors

        #region Public Properties

        public Graphic EndGraphic { get; protected set; }
        public int RepeatTimes { get; set; }
        public float Time { get; set; }
        public float ElapsedTime { get; set; }
        public bool Playing { get; private set; }
        public bool Finished { get; private set; }
        public bool Reverse { get; set; }

        #endregion Public Properties

        #region Public Methods

        public override void Update(int delta) {
            if (!Playing) {
                return;
            }

            float deltaSec = delta * Util.Time.MiliToSec;
            ElapsedTime += deltaSec;
            if (Reverse) {
                Time -= deltaSec;
                if (Time <= 0f) {
                    Time = 0f;
                    Finished = true;
                }
            } else {
                Time += deltaSec;
                if (Time >= 1f) {
                    Time = 1f;
                    Finished = true;
                }
            }

            if (Finished) {
                if (RepeatTimes > 0) {
                    Finished = false;
                    Reverse = !Reverse;
                    Time = Reverse ? 1f : 0f;
                    RepeatTimes--;
                } else {
                    Playing = false;
                    OnEnd?.Invoke();
                }
            }
        }

        public virtual void Play(int repeatTimes = 0) {
            Playing = true;
            Finished = false;
            Time = Reverse ? 1f : 0f;
            ElapsedTime = 0f;
            RepeatTimes = repeatTimes;
            OnStart?.Invoke();
        }

        public override void Dispose() {
            if (IsDisposed) {
                return;
            }

            if (EndGraphic != null) {
                EndGraphic.Dispose();
            }

            base.Dispose();
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, Vector2 origin, float layerDepth) {
            EndGraphic.Render(position, rotation, scale, flip, color, scroll, shader, ConvertLayerDepthToLayer(layerDepth));
        }

        #endregion Protected Methods
    }
}
