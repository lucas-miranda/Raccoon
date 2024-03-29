﻿using System.Collections.Generic;

using Raccoon.Graphics;
using Raccoon.Components;
using Raccoon.Util.Collections;

namespace Raccoon {
    public class Entity : ISceneObject {
        #region Public Delegates

        public event SceneObjectEvent OnSceneAdded          = delegate { },
                                      OnSceneRemoved        = delegate { },
                                      OnStart               = delegate { },
                                      OnSceneBegin          = delegate { },
                                      OnSceneEnd            = delegate { },
                                      OnBeforeUpdate        = delegate { },
                                      OnUpdate              = delegate { },
                                      OnLateUpdate          = delegate { },
                                      OnRender              = delegate { };

#if DEBUG
        public event SceneObjectEvent OnDebugRender     = delegate { };
#endif

        #endregion Public Delegates

        #region Private Members

        private Renderer _renderer;
        private int _layer;
        private ControlGroup _controlGroup;

        #endregion Private Members

        #region Constructors

        public Entity() {
            Name = GetType().Name;
            _renderer = Game.Instance.MainRenderer;
            Transform = new Transform(this);
        }

        #endregion Constructors

        #region Public Properties

        public string Name { get; set; }
        public Transform Transform { get; private set; }
        public bool Active { get; set; } = true;
        public bool Visible { get; set; } = true;
        public bool Enabled { get { return Active || Visible; } set { Active = Visible = value; } }
        public bool AutoUpdate { get; set; } = true;
        public bool AutoRender { get; set; } = true;
        public bool ShouldUseTransformParentRenderer { get; set; } = true;
        public bool IgnoreDebugRender { get; set; }
        public bool HasStarted { get; private set; }
        public bool WipeOnRemoved { get; set; } = true;
        public bool IsWiped { get; private set; }
        public int Order { get; set; }
        public uint Timer { get; private set; }
        public Locker<Graphic> Graphics { get; private set; } = new Locker<Graphic>(Graphic.LayerComparer);
        public Locker<Component> Components { get; private set; } = new Locker<Component>();
        public Scene Scene { get; private set; }
        public bool IsSceneFromTransformAncestor { get; internal set; }

        public Graphic Graphic {
            get {
                if (Graphics == null || Graphics.Count == 0) {
                    return null;
                }

                return Graphics[0];
            }

            set {
                if (Graphics.Count == 0) {
                    if (value == null) {
                        return;
                    }

                    AddGraphic(value);
                    return;
                } else if (value == Graphics[0]) {
                    return;
                }

                RemoveGraphic(Graphics[0]);

                if (value != null && GraphicAdded(value)) {
                    Graphics.Insert(0, value);
                    value.Renderer = Renderer;

                    Components.Lock();
                    foreach (Component c in Components) {
                        c.GraphicAdded(value);
                    }
                    Components.Unlock();
                }
            }
        }

        public Renderer Renderer {
            get {
                return _renderer;
            }

            set {
                bool changed = value != _renderer;
                _renderer = value;

                foreach (Graphic g in Graphics) {
                    g.Renderer = _renderer;
                }

                if (!(Transform == null || Transform.IsDetached)) {
                    Transform.LockChildren();
                    foreach (Transform child in Transform) {
                        if (!child.Entity.ShouldUseTransformParentRenderer) {
                            continue;
                        }

                        child.Entity.Renderer = _renderer;
                    }
                    Transform.UnlockChildren();
                }

                if (changed) {
                    RendererChanged(_renderer);
                }
            }
        }

        public int Layer {
            get {
                return Transform.Parent != null ? Transform.Parent.Entity.Layer + _layer : _layer;
            }

            set {
                _layer = value;
            }
        }

        public int LocalLayer { get { return _layer; } }

        public ControlGroup ControlGroup {
            get {
                if (_controlGroup != null) {
                    return _controlGroup;
                }

                return Transform.Parent?.Entity.ControlGroup;
            }

            private set {
                _controlGroup = value;
            }
        }

        #endregion Public Properties

        #region Public Methods

        public static int LayerComparer(Entity a, Entity b) {
            return a.Layer.CompareTo(b.Layer);
        }

