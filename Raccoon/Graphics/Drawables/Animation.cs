using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Raccoon.Util;

namespace Raccoon.Graphics {
    public partial class Animation<KeyType> : Image {
        #region Private Readonly

        private static readonly Regex FrameRegex = new Regex(@"(\d+)-(\d+)|(\d+)");
        private static readonly Regex DurationRegex = new Regex(@"(\d+)");

        #endregion Private Readonly

        #region Constructors

        public Animation() { 
        }

        public Animation(Texture texture, Size frameSize) {
            Texture = texture;
            ClippingRegion = new Rectangle(frameSize);
            Load();
        }

        public Animation(string filename, Size frameSize) : this(new Texture(filename), frameSize) {
        }

        public Animation(AtlasSubTexture subTexture, Size frameSize) {
            Texture = subTexture.Texture;
            SourceRegion = subTexture.SourceRegion;
            ClippingRegion = new Rectangle(frameSize);
        }

        public Animation(AtlasAnimation animTexture) {
            // TODO: support KeyType with AtlasAnimation, converting string animations label to KeyType enum
            if (typeof(KeyType) != typeof(string)) {
                throw new System.NotSupportedException($"KeyType '{typeof(KeyType)}' doesn't support AtlasAnimationTexture, switch to string.");
            }

            Texture = animTexture.Texture;
            SourceRegion = animTexture.SourceRegion;
            ClippingRegion = animTexture["all"][0].ClippingRegion;

            foreach (KeyValuePair<string, List<AtlasAnimationFrame>> anim in animTexture) {
                Rectangle[] framesRegions = new Rectangle[anim.Value.Count];
                int[] durations = new int[framesRegions.Length];
                Rectangle[] destinationFrames = new Rectangle[anim.Value.Count];

                int i = 0;
                foreach (AtlasAnimationFrame frame in anim.Value) {
                    framesRegions[i] = frame.ClippingRegion;
                    durations[i] = frame.Duration;
                    destinationFrames[i] = new Rectangle(frame.OriginalFrame.Position, frame.ClippingRegion.Size);
                    i++;
                }

                Add((KeyType) (object) anim.Key, framesRegions, destinationFrames, durations);
            }
        }

        public Animation(Animation<KeyType> animation) {
            Texture = animation.Texture;
            SourceRegion = animation.SourceRegion;
            ClippingRegion = animation.ClippingRegion;

            if (!animation.DestinationRegion.IsEmpty) {
                DestinationRegion = animation.DestinationRegion;
            }

            foreach (KeyValuePair<KeyType, Track> track in animation.Tracks) {
                Add(track.Key, new Track(track.Value));
            }

            Origin = animation.Origin;
        }

        #endregion Constructors

        #region Public Properties

        public bool IsPlaying { get; private set; }
        public KeyType CurrentKey { get; private set; }
        public Track CurrentTrack { get; private set; }
        public int ElapsedTime { get; set; }
        public float PlaybackSpeed { get; set; } = 1f;
        public Dictionary<KeyType, Track>.KeyCollection TracksKeys { get { return Tracks.Keys; } }
        public int TrackCount { get { return Tracks.Count; } }

        public virtual KeyType AllFramesTrackKey {
            get {
                return default(KeyType);
            }
        }

        public Track AllFramesTrack {
            get {
                if (Tracks.TryGetValue(AllFramesTrackKey, out Track allFramesTrack)) {
                    return allFramesTrack;
                }

                return null;
            }
        }

        public virtual Track this[KeyType key] {
            get {
                try {
                    return Tracks[key];
                } catch (KeyNotFoundException e) {
                    throw new KeyNotFoundException($"Animation frame Key '{key}' not found.", e);
                }
            }
        }

        #endregion Public Properties

        #region Protected Properties

        protected Dictionary<KeyType, Track> Tracks = new Dictionary<KeyType, Track>();

        #endregion Protected Properties

        #region Public Methods

        public override void Update(int delta) {
            if (!IsPlaying) {
                return;
            }

            ElapsedTime += (int) Math.Round(delta * PlaybackSpeed);
            if (ElapsedTime >= CurrentTrack.CurrentFrameDuration) {
                ElapsedTime -= CurrentTrack.CurrentFrameDuration;
                CurrentTrack.NextFrame();
                if (CurrentTrack.HasEnded) {
                    Pause();
                } else {
                    UpdateClippingRegion();
                }
            }
        }

        public void Play(bool forceReset = true) {
            if (CurrentTrack == null) {
                throw new System.InvalidOperationException("Animation can't play or resume an invalid track");
            }

            IsPlaying = true;
            if (forceReset) {
                ElapsedTime = 0;
                CurrentTrack.Reset();
                UpdateClippingRegion();
            }
        }

