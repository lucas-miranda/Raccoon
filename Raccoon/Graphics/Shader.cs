using System.IO;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

namespace Raccoon.Graphics {
    public class Shader : IAsset, IEnumerable {
        #region Private Members

        private static int NextId = 0;
        private static readonly Dictionary<string, int> ShaderIds = new Dictionary<string, int>();

        #endregion Private Members

        #region Constructors

        public Shader(string filename) {
            if (string.IsNullOrEmpty(filename)) {
                throw new System.ArgumentException($"Invalid shader filename '{filename}'");
            }

            Filename = filename;
            if (filename.Contains(".") && !Filename.Contains(Game.Instance.ContentDirectory)) {
                Filename = Path.Combine(Game.Instance.ContentDirectory, Filename);
            }

            Load();
            UpdateId();
        }

        public Shader(byte[] shaderCode) {
            XNAEffect = new Effect(Game.Instance.GraphicsDevice, shaderCode);
            UpdateId();
        }

        public Shader(Stream shaderStream) {
            Load(shaderStream);
            UpdateId();
        }

        internal Shader(Effect effect) {
            XNAEffect = effect;
            UpdateId();
        }

        #endregion Constructors

        #region Public Properties

        public string Name { get; set; } = "Shader";
        public string[] Filenames { get; private set; }
        public int TechniqueCount { get { return XNAEffect.Techniques.Count; } }
        public string CurrentTechniqueName { get { return XNAEffect.CurrentTechnique.Name; } set { XNAEffect.CurrentTechnique = XNAEffect.Techniques[value]; } }
        public bool IsDisposed { get; private set; }

        public string Filename { 
            get { 
                return Filenames?[0] ?? ""; 
            } 

            private set { 
                if (Filenames == null) {
                    Filenames = new string[1];
                }

                Filenames[0] = value; 
            } 
        }

        #endregion Public Properties

        #region Internal Properties

        internal int Id { get; private set; }
        internal protected Effect XNAEffect { get; set; }

        #endregion Internal Properties

        #region Public Methods

        public void Apply() {
            OnApply();
            foreach (EffectPass pass in XNAEffect.CurrentTechnique.Passes) {
                pass.Apply();
            }
        }