        public virtual void SceneAdded(Scene scene) {
            if (Scene != null) {
                return;
            }

            Scene = scene;
            Components.Lock();
            foreach (Component c in Components) {
                c.OnSceneAdded();
            }
            Components.Unlock();

            Transform.EntitySceneAdded(Scene);
            OnSceneAdded();
        }

        public virtual void SceneRemoved(bool allowWipe = true) {
            if (IsWiped) {
                // avoid more than one wipe call
                return;
            }

            if (ControlGroup != null) {
                ControlGroup.Unregister(this);
            }

            Scene = null;

            if (WipeOnRemoved && allowWipe) {
                IsWiped = true;
                Enabled = false;
                OnSceneAdded = null;
                OnStart = null;
                OnSceneBegin = null;
                OnSceneEnd = null;
                OnBeforeUpdate = null;
                OnUpdate = null;
                OnLateUpdate = null;
                OnRender = null;
#if DEBUG
                OnDebugRender = null;
#endif

                _renderer = null;

                Components.Lock();
                foreach (Component c in Components) {
                    if (c.Entity == null) {
                        continue;
                    }

                    ComponentRemoved(c);
                    c.Enabled = false;
                    c.OnSceneRemoved(wipe: true);
                    c.OnRemoved();
                }
                Components.Unlock();
                Components.Clear();
            } else {
                Components.Lock();
                foreach (Component c in Components) {
                    if (c.Entity == null) {
                        continue;
                    }

                    c.OnSceneRemoved(WipeOnRemoved && allowWipe);
                }
                Components.Unlock();
            }

            Transform.EntitySceneRemoved(WipeOnRemoved && allowWipe);
            OnSceneRemoved?.Invoke();

            if (WipeOnRemoved && allowWipe) {
                OnSceneRemoved = null;
                Transform.Detach();

                Graphics.Lock();
                foreach (Graphic g in Graphics) {
                    GraphicRemoved(g);
                    g.Dispose();
                }
                Graphics.Unlock();

                Graphics.Clear();
            }
        }

        public virtual void Start() {
            if (HasStarted) {
                return;
            }

            HasStarted = true;

            Transform.LockChildren();
            foreach (Transform child in Transform) {
                if (child.Entity.HasStarted) {
                    continue;
                }

                child.Entity.Start();
            }
            Transform.UnlockChildren();

            OnStart();
            OnStart = null;
        }

        public virtual void SceneBegin() {
            Transform.LockChildren();
            foreach (Transform child in Transform) {
                if (!IsChildAvailable(child)) {
                    continue;
                }

                child.Entity.SceneBegin();
            }
            Transform.UnlockChildren();

            OnSceneBegin();
        }

        public virtual void SceneEnd() {
            Transform.LockChildren();
            foreach (Transform child in Transform) {
                if (!IsChildAvailable(child)) {
                    continue;
                }

                child.Entity.SceneEnd();
            }
            Transform.UnlockChildren();

            OnSceneEnd();
        }

        public virtual void BeforeUpdate() {
            Components.Lock();
            foreach (Component c in Components) {
                if (!c.Active) {
                    continue;
                }

                c.BeforeUpdate();
            }
            Components.Unlock();

            if (Transform != null) {
                Transform.LockChildren();
                foreach (Transform child in Transform) {
                    if (!CanUpdateChild(child)) {
                        continue;
                    }

                    child.Entity.BeforeUpdate();
                }
                Transform.UnlockChildren();
            }

            OnBeforeUpdate?.Invoke();
        }

        public virtual void Update(int delta) {
            Timer += (uint) delta;

            Graphics.Lock();
            foreach (Graphic g in Graphics) {
                if (!g.Visible
                 || !g.Active
                 || (g.ControlGroup != null && g.ControlGroup.IsPaused)
                ) {
                    continue;
                }

                g.Update(delta);
            }
            Graphics.Unlock();

            Components.Lock();
            foreach (Component c in Components) {
                if (!c.Active) {
                    continue;
                }

                c.Update(delta);
            }
            Components.Unlock();

            if (Transform != null) {
                Transform.LockChildren();
                foreach (Transform child in Transform) {
                    if (!CanUpdateChild(child)) {
                        continue;
                    }

                    child.Entity.Update(delta);
                }
                Transform.UnlockChildren();
            }

            OnUpdate?.Invoke();
        }

