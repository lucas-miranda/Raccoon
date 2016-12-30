using Microsoft.Xna.Framework.Graphics;
using System;

namespace Raccoon.Graphics {
    public class Shader {
        #region Private Members

        private Effect _effect;
        private string _currentTechniqueName;

        #endregion Private Members

        #region Constructors

        public Shader(string filename) {
            Load(filename);
        }

        #endregion Constructors

        #region Public Properties

        public int TechniqueCount { get { return _effect.Techniques.Count; } }

        public string CurrentTechniqueName {
            get {
                return _currentTechniqueName;
            }
            set {
                _currentTechniqueName = value;
                _effect.CurrentTechnique = _effect.Techniques[_currentTechniqueName];
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void Apply() {
            foreach (EffectPass pass in _effect.CurrentTechnique.Passes) {
                pass.Apply();
            }
        }

        public void Apply(int id) {
            _effect.CurrentTechnique.Passes[id].Apply();
        }

        public void SetCurrentTechnique(int id) {
            _effect.CurrentTechnique = _effect.Techniques[id];
        }

        public bool GetParameterBoolean(string name) {
            return _effect.Parameters[name].GetValueBoolean();
        }

        public void SetParameter(string name, bool value) {
            _effect.Parameters[name].SetValue(value);
        }

        public float GetParameterFloat(string name) {
            return _effect.Parameters[name].GetValueSingle();
        }

        public void SetParameter(string name, float value) {
            _effect.Parameters[name].SetValue(value);
        }

        public float[] GetParameterFloatArray(string name) {
            return _effect.Parameters[name].GetValueSingleArray();
        }

        public void SetParameter(string name, float[] value) {
            _effect.Parameters[name].SetValue(value);
        }

        public int GetParameterInteger(string name) {
            return _effect.Parameters[name].GetValueInt32();
        }

        public void SetParameter(string name, int value) {
            _effect.Parameters[name].SetValue(value);
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
            return new Vector2(_effect.Parameters[name].GetValueVector2());
        }

        public void SetParameter(string name, Vector2 value) {
            _effect.Parameters[name].SetValue(value);
        }

        public Vector2[] GetParameterVector2Array(string name) {
            Microsoft.Xna.Framework.Vector2[] xnaVec2Arr = _effect.Parameters[name].GetValueVector2Array();
            Vector2[] vec2Arr = new Vector2[xnaVec2Arr.Length];
            for (int i = 0; i < xnaVec2Arr.Length; i++) {
                vec2Arr[i] = new Vector2(xnaVec2Arr[i]);
            }

            return vec2Arr;
        }

        public void SetParameter(string name, Vector2[] value) {
            Microsoft.Xna.Framework.Vector2[] vec2 = new Microsoft.Xna.Framework.Vector2[value.Length];
            for (int i = 0; i < value.Length; i++) {
                vec2[i] = value[i];
            }

            _effect.Parameters[name].SetValue(vec2);
        }

        public Texture GetParameterTexture(string name) {
            return new Texture(_effect.Parameters[name].GetValueTexture2D());
        }

        public void SetParameter(string name, Texture value) {
            _effect.Parameters[name].SetValue(value.XNATexture);
        }

        #endregion Public Methods

        #region Protected Methods

        protected void Load(string filename) {
            if (Game.Instance.Core.GraphicsDevice == null) {
                throw new NoSuitableGraphicsDeviceException("Shader needs a valid graphics device. Maybe are you creating before Scene.Start() is called?");
            }

            _effect = Game.Instance.Core.Content.Load<Effect>(filename);
            if (_effect == null) throw new NullReferenceException($"Shader '{filename}' not found");
            if (_effect.Techniques.Count > 0) {
                _currentTechniqueName = _effect.Techniques[0].Name;
            }
        }

        #endregion Protected Methods
    }
}