        public virtual void Play(KeyType key, bool forceReset = true) {
            if (IsDisposed) {
                return;
            }

            IsPlaying = true;
            if (CurrentTrack == null || !CurrentKey.Equals(key)) {
                CurrentKey = key;
                CurrentTrack = Tracks[CurrentKey];
                ElapsedTime = 0;
                
                if (forceReset) {
                    CurrentTrack.Reset();
                }

                UpdateClippingRegion();
            } else if (forceReset) {
                ElapsedTime = 0;
                CurrentTrack.Reset();
                UpdateClippingRegion();
            }
        }

        public void Play(KeyType key, int frameIndex) {
            Play(key);
            Tracks[key].CurrentFrameIndex = frameIndex;
            UpdateClippingRegion();
        }

        public void Prepare(KeyType key, bool forceReset = true) {
            Play(key, forceReset);
            Pause();
        }

        public void Prepare(KeyType key, int frameIndex) {
            Play(key);
            Tracks[key].CurrentFrameIndex = frameIndex;
            UpdateClippingRegion();
            Pause();
        }

        public void Resume() {
            Play(false);
        }

        public void Pause() {
            IsPlaying = false;
        }

        public void Stop() {
            Pause();
            CurrentTrack.Reset();
            UpdateClippingRegion();
            ElapsedTime = 0;
        }

        public virtual Track Add(KeyType key, Rectangle[] framesRegions, string durations) {
            ValidateArraySize(framesRegions, "framesRegions");
            ValidateDurations(durations);

            ParseDurations(durations, out List<int> durationList);

            Track track = new Track(framesRegions, durationList.ToArray());
            Add(key, track);

            return track;
        }

        public virtual Track Add(KeyType key, Rectangle[] framesRegions, int duration) {
            ValidateArraySize(framesRegions, "framesRegions");
            ValidateDuration(duration);

            GenerateDurationsArray(framesRegions.Length, duration, out int[] durations);

            Track track = new Track(framesRegions, durations);
            Add(key, track);

            return track;
        }

        public virtual Track Add(KeyType key, Rectangle[] framesRegions, Rectangle[] framesDestinations, ICollection<int> durations) {
            ValidateArraySize(framesRegions, "framesRegions");
            ValidateArraySize(framesDestinations, "framesDestinations");
            ValidateArraySize((ICollection) durations, "durations");

            int[] durationList = new int[durations.Count];
            durations.CopyTo(durationList, 0);

            Track track = new Track(framesRegions, framesDestinations, durationList);
            Add(key, track);

            return track;
        }

        public virtual Track Add(KeyType key, Rectangle[] framesRegions, ICollection<int> durations) {
            ValidateArraySize(framesRegions, "framesRegions");
            ValidateArraySize((ICollection) durations, "durations");

            int[] durationList = new int[durations.Count];
            durations.CopyTo(durationList, 0);

            Track track = new Track(framesRegions, durationList);
            Add(key, track);

            return track;
        }

        public virtual Track Add(KeyType key, string frames, string durations) {
            ValidateFrames(frames);
            ValidateDurations(durations);

            ParseFramesDuration(frames, durations, out List<int> durationList, out List<int> frameList);

            Rectangle[] framesRegions = null, 
                        framesDestinations = null;

            GenerateFramesRegions(frameList.ToArray(), ref framesRegions, ref framesDestinations);

            Track track = new Track(framesRegions, durationList.ToArray());
            Add(key, track);

            return track;
        }

        public virtual Track Add(KeyType key, string frames, int duration) {
            ValidateFrames(frames);
            ValidateDuration(duration);

            ParseFrames(frames, out List<int> frameList);
            GenerateDurationsArray(frameList.Count, duration, out int[] durations);

            Rectangle[] framesRegions = null, 
                        framesDestinations = null;

            GenerateFramesRegions(frameList.ToArray(), ref framesRegions, ref framesDestinations);

            Track track = new Track(framesRegions, durations);
            Add(key, track);

            return track;
        }

        public virtual Track Add(KeyType key, ICollection<int> frames, ICollection<int> durations) {
            ValidateArraySize((ICollection) frames, "frames");
            ValidateArraySize((ICollection) durations, "durations");

            int[] frameList = new int[frames.Count];
            frames.CopyTo(frameList, 0);

            int[] durationList = new int[durations.Count];
            durations.CopyTo(durationList, 0);

            Rectangle[] framesRegions = null, 
                        framesDestinations = null;

            GenerateFramesRegions(frameList, ref framesRegions, ref framesDestinations);

            Track track = new Track(framesRegions, durationList);
            Add(key, track);

            return track;
        }