        public virtual void LateUpdate() {
            Components.Lock();
            foreach (Component c in Components) {
                if (!c.Active) {
                    continue;
                }

                c.LateUpdate();
            }
            Components.Unlock();

            if (Transform != null) {
                Transform.LockChildren();
                foreach (Transform child in Transform) {
                    if (!CanUpdateChild(child)) {
                        continue;
                    }

                    child.Entity.LateUpdate();
                }
                Transform.UnlockChildren();
            }

            OnLateUpdate?.Invoke();
        }

        public virtual void BeforePhysicsStep() {
            Components.Lock();
            foreach (Component c in Components) {
                if (!c.Active) {
                    continue;
                }

                c.BeforePhysicsStep();
            }
            Components.Unlock();

            Transform.LockChildren();
            foreach (Transform child in Transform) {
                if (!CanUpdateChild(child)) {
                    continue;
                }

                child.Entity.BeforePhysicsStep();
            }
            Transform.UnlockChildren();
        }

        public virtual void PhysicsStep(float stepDelta) {
            Components.Lock();
            foreach (Component c in Components) {
                if (!c.Active) {
                    continue;
                }

                c.PhysicsStep(stepDelta);
            }
            Components.Unlock();

            Transform.LockChildren();
            foreach (Transform child in Transform) {
                if (!CanUpdateChild(child)) {
                    continue;
                }

                child.Entity.PhysicsStep(stepDelta);
            }
            Transform.UnlockChildren();
        }

        public virtual void LatePhysicsStep() {
            Components.Lock();
            foreach (Component c in Components) {
                if (!c.Active) {
                    continue;
                }

                c.LatePhysicsStep();
            }
            Components.Unlock();

            Transform.LockChildren();
            foreach (Transform child in Transform) {
                if (!CanUpdateChild(child)) {
                    continue;
                }

                child.Entity.LatePhysicsStep();
            }
            Transform.UnlockChildren();
        }

        public virtual void Render() {
            Graphics.Lock();

            float r = Transform.Rotation;
            Vector2 p = Transform.Position,
                    s = Transform.Scale;

            foreach (Graphic g in Graphics) {
                if (!g.Visible) {
                    continue;
                }

                g.Render(
                    position:   p + g.Position * s,
                    rotation:   r + g.Rotation,
                    scale:      s * g.Scale,
                    flip:       g.Flipped,
                    color:      g.Color,
                    scroll:     g.Scroll,
                    shader:     g.Shader,
                    layer:      Layer + g.Layer
                );
            }
            Graphics.Unlock();

            Components.Lock();
            foreach (Component c in Components) {
                if (!c.Visible) {
                    continue;
                }

                c.Render();
            }
            Components.Unlock();

            Transform.LockChildren();
            foreach (Transform child in Transform) {
                if (!CanRenderChild(child)) {
                    continue;
                }

                child.Entity.Render();
            }
            Transform.UnlockChildren();

            OnRender();
        }

#if DEBUG
        public virtual void DebugRender() {
            Graphics.Lock();
            foreach (Graphic g in Graphics) {
                if (!g.Visible || g.IgnoreDebugRender) {
                    continue;
                }

                g.DebugRender();
            }
            Graphics.Unlock();

            Components.Lock();
            foreach (Component c in Components) {
                if (!c.Enabled || c.IgnoreDebugRender) {
                    continue;
                }

                c.DebugRender();
            }
            Components.Unlock();

            Transform.LockChildren();
            foreach (Transform child in Transform) {
                if (!CanDebugRenderChild(child)) {
                    continue;
                }

                child.Entity.DebugRender();
            }
            Transform.UnlockChildren();

            OnDebugRender();
        }
#endif

        public virtual void Paused() {
            Graphics.Lock();
            foreach (Graphic g in Graphics) {
                if (g.ControlGroup != null) {
                    continue;
                }

                g.Paused();
            }
            Graphics.Unlock();

            Components.Lock();
            foreach (Component c in Components) {
                c.Paused();
            }
            Components.Unlock();
        }

