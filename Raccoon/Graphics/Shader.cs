using Microsoft.Xna.Framework.Graphics;
using System;

namespace Raccoon.Graphics {
    public class Shader {
        #region Constructors

        public Shader(string filename) {
            Load(filename);
        }

        #endregion Constructors

        #region Public Properties

        public int TechniqueCount { get { return XNAEffect.Techniques.Count; } }
        public string CurrentTechniqueName { get { return XNAEffect.CurrentTechnique.Name; } }

        #endregion Public Properties

        #region Internal Properties

        internal Effect XNAEffect { get; set; }

        #endregion Internal Properties

        #region Public Methods

        public void Apply() {
            foreach (EffectPass pass in XNAEffect.CurrentTechnique.Passes) {
                pass.Apply();
            }
        }

        public void Apply(int passId) {
            XNAEffect.CurrentTechnique.Passes[passId].Apply();
        }

        public void SetCurrentTechnique(int techniqueId) {
            XNAEffect.CurrentTechnique = XNAEffect.Techniques[techniqueId];
        }

        public void SetCurrentTechnique(string name) {
            XNAEffect.CurrentTechnique = XNAEffect.Techniques[name];
        }

        public bool GetParameterBoolean(string name) {
            return XNAEffect.Parameters[name].GetValueBoolean();
        }

        public void SetParameter(string name, bool value) {
            XNAEffect.Parameters[name].SetValue(value);
        }

        public float GetParameterFloat(string name) {
            return XNAEffect.Parameters[name].GetValueSingle();
        }

        public void SetParameter(string name, float value) {
            XNAEffect.Parameters[name].SetValue(value);
        }

        public float[] GetParameterFloatArray(string name) {
            return XNAEffect.Parameters[name].GetValueSingleArray();
        }

        public void SetParameter(string name, float[] value) {
            XNAEffect.Parameters[name].SetValue(value);
        }

        public int GetParameterInteger(string name) {
            return XNAEffect.Parameters[name].GetValueInt32();
        }

        public void SetParameter(string name, int value) {
            XNAEffect.Parameters[name].SetValue(value);
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
            return new Vector2(XNAEffect.Parameters[name].GetValueVector2());
        }

        public void SetParameter(string name, Vector2 value) {
            XNAEffect.Parameters[name].SetValue(value);
        }

        public Vector2[] GetParameterVector2Array(string name) {
            Microsoft.Xna.Framework.Vector2[] xnaVec2Arr = XNAEffect.Parameters[name].GetValueVector2Array();
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

            XNAEffect.Parameters[name].SetValue(vec2);
        }

        public Texture GetParameterTexture(string name) {
            return new Texture(XNAEffect.Parameters[name].GetValueTexture2D());
        }

        public void SetParameter(string name, Texture value) {
            XNAEffect.Parameters[name].SetValue(value.XNATexture);
        }

        #endregion Public Methods

        #region Protected Methods

        protected void Load(string filename) {
            if (Game.Instance.Core.GraphicsDevice == null) {
                throw new NoSuitableGraphicsDeviceException("Shader needs a valid graphics device. Maybe are you creating before Scene.Start() is called?");
            }

            XNAEffect = Game.Instance.Core.Content.Load<Effect>(filename);

            if (XNAEffect == null) throw new NullReferenceException($"Shader '{filename}' not found");
            
            /*if (XNAEffect.Techniques.Count > 0) {
                XNAEffect.CurrentTechnique = XNAEffect.Techniques[0];
            }*/
        }

        #endregion Protected Methods
    }
}