        public virtual Track Add(KeyType key, ICollection<int> frames, int duration) {
            ValidateDuration(duration);

            int[] frameList = new int[frames.Count];
            frames.CopyTo(frameList, 0);

            GenerateDurationsArray(frameList.Length, duration, out int[] durations);
            Rectangle[] framesRegions = null, 
                        framesDestinations = null;

            GenerateFramesRegions(frameList, ref framesRegions, ref framesDestinations);

            Track track = new Track(framesRegions, framesDestinations, durations);
            Add(key, track);

            return track;
        }

        public virtual Track Add(KeyType key, Track track) {
            Tracks.Add(key, track);
            return track;
        }

        public virtual Track CloneAdd(KeyType targetKey, KeyType originalKey, bool reverse = false) {
            Track originalTrack = Tracks[originalKey];
            Track targetTrack = new Track(originalTrack);
            Add(targetKey, targetTrack);

            if (reverse) {
                targetTrack.Reverse();
            }

            return targetTrack;
        }

        public virtual Track CloneAdd(KeyType targetKey, KeyType originalKey, string frames, bool reverse = false) {
            Track originalTrack = Tracks[originalKey];
            ParseFrames(frames, out List<int> frameList);

            Rectangle[] frameRegions = new Rectangle[frameList.Count],
                        frameDestinations = new Rectangle[frameList.Count];

            int[] durations = new int[frameList.Count];

            for (int i = 0; i < frameList.Count; i++) {
                int id = frameList[i];

                frameRegions[i] = originalTrack.FramesRegions[id];
                frameDestinations[i] = originalTrack.FramesDestinations[id];
                durations[i] = originalTrack.Durations[id];
            }

            Track targetTrack = new Track(originalTrack, frameRegions, frameDestinations, durations);
            Add(targetKey, targetTrack);

            if (reverse) {
                targetTrack.Reverse();
            }

            return targetTrack;
        }

        public virtual Track CloneAdd(KeyType targetKey, KeyType originalKey, Rectangle[] replaceFrameRegions, Rectangle[] replaceFrameDestinations, bool reverse = false) {
            Track originalTrack = Tracks[originalKey];
            Track targetTrack = new Track(originalTrack, replaceFrameRegions, replaceFrameDestinations, replaceDurations: null);
            Add(targetKey, targetTrack);

            if (reverse) {
                targetTrack.Reverse();
            }

            return targetTrack;
        }

        public virtual Track CloneAdd(KeyType targetKey, KeyType originalKey, int[] replaceDurations, bool reverse = false) {
            Track originalTrack = Tracks[originalKey];
            Track targetTrack = new Track(originalTrack, null, null, replaceDurations);
            Add(targetKey, targetTrack);

            if (reverse) {
                targetTrack.Reverse();
            }

            return targetTrack;
        }

        public virtual Track CloneAdd(KeyType targetKey, Track originalTrack) {
            Track targetTrack = new Track(originalTrack);
            Add(targetKey, targetTrack);
            return targetTrack;
        }

        public virtual Track CreateTrackFromTracks(KeyType newKey, KeyType[] keys) {
            if (keys == null) {
                throw new System.ArgumentNullException(nameof(keys));
            }

            if (keys.Length == 0) {
                throw new System.ArgumentException("Expected KeyType[] with length > 0, but none was suplied.", nameof(keys));
            }

            List<Rectangle> framesRegions = new List<Rectangle>();
            List<int> durations = new List<int>();
            List<Rectangle> destinationRegions = new List<Rectangle>();

            for (int i = 0; i < keys.Length; i++) {
                Track track = Tracks[keys[i]];

                framesRegions.AddRange(track.FramesRegions);
                durations.AddRange(track.Durations);
                destinationRegions.AddRange(track.FramesDestinations);

            }

            return Add(newKey, framesRegions.ToArray(), destinationRegions.ToArray(), durations);
        }

        public virtual bool ContainsTrack(KeyType key) {
            return Tracks.ContainsKey(key);
        }

        public void Refresh() {
            if (CurrentTrack != null) {
                UpdateClippingRegion();
            }
        }

        public override void Dispose() {
            if (IsDisposed) {
                return;
            }

            foreach (Track t in Tracks.Values) {
                t.Dispose();
            }

            Tracks.Clear();
            base.Dispose();
        }