        public virtual void Resumed() {
            Graphics.Lock();
            foreach (Graphic g in Graphics) {
                if (g.ControlGroup != null) {
                    continue;
                }

                g.Resumed();
            }
            Graphics.Unlock();

            Components.Lock();
            foreach (Component c in Components) {
                c.Resumed();
            }
            Components.Unlock();
        }

        public virtual void ControlGroupRegistered(ControlGroup controlGroup) {
            ControlGroup = controlGroup;

            Graphics.Lock();
            foreach (Graphic g in Graphics) {
                if (g.ControlGroup != null) {
                    continue;
                }

                g.ControlGroupRegistered(controlGroup);
            }
            Graphics.Unlock();

            Components.Lock();
            foreach (Component c in Components) {
                c.ControlGroupRegistered(ControlGroup);
            }
            Components.Unlock();
        }

        public virtual void ControlGroupUnregistered() {
            Graphics.Lock();
            foreach (Graphic g in Graphics) {
                if (g.ControlGroup == null) {
                    continue;
                }

                g.ControlGroupUnregistered();
            }
            Graphics.Unlock();

            Components.Lock();
            foreach (Component c in Components) {
                c.ControlGroupUnregistered();
            }
            Components.Unlock();

            ControlGroup = null;
        }

        public Graphic AddGraphic(Graphic graphic) {
            if (graphic == null) {
                throw new System.ArgumentNullException(nameof(graphic));
            }

            if (!GraphicAdded(graphic)) {
                return null;
            }

            Graphics.Add(graphic);
            graphic.Renderer = Renderer;

            Components.Lock();
            foreach (Component c in Components) {
                c.GraphicAdded(graphic);
            }
            Components.Unlock();

            return graphic;
        }

        public T AddGraphic<T>(T graphic) where T : Graphic {
            return AddGraphic(graphic as Graphic) as T;
        }

        public void AddGraphics(IEnumerable<Graphic> graphics) {
            foreach (Graphic g in graphics) {
                AddGraphic(g);
            }
        }

        public void AddGraphics(params Graphic[] graphics) {
            AddGraphics((IEnumerable<Graphic>) graphics);
        }

        public Component AddComponent(Component component) {
            if (component == null) {
                throw new System.ArgumentNullException(nameof(component));
            }

            // validate component usage
            System.Type componentType = component.GetType(),
                        entityType = GetType();

            foreach (object attrObj in component.GetType().GetCustomAttributes(typeof(ComponentUsageAttribute), inherit: true)) {
                ComponentUsageAttribute usageAttribute = (ComponentUsageAttribute) attrObj;

                if (!usageAttribute.IsEntityTypeAllowed(entityType)) {
                    throw new System.InvalidOperationException($"Entity with type '{entityType}' isn't allowed to add Component '{component.GetType()}'.");
                }

                if (usageAttribute.Unique) {
                    foreach (Component c in Components) {
                        if (c.GetType().Equals(componentType)) {
                            throw new System.InvalidOperationException($"Can't add another Component with type '{componentType}', it should be unique.");
                        }
                    }
                }
            }

            if (!ComponentAdded(component)) {
                return null;
            }

            Components.Add(component);
            component.OnAdded(this);

            return component;
        }

        public T AddComponent<T>(T component) where T : Component {
            return (T) AddComponent((Component) component);
        }

        public T AddComponent<T>() where T : Component, new() {
            return (T) AddComponent((Component) new T());
        }

        public bool RemoveGraphic(Graphic graphic) {
            if (graphic == null) {
                return false;
            }

            if (Graphics.Remove(graphic)) {
                GraphicRemoved(graphic);

                Components.Lock();
                foreach (Component c in Components) {
                    c.GraphicRemoved(graphic);
                }
                Components.Unlock();

                return true;
            }

            return false;
        }

        public void RemoveGraphics(IEnumerable<Graphic> graphics) {
            foreach (Graphic g in graphics) {
                RemoveGraphic(g);
            }
        }

        public void RemoveGraphics(params Graphic[] graphics) {
            RemoveGraphics((IEnumerable<Graphic>) graphics);
        }