        public void Apply(int passId) {
            OnApply();
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

        public float[] GetParameterFloatArray(string name, int count) {
            return XNAEffect.Parameters[name].GetValueSingleArray(count);
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

        public Microsoft.Xna.Framework.Matrix GetParameterMatrix(string name) {
            return XNAEffect.Parameters[name].GetValueMatrix();
        }

        public void SetParameter(string name, Microsoft.Xna.Framework.Matrix value) {
            XNAEffect.Parameters[name].SetValue(value);
        }

        public Microsoft.Xna.Framework.Matrix[] GetParameterMatrixArray(string name, int count) {
            return XNAEffect.Parameters[name].GetValueMatrixArray(count);
        }

        public void SetParameter(string name, Microsoft.Xna.Framework.Matrix[] value) {
            XNAEffect.Parameters[name].SetValue(value);
        }

        public Microsoft.Xna.Framework.Quaternion GetParameterQuaternion(string name) {
            return XNAEffect.Parameters[name].GetValueQuaternion();
        }

        public void SetParameter(string name, Microsoft.Xna.Framework.Quaternion value) {
            XNAEffect.Parameters[name].SetValue(value);
        }

        public Vector2 GetParameterVector2(string name) {
            return new Vector2(XNAEffect.Parameters[name].GetValueVector2());
        }

        public void SetParameter(string name, Vector2 value) {
            XNAEffect.Parameters[name].SetValue(value);
        }

        public Vector2[] GetParameterVector2Array(string name, int count) {
            Microsoft.Xna.Framework.Vector2[] xnaVec2Arr = XNAEffect.Parameters[name].GetValueVector2Array(count);
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

        public void Reload() {
            try {
                Effect currentEffect = XNAEffect;
                Load();

                if (currentEffect != null) {
                    currentEffect.Dispose();
                }
            } catch(System.Exception e) {
                throw e;
            }
        }

        public void Reload(Stream shaderStream) {
            try {
                Effect currentEffect = XNAEffect;
                Load(shaderStream);

                if (currentEffect != null) {
                    currentEffect.Dispose();
                }
            } catch(System.Exception e) {
                throw e;
            }
        }

        public void Dispose() {
            if (IsDisposed) {
                return;
            }

            if (XNAEffect != null) {
                if (string.IsNullOrWhiteSpace(XNAEffect.Name)) {
                    ShaderIds.Remove($"Shader {Id}");
                } else {
                    ShaderIds.Remove(XNAEffect.Name);
                }

                XNAEffect.Dispose();
                XNAEffect = null;
            }

            IsDisposed = true;
        }

        /*
        public IEnumerator<EffectPass> GetEnumerator() {
            OnApply();
            foreach (EffectPass pass in XNAEffect.CurrentTechnique.Passes) {
                yield return pass;
            }
        }
        */

        public IEnumerator GetEnumerator() {
            OnApply();

            foreach (EffectPass pass in XNAEffect.CurrentTechnique.Passes) {
                BeforePassApply();
                pass.Apply();
                yield return pass;
            }
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual void OnApply() {
        }

        protected virtual void BeforePassApply() {
        }

        protected void Load() {
            if (Game.Instance.XNAGameWrapper.GraphicsDevice == null) {
                throw new NoSuitableGraphicsDeviceException("Shader needs a valid graphics device. Maybe are you creating before Scene.Start() is called?");
            }

            if (Filename.EndsWith(".fxb") || Filename.EndsWith(".fxc")) {
                XNAEffect = new Effect(Game.Instance.GraphicsDevice, File.ReadAllBytes(Filename)) {
                    Name = Path.GetFileNameWithoutExtension(Filename)
                };
            } else {
                XNAEffect = Game.Instance.XNAGameWrapper.Content.Load<Effect>(Filename);
            }

            if (XNAEffect == null) {
                throw new System.NullReferenceException($"Shader '{Filename}' not found");
            }
        }

        protected void Load(Stream shaderStream) {
            if (shaderStream is FileStream fileStream) {
                Filename = fileStream.Name;
            }

            shaderStream.Seek(0, SeekOrigin.End);
            long streamSize = shaderStream.Position;
            shaderStream.Seek(0, SeekOrigin.Begin);

            byte[] shaderCode = new byte[streamSize];

            if (streamSize > int.MaxValue) {
                byte[] buffer = new byte[int.MaxValue];
                int count;
                long offset = 0L;

                while (offset < streamSize) {
                    if (streamSize >= int.MaxValue) {
                        count = int.MaxValue;
                    } else {
                        count = (int) streamSize;
                    }

                    shaderStream.Read(buffer, 0, count);
                    buffer.CopyTo(shaderCode , offset);
                    offset += count;
                }
            } else {
                shaderStream.Read(shaderCode , 0, (int) streamSize);
            }

            XNAEffect = new Effect(Game.Instance.GraphicsDevice, shaderCode);
        }

        protected EffectParameter RequestParameter(string paramName) {
            return XNAEffect.Parameters[paramName] ?? throw new System.InvalidOperationException($"Requested parameter '{paramName}' was not found.");
        }

        protected EffectParameter RequestParameter(int id) {
            return XNAEffect.Parameters[id] ?? throw new System.InvalidOperationException($"Requested parameter with index {id} was not found.");
        }

        protected bool TryRequestParameter(string paramName, out EffectParameter parameter) {
            parameter = XNAEffect.Parameters[paramName];
            return parameter != null;
        }

        protected bool TryRequestParameter(int id, out EffectParameter parameter) {
            try {
                parameter = XNAEffect.Parameters[id];
            } catch (System.ArgumentOutOfRangeException) {
                parameter = null;
                return false;
            }

            return true;
        }

        #endregion Protected Methods

        #region Private Methods

        private void UpdateId() {
            if (string.IsNullOrWhiteSpace(XNAEffect.Name)) {
                XNAEffect.Name = $"Shader {NextId}";
            }

            if (!ShaderIds.TryGetValue(XNAEffect.Name, out int id)) {
                id = NextId;
                NextId++;
                ShaderIds.Add(XNAEffect.Name, id);
            }

            Id = id;
        }

        #endregion Private Methods
    }
}