        public override string ToString() {
            return $"[Animation | Position: {Position}, Texture: {Texture}]";
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void Draw(Vector2 position, float rotation, Vector2 scale, ImageFlip flip, Color color, Vector2 scroll, Shader shader, IShaderParameters shaderParameters, Vector2 origin, float layerDepth) {
            if (CurrentTrack != null && CurrentTrack.FramesDestinations != null) {
                // frame destination works like a guide to where render at local space
                // 'frameDestination.position' should be interpreted as 'origin'
                ref Rectangle frameDestination = ref CurrentTrack.CurrentFrameDestination;
                if (frameDestination.Position != Vector2.Zero) {
                    origin += frameDestination.Position;
                }

                // maybe we should make a careful check before modifying DestinationRegion here
                // user could modify value globally to Animation and we'll be interfering
                // making a change frame based here

                /*
                // there we give the power to frame regions deform animations
                // I don't know if it will be usefull anywhere, but we can do this
                ref Rectangle frameRegion = ref CurrentTrack.CurrentFrameRegion;
                if (frameDestination.Size != frameRegion.Size) {
                    DestinationRegion = new Rectangle(frameDestination.Size);
                }
                */
            }

            base.Draw(position, rotation, scale, flip, color, scroll, shader, shaderParameters, origin, layerDepth);
        }

        #endregion Protected Methods

        #region Private Methods

        private void UpdateClippingRegion() {
            ClippingRegion = CurrentTrack.CurrentFrameRegion;
        }

        private void GenerateFramesRegions(int[] frames, ref Rectangle[] framesRegions, ref Rectangle[] framesDestinations) {
            framesRegions = new Rectangle[frames.Length];
            framesDestinations = new Rectangle[frames.Length];

            Track allFramesTrack = AllFramesTrack;
            if (allFramesTrack != null) {
                for (int i = 0; i < frames.Length; i++) {
                    int frameId = frames[i];
                    framesRegions[i] = allFramesTrack.FramesRegions[frameId];
                    framesDestinations[i] = allFramesTrack.FramesDestinations[frameId];
                }
            } else {
                int columns = (int) (SourceRegion.Width / ClippingRegion.Width);
                //int rows = (int) (SourceRegion.Height / ClippingRegion.Height);

                for (int i = 0; i < frames.Length; i++) {
                    int frameId = frames[i];

                    Rectangle frameRegion = new Rectangle(
                        (frameId % columns) * ClippingRegion.Width,
                        (frameId / columns) * ClippingRegion.Height,
                        ClippingRegion.Width,
                        ClippingRegion.Height
                    );

                    framesRegions[i] = frameRegion;
                    framesDestinations[i] = Raccoon.Rectangle.Empty;
                }
            }
        }

        private void ParseFramesDuration(string frames, string durations, out List<int> durationList, out List<int> frameList) {
            ParseDurations(durations, out durationList);

            frameList = new List<int>();
            int frameOrderIndex = 0;

            foreach (Match match in FrameRegex.Matches(frames)) {
                string frameId = match.Groups[3].Value;

                if (frameId.Length > 0) {
                    // single frame entry

                    frameList.Add(int.Parse(frameId));

                    // expand duration list, by cloning the last duration's entry, to maintain same size on both lists
                    if (frameOrderIndex >= durationList.Count) {
                        durationList.Add(durationList[durationList.Count - 1]);
                    }

                    frameOrderIndex++;
                } else {
                    // frames range entry (format: from-to)

                    int duration = durationList[frameOrderIndex],
                        startRange = int.Parse(match.Groups[1].Value),
                        endRange = int.Parse(match.Groups[2].Value);

                    durationList.RemoveAt(frameOrderIndex);
                    for (int i = startRange; i <= endRange; i++) {
                        // clone duration at frameOrderIndex
                        frameList.Add(i);
                        durationList.Insert(frameOrderIndex, duration);
                        frameOrderIndex++;
                    }
                }
            }

            if (frameList.Count == 0) {
                throw new System.ArgumentException("Wrong value format.", "frames");
            }
        }

        private void ParseFrames(string frames, out List<int> frameList) {
            frameList = new List<int>();

            foreach (Match match in FrameRegex.Matches(frames)) {
                string frameId = match.Groups[3].Value;

                if (frameId.Length > 0) {
                    frameList.Add(int.Parse(frameId));
                } else {
                    int startRange = int.Parse(match.Groups[1].Value),
                        endRange = int.Parse(match.Groups[2].Value);

                    for (int i = startRange; i <= endRange; i++) {
                        frameList.Add(i);
                    }
                }
            }

            if (frameList.Count == 0) {
                throw new System.ArgumentException("Wrong value format.", "frames");
            }
        }

        private void ParseDurations(string durations, out List<int> durationList) {
            durationList = new List<int>();
            foreach (Match match in DurationRegex.Matches(durations)) {
                durationList.Add(int.Parse(match.Groups[1].Value));
            }

            if (durationList.Count == 0) {
                throw new System.ArgumentException("Wrong value format.", "durations");
            }
        }

        private void GenerateDurationsArray(int frameCount, int duration, out int[] durations) {
            durations = new int[frameCount];
            for (int i = 0; i < frameCount; i++) {
                durations.SetValue(duration, i);
            }
        }

        private void ValidateFrames(string frames) {
            if (string.IsNullOrWhiteSpace(frames)) {
                throw new System.ArgumentException("Value is empty.", "frames");
            }
        }

        private void ValidateDurations(string durations) {
            if (string.IsNullOrWhiteSpace(durations)) {
                throw new System.ArgumentException("Value is empty.", "durations");
            }
        }

        private void ValidateDuration(int duration) {
            if (duration < 0) {
                throw new System.ArgumentException("Value is invalid, must be greater or equal zero.", "durations");
            }
        }

        private void ValidateArraySize(ICollection collection, string name) {
            if (collection.Count == 0) {
                throw new System.ArgumentException("Array is empty.", name);
            }
        }

        #endregion Private Methods
    }