        public bool RemoveComponent(Component component, bool wipe = true) {
            if (Components == null || !Components.Remove(component)) {
                return false;
            }

            ComponentRemoved(component);
            component.Enabled = false;
            component.OnSceneRemoved(wipe);
            component.OnRemoved();
            return true;
        }

        public T RemoveComponent<T>(bool wipe = true) where T : Component {
            if (Components == null) {
                return null;
            }

            T retComponent = null;

            Components.Lock();
            foreach (Component component in Components) {
                if (component is T ct) {
                    Components.Remove(component);
                    ComponentRemoved(component);
                    component.Enabled = false;
                    component.OnSceneRemoved(wipe);
                    component.OnRemoved();
                    retComponent = ct;
                    break;
                }
            }
            Components.Unlock();

            return retComponent;
        }

        public T GetComponent<T>() where T : Component {
            if (Components == null) {
                return null;
            }

            foreach (Component c in Components) {
                if (c is T ct) {
                    return ct;
                }
            }

            return null;
        }

        public T GetAddComponent<T>() where T : Component, new() {
            if (TryGetComponent<T>(out T component)) {
                return component;
            }

            return AddComponent<T>();
        }

        public List<T> GetComponents<T>() where T : Component {
            List<T> components = new List<T>();
            foreach (Component c in Components) {
                if (c is T ct) {
                    components.Add(ct);
                }
            }

            return components;
        }

        public IEnumerable<T> IterateComponents<T>() where T : Component {
            foreach (Component c in Components) {
                if (c is T ct) {
                    yield return ct;
                }
            }
        }

        public bool TryGetComponent<T>(out T component) where T : Component {
            if (Components == null) {
                component = null;
                return false;
            }

            foreach (Component c in Components) {
                if (c is T ct) {
                    component = ct;
                    return true;
                }
            }

            component = null;
            return false;
        }

        public bool TryGetComponents<T>(out List<T> components) where T : Component {
            components = new List<T>();
            foreach (Component c in Components) {
                if (c is T ct) {
                    components.Add(ct);
                }
            }

            return components.Count > 0;
        }

        public bool HasComponent<T>() where T : Component {
            if (Components == null) {
                return false;
            }

            foreach (Component c in Components) {
                if (c is T) {
                    return true;
                }
            }

            return false;
        }

        public void RemoveComponents<T>() where T : Component {
            Components.Lock();
            foreach (Component c in Components) {
                if (c is T && Components.Remove(c)) {
                    ComponentRemoved(c);
                    c.Enabled = false;
                    c.OnSceneRemoved(wipe: true);
                    c.OnRemoved();
                }
            }
            Components.Unlock();
        }

        public void RemoveComponents<T, K>()
         where T : Component
         where K : Component {
            Components.Lock();
            foreach (Component c in Components) {
                if ((c is T || c is K) && Components.Remove(c)) {
                    ComponentRemoved(c);
                    c.Enabled = false;
                    c.OnSceneRemoved(wipe: true);
                    c.OnRemoved();
                }
            }
            Components.Unlock();
        }

        public void RemoveComponents<T, K, V>()
         where T : Component
         where K : Component
         where V : Component {
            Components.Lock();
            foreach (Component c in Components) {
                if ((c is T || c is K || c is V) && Components.Remove(c)) {
                    ComponentRemoved(c);
                    c.Enabled = false;
                    c.OnSceneRemoved(wipe: true);
                    c.OnRemoved();
                }
            }
            Components.Unlock();
        }

        public void RemoveComponents<T, K, V, U>()
         where T : Component
         where K : Component
         where V : Component
         where U : Component {
            Components.Lock();
            foreach (Component c in Components) {
                if ((c is T || c is K || c is V || c is U) && Components.Remove(c)) {
                    ComponentRemoved(c);
                    c.Enabled = false;
                    c.OnSceneRemoved(wipe: true);
                    c.OnRemoved();
                }
            }
            Components.Unlock();

        }

        public void RemoveComponents(System.Type componentType) {
            Components.Lock();
            foreach (Component c in Components) {
                if (c.GetType().Equals(componentType) && Components.Remove(c)) {
                    ComponentRemoved(c);
                    c.Enabled = false;
                    c.OnSceneRemoved(wipe: true);
                    c.OnRemoved();
                }
            }
            Components.Unlock();
        }

