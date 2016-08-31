using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public class Shader {
        private Effect effect;
        private string currentTechnique;
        
        public Shader(string filename) {
            Filename = filename;
            if (Game.Instance.IsRunning) {
                Load();
            } else {
                Game.Instance.Core.OnLoadContent += new Core.GeneralHandler(Load);
            }
        }

        public string Filename { get; private set; }
        public int TechniqueCount { get { return effect.Techniques.Count; } }

        public string CurrentTechnique {
            get {
                return currentTechnique;
            }
            set {
                currentTechnique = value;
                if (Game.Instance.IsRunning) {
                    effect.CurrentTechnique = effect.Techniques[currentTechnique];
                }
            }
        }

        public void Apply(int id = -1) {
            EffectPassCollection passes = effect.CurrentTechnique.Passes;
            if (id < 0) {
                for (id = 0; id < passes.Count; id++) {
                    passes[id].Apply();
                }
            } else {
                passes[id].Apply();
            }
        }

        public void SetCurrentTechniqueId(int id) {
            effect.CurrentTechnique = effect.Techniques[id];
        }

        public bool GetParameterBoolean(string name) {
            return effect.Parameters[name].GetValueBoolean();
        }

        public void SetParameter(string name, bool value) {
            effect.Parameters[name].SetValue(value);
        }

        public float GetParameterSingle(string name) {
            return effect.Parameters[name].GetValueSingle();
        }

        public void SetParameter(string name, float value) {
            effect.Parameters[name].SetValue(value);
        }

        public float[] GetParameterSingleArray(string name) {
            return effect.Parameters[name].GetValueSingleArray();
        }

        public void SetParameter(string name, float[] value) {
            effect.Parameters[name].SetValue(value);
        }

        public int GetParameterInteger(string name) {
            return effect.Parameters[name].GetValueInt32();
        }

        public void SetParameter(string name, int value) {
            effect.Parameters[name].SetValue(value);
        }

        /*public void SetParameter(string name, Microsoft.Xna.Framework.Matrix value) {
            effect.Parameters[name].SetValue(value);
        }

        public void SetParameter(string name, Microsoft.Xna.Framework.Matrix[] value) {
            effect.Parameters[name].SetValue(value);
        }

        public void SetParameter(string name, Microsoft.Xna.Framework.Quaternion value) {
            effect.Parameters[name].SetValue(value);
        }*/

        public Vector2 GetParameterVector2(string name) {
            Microsoft.Xna.Framework.Vector2 vec2 = effect.Parameters[name].GetValueVector2();
            return new Vector2(vec2.X, vec2.Y);
        }

        public void SetParameter(string name, Vector2 value) {
            effect.Parameters[name].SetValue(value);
        }

        public Vector2[] GetParameterVector2Array(string name) {
            Microsoft.Xna.Framework.Vector2[] vec2 = effect.Parameters[name].GetValueVector2Array();
            Vector2[] v = new Vector2[vec2.Length];
            for (int i = 0; i < vec2.Length; i++) {
                v[i] = new Vector2(vec2[i].X, vec2[i].Y);
            }

            return v;
        }

        public void SetParameter(string name, Vector2[] value) {
            Microsoft.Xna.Framework.Vector2[] vec2 = new Microsoft.Xna.Framework.Vector2[value.Length];
            for (int i = 0; i < value.Length; i++) {
                vec2[i] = value[i];
            }

            effect.Parameters[name].SetValue(vec2);
        }

        /*public Texture GetParameterTexture(string name) {
            Microsoft.Xna.Framework.Vector2 tex = effect.Parameters[name].GetValueTexture2D();
            return (Texture) tex;
        }*/

        public void SetParameter(string name, Image value) {
            effect.Parameters[name].SetValue(value.Texture);
        }

        internal void Load() {
            effect = Game.Instance.Core.Content.Load<Effect>(Filename);
            if (currentTechnique.Length > 0) {
                effect.CurrentTechnique = effect.Techniques[currentTechnique];
            } else {
                currentTechnique = effect.Techniques[0].Name;
            }
        }
    }
}