    public class Animation : Animation<string> {
        #region Constructors

        public Animation() : base() { }
        public Animation(Texture texture, Size frameSize) : base(texture, frameSize) { }
        public Animation(string filename, Size frameSize) : base(filename, frameSize) { }
        public Animation(AtlasSubTexture subTexture, Size frameSize) : base(subTexture, frameSize) { }
        public Animation(AtlasAnimation animTexture) : base(animTexture) { }
        public Animation(Animation animation) : base(animation) { }

        #endregion Constructors

        public override string AllFramesTrackKey {
            get {
                return "all";
            }
        }

        public override Track this[string key] {
            get {
                try {
                    return Tracks[key.ToLowerInvariant()];
                } catch (KeyNotFoundException e) {
                    throw new KeyNotFoundException($"Animation frame Key '{key}' not found.", e);
                }
            }
        }

        public override void Play(string key, bool forceReset = true) {
            base.Play(key.ToLowerInvariant(), forceReset);
        }

        public override Track Add(string key, Rectangle[] framesRegions, string durations) {
            return base.Add(key.ToLowerInvariant(), framesRegions, durations);
        }

        public override Track Add(string key, Rectangle[] framesRegions, int duration) {
            return base.Add(key.ToLowerInvariant(), framesRegions, duration);
        }

        public override Track Add(string key, Rectangle[] framesRegions, Rectangle[] framesDestinations, ICollection<int> durations) {
            return base.Add(key.ToLowerInvariant(), framesRegions, framesDestinations, durations);
        }

        public override Track Add(string key, Rectangle[] framesRegions, ICollection<int> durations) {
            return base.Add(key.ToLowerInvariant(), framesRegions, durations);
        }

        public override Track Add(string key, string frames, string durations) {
            return base.Add(key.ToLowerInvariant(), frames, durations);
        }

        public override Track Add(string key, string frames, int duration) {
            return base.Add(key.ToLowerInvariant(), frames, duration);
        }

        public override Track Add(string key, ICollection<int> frames, ICollection<int> durations) {
            return base.Add(key.ToLowerInvariant(), frames, durations);
        }

        public override Track Add(string key, ICollection<int> frames, int duration) {
            return base.Add(key.ToLowerInvariant(), frames, duration);
        }

        public override Track Add(string key, Track track) {
            return base.Add(key.ToLowerInvariant(), track);
        }

        public override Track CloneAdd(string targetKey, string originalKey, bool reverse = false) {
            return base.CloneAdd(targetKey.ToLowerInvariant(), originalKey.ToLowerInvariant(), reverse);
        }

        public override Track CloneAdd(string targetKey, string originalKey, Rectangle[] replaceFrameRegions, Rectangle[] replaceFrameDestinations, bool reverse = false) {
            return base.CloneAdd(targetKey.ToLowerInvariant(), originalKey, replaceFrameRegions, replaceFrameDestinations);
        }

        public override Track CloneAdd(string targetKey, string originalKey, int[] replaceDurations, bool reverse = false) {
            return base.CloneAdd(targetKey.ToLowerInvariant(), originalKey, replaceDurations, reverse);
        }

        public override Track CreateTrackFromTracks(string newKey, string[] keys) {
            return base.CreateTrackFromTracks(newKey.ToLowerInvariant(), keys);
        }

        public override bool ContainsTrack(string key) {
            return base.ContainsTrack(key.ToLowerInvariant());
        }
    }
}