        public void RemoveComponents(System.Type componentAType, System.Type componentBType) {
            Components.Lock();
            foreach (Component c in Components) {
                if ((c.GetType().Equals(componentAType) || c.GetType().Equals(componentBType))
                 && Components.Remove(c)) {
                    ComponentRemoved(c);
                    c.Enabled = false;
                    c.OnSceneRemoved(wipe: true);
                    c.OnRemoved();
                }
            }
            Components.Unlock();
        }

        public void RemoveComponents(System.Type componentAType, System.Type componentBType, System.Type componentCType) {
            Components.Lock();
            foreach (Component c in Components) {
                if ((c.GetType().Equals(componentAType) || c.GetType().Equals(componentBType) || c.GetType().Equals(componentCType))
                 && Components.Remove(c)) {
                    ComponentRemoved(c);
                    c.Enabled = false;
                    c.OnSceneRemoved(wipe: true);
                    c.OnRemoved();
                }
            }
            Components.Unlock();
        }

        public void RemoveComponents(params System.Type[] componentTypes) {
            Components.Lock();
            foreach (Component c in Components) {
                bool canRemove = false;
                System.Type type = c.GetType();

                foreach (System.Type componentType in componentTypes) {
                    if (type.Equals(componentType)) {
                        canRemove = true;
                        break;
                    }
                }

                if (canRemove && Components.Remove(c)) {
                    ComponentRemoved(c);
                    c.Enabled = false;
                    c.OnSceneRemoved(wipe: true);
                    c.OnRemoved();
                }
            }
            Components.Unlock();
        }

        public void ClearGraphics() {
            if (Graphics == null) {
                return;
            }

            Graphics.Lock();
            foreach (Graphic g in Graphics) {
                GraphicRemoved(g);

                Components.Lock();
                foreach (Component c in Components) {
                    c.GraphicRemoved(g);
                }
                Components.Unlock();

                g.Dispose();
            }
            Graphics.Unlock();

            Graphics.Clear();
        }

        public void ClearComponents() {
            if (Components == null) {
                return;
            }

            foreach (Component c in Components) {
                ComponentRemoved(c);
                c.Enabled = false;
                c.OnSceneRemoved(wipe: true);
                c.OnRemoved();
            }

            Components.Clear();
        }

        public void RemoveSelf() {
            if (IsWiped) {
                return;
            }

            if (Scene == null) {
                throw new System.NullReferenceException("Can't remove from a null Scene.");
            }

            Scene.RemoveEntity(this);
        }

        public virtual void TransformChildAdded(Transform child) {
        }

        public virtual void TransformChildRemoved(Transform child) {
        }

        public virtual void TransformParentAdded() {
        }

        public virtual void TransformParentRemoved() {
        }

        public override string ToString() {
            return $"[Entity '{Name}' | {Transform} | Graphics: {Graphics.Count} Components: {Components.Count}]";
        }

        #endregion Public Methods

        #region Protected Methods

        protected virtual bool GraphicAdded(Graphic graphic) {
            return true;
        }

        protected virtual void GraphicRemoved(Graphic graphic) {
            if (graphic.ControlGroup != null) {
                graphic.ControlGroup.Unregister(graphic);
            }
        }

        protected virtual bool ComponentAdded(Component component) {
            return true;
        }

        protected virtual void ComponentRemoved(Component component) {
        }

        protected virtual void RendererChanged(Renderer renderer) {
        }

        protected bool CanUpdateChild(Transform child) {
            return !(!IsChildAvailable(child)
                || !child.Entity.Active
                || (child.Entity.ControlGroup != null && child.Entity.ControlGroup.IsPaused)
            );
        }

        protected bool CanRenderChild(Transform child) {
            return !(!IsChildAvailable(child)
                || !child.Entity.Visible
                || !child.Entity.AutoRender
            );
        }

        protected bool CanDebugRenderChild(Transform child) {
            return !(!IsChildAvailable(child)
                || !child.Entity.Visible
            );
        }

        protected bool IsChildAvailable(Transform child) {
            return !(child.IsDetached
                || child.Entity == null
                || !child.Entity.IsSceneFromTransformAncestor
            );
        }

        #endregion Protected Methods
    }
}
