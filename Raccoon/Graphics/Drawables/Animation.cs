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
            Initialize(animTexture);
        }

        public Animation(Atlas atlas, string name) : this(atlas.RetrieveAnimation(name)) {
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

        public Size FrameSize {
            get {
                if (CurrentTrack != null && CurrentTrack.Frames.Length > 0) {
                    ref Track.Frame frame = ref CurrentTrack.CurrentFrame;

                    if (frame.FrameDestination.HasValue) {
                        return frame.FrameDestination.Value.Size;
                    }

                    return frame.FrameRegion.Size;
                }

                return Size;
            }
        }

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

        public Track this[KeyType key] {
            get {
                try {
                    return Tracks[HandleKey(key)];
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

            if (CurrentTrack == null || CurrentTrack.FrameCount == 0) {
                return;
            }

            ElapsedTime += (int) Math.Round(delta * PlaybackSpeed);
            ref Track.Frame currentFrame = ref CurrentTrack.CurrentFrame;

            if (ElapsedTime >= currentFrame.Duration) {
                ElapsedTime -= currentFrame.Duration;
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

        public void Play(KeyType key, bool forceReset = true) {
            if (IsDisposed) {
                return;
            }

            key = HandleKey(key);
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

            Track track = Tracks[key];
            if (frameIndex < 0) {
                if (track.FrameCount + frameIndex < -track.FrameCount) {
                    throw new System.ArgumentOutOfRangeException($"Invalid frame reverse index value '{frameIndex}', allowed range is [{-track.FrameCount}, -1].");
                }

                track.CurrentFrameIndex = track.FrameCount + frameIndex;
            } else {
                if (frameIndex >= track.FrameCount) {
                    throw new System.ArgumentOutOfRangeException($"Invalid frame index value '{frameIndex}', allowed range is [0, {track.FrameCount - 1}].");
                }

                track.CurrentFrameIndex = frameIndex;
            }

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
            CurrentTrack?.Reset();
            UpdateClippingRegion();
            ElapsedTime = 0;
        }

        public Track Add(KeyType key, Track track) {
            if (track == null) {
                throw new System.ArgumentNullException(nameof(track));
            }

            Tracks.Add(HandleKey(key), track);
            return track;
        }

        public Track Add(KeyType key, IList<int> frames, IList<int> durations) {
            ValidateArraySize((ICollection) frames, "frames");
            ValidateArraySize((ICollection) durations, "durations");

            Track.Frame[] f = GenerateFrames(frames);

            for (int i = 0; i < f.Length; i++) {
                f[i].Duration = durations[i];
            }

            Track track = new Track(f);
            Add(key, track);
            return track;
        }

        public Track Add(KeyType key, string frames, string durations) {
            ValidateFrames(frames);
            ValidateDurations(durations);

            ParseFramesDuration(frames, durations, out List<int> durationList, out List<int> frameList);
            return Add(key, frameList, durationList);
        }

        public Track Add(KeyType key, IList<int> frames, int duration) {
            ValidateDuration(duration);

            Track.Frame[] f = GenerateFrames(frames);

            for (int i = 0; i < f.Length; i++) {
                f[i].Duration = duration;
            }

            Track track = new Track(f);
            Add(key, track);
            return track;
        }

        public Track Add(KeyType key, string frames, int duration) {
            ValidateFrames(frames);
            ValidateDuration(duration);

            ParseFrames(frames, out List<int> frameList);
            return Add(key, frameList, duration);
        }

        public Track AddRaw(KeyType key, Rectangle[] framesRegions, IList<int> durations) {
            ValidateArraySize(framesRegions, "framesRegions");
            ValidateArraySize((ICollection) durations, "durations");

            if (framesRegions.Length != durations.Count) {
                throw new System.ArgumentException($"Mismatch lengths at frames regions ({framesRegions.Length}) and durations ({durations.Count}).");
            }

            Track.Frame[] frames = new Track.Frame[framesRegions.Length];

            for (int i = 0; i < framesRegions.Length; i++) {
                ref Track.Frame frame = ref frames[i];
                frame = new Track.Frame(
                    -1,
                    durations[i],
                    framesRegions[i]
                );
            }

            Track track = new Track(frames);
            Add(key, track);
            return track;
        }

        public Track AddRaw(KeyType key, Rectangle[] framesRegions, int duration) {
            ValidateArraySize(framesRegions, "framesRegions");
            ValidateDuration(duration);

            Track.Frame[] frames = new Track.Frame[framesRegions.Length];

            for (int i = 0; i < framesRegions.Length; i++) {
                ref Track.Frame frame = ref frames[i];
                frame = new Track.Frame(
                    -1,
                    duration,
                    framesRegions[i]
                );
            }

            Track track = new Track(frames);
            Add(key, track);
            return track;
        }

        public Track AddRaw(KeyType key, Rectangle[] framesRegions, string durations) {
            ValidateArraySize(framesRegions, "framesRegions");
            ValidateDurations(durations);

            ParseDurations(durations, out List<int> durationList);
            return AddRaw(key, framesRegions, durationList);
        }

        public Track AddRaw(KeyType key, Rectangle[] framesRegions, Rectangle[] framesDestinations, IList<int> durations) {
            ValidateArraySize(framesRegions, "framesRegions");
            ValidateArraySize(framesDestinations, "framesDestinations");
            ValidateArraySize((ICollection) durations, "durations");

            if (framesRegions.Length != framesDestinations.Length) {
                throw new System.ArgumentException($"Mismatch lengths at frames regions ({framesRegions.Length}) and frames destinations ({framesDestinations.Length}).");
            }

            if (framesRegions.Length != durations.Count) {
                throw new System.ArgumentException($"Mismatch lengths at frames regions ({framesRegions.Length}) and durations ({durations.Count}).");
            }

            Track.Frame[] frames = new Track.Frame[framesRegions.Length];

            for (int i = 0; i < framesRegions.Length; i++) {
                ref Track.Frame frame = ref frames[i];
                frame = new Track.Frame(
                    -1,
                    durations[i],
                    framesRegions[i],
                    framesDestinations[i]
                );
            }

            Track track = new Track(frames);
            Add(key, track);
            return track;
        }

        public Track CloneAdd(KeyType targetKey, Track originalTrack) {
            Track targetTrack = new Track(originalTrack);
            Add(targetKey, targetTrack);
            return targetTrack;
        }

        public Track CloneAdd(KeyType targetKey, KeyType originalKey, bool reverse = false) {
            Track originalTrack = Tracks[originalKey];
            Track targetTrack = new Track(originalTrack);
            Add(targetKey, targetTrack);

            if (reverse) {
                targetTrack.Reverse();
            }

            return targetTrack;
        }

        public Track CloneAdd(KeyType targetKey, KeyType originalKey, Track.ReplaceFrame?[] replace, bool reverse = false) {
            Track originalTrack = Tracks[originalKey];
            Track targetTrack = new Track(originalTrack, replace);
            Add(targetKey, targetTrack);

            if (reverse) {
                targetTrack.Reverse();
            }

            return targetTrack;
        }

        public Track CloneAdd(KeyType targetKey, KeyType originalKey, IList<int> frames, bool reverse = false) {
            Track originalTrack = Tracks[originalKey];
            Track.Frame[] f = new Track.Frame[frames.Count];

            for (int i = 0; i < frames.Count; i++) {
                int localIndex = frames[i];

                if (localIndex < 0) {
                    localIndex = originalTrack.Frames.Length + localIndex;
                }

                ref Track.Frame originalFrame = ref originalTrack.Frames[localIndex];
                f[i] = originalFrame;
            }

            Track targetTrack = new Track(f);
            Add(targetKey, targetTrack);

            if (reverse) {
                targetTrack.Reverse();
            }

            return targetTrack;
        }

        public Track CloneAdd(KeyType targetKey, KeyType originalKey, string frames, bool reverse = false) {
            ParseFrames(frames, out List<int> frameList);
            return CloneAdd(targetKey, originalKey, frameList, reverse);
        }

        public Track CreateTrackFromTracks(KeyType newKey, KeyType key) {
            return CloneAdd(newKey, key);
        }

        public Track CreateTrackFromTracks(KeyType newKey, KeyType sourceKey, KeyType sourceKey2) {
            Track track = Tracks[sourceKey],
                  track2 = Tracks[sourceKey2];

            Track.Frame[] frames = new Track.Frame[track.Frames.Length + track2.Frames.Length];

            int i = 0;
            track.Frames.CopyTo(frames, i);
            i += track.Frames.Length;
            track2.Frames.CopyTo(frames, i);

            return Add(newKey, new Track(frames));
        }

        public Track CreateTrackFromTracks(KeyType newKey, KeyType sourceKey, KeyType sourceKey2, KeyType sourceKey3) {
            Track track = Tracks[sourceKey],
                  track2 = Tracks[sourceKey2],
                  track3 = Tracks[sourceKey3];

            Track.Frame[] frames = new Track.Frame[track.Frames.Length + track2.Frames.Length + track3.Frames.Length];

            int i = 0;
            track.Frames.CopyTo(frames, i);
            i += track.Frames.Length;
            track2.Frames.CopyTo(frames, i);
            i += track2.Frames.Length;
            track3.Frames.CopyTo(frames, i);

            return Add(newKey, new Track(frames));
        }

        public Track CreateTrackFromTracks(KeyType newKey, KeyType sourceKey, KeyType sourceKey2, KeyType sourceKey3, KeyType sourceKey4) {
            Track track = Tracks[sourceKey],
                  track2 = Tracks[sourceKey2],
                  track3 = Tracks[sourceKey3],
                  track4 = Tracks[sourceKey4];

            Track.Frame[] frames = new Track.Frame[track.Frames.Length + track2.Frames.Length + track3.Frames.Length + track4.Frames.Length];

            int i = 0;
            track.Frames.CopyTo(frames, i);
            i += track.Frames.Length;
            track2.Frames.CopyTo(frames, i);
            i += track2.Frames.Length;
            track3.Frames.CopyTo(frames, i);
            i += track3.Frames.Length;
            track4.Frames.CopyTo(frames, i);

            return Add(newKey, new Track(frames));
        }

        public Track CreateTrackFromTracks(KeyType newKey, params KeyType[] sourceKeys) {
            if (sourceKeys == null) {
                throw new System.ArgumentNullException(nameof(sourceKeys));
            }

            if (sourceKeys.Length == 0) {
                throw new System.ArgumentException("Expected KeyType[] with length > 0, but none was suplied.", nameof(sourceKeys));
            }

            int length = 0;
            for (int i = 0; i < sourceKeys.Length; i++) {
                Track track = Tracks[sourceKeys[i]];
                length += track.Frames.Length;
            }

            Track.Frame[] frames = new Track.Frame[length];

            length = 0;
            for (int i = 0; i < sourceKeys.Length; i++) {
                Track track = Tracks[sourceKeys[i]];
                track.Frames.CopyTo(frames, length);
                length += track.Frames.Length;
            }

            return Add(newKey, new Track(frames));
        }

        public bool ContainsTrack(KeyType key) {
            return Tracks.ContainsKey(HandleKey(key));
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

        protected void Initialize(AtlasAnimation animTexture) {
            // TODO: support KeyType with AtlasAnimation, converting string animations label to KeyType enum
            if (typeof(KeyType) != typeof(string)) {
                throw new System.NotSupportedException($"KeyType '{typeof(KeyType)}' doesn't support AtlasAnimationTexture, switch to string.");
            }

            Texture = animTexture.Texture;
            SourceRegion = animTexture.SourceRegion;
            ClippingRegion = animTexture["all"][0].ClippingRegion;

            foreach (KeyValuePair<string, List<AtlasAnimationFrame>> anim in animTexture) {
                Track.Frame[] frames = new Track.Frame[anim.Value.Count];

                for (int i = 0; i < anim.Value.Count; i++) {
                    AtlasAnimationFrame atlasFrame = anim.Value[i];
                    frames[i] = new Track.Frame(
                        atlasFrame.GlobalIndex,
                        atlasFrame.Duration,
                        atlasFrame.ClippingRegion,
                        new Rectangle(atlasFrame.OriginalFrame.Position, atlasFrame.ClippingRegion.Size)
                    );
                }

                Add((KeyType) (object) anim.Key, new Track(frames));
            }
        }

        protected override void Draw(
            Vector2 position,
            float rotation,
            Vector2 scale,
            ImageFlip flip,
            Color color,
            Vector2 scroll,
            Shader shader,
            IShaderParameters shaderParameters,
            Vector2 origin,
            float layerDepth
        ) {
            CalculateFrameOrigin(flip, ref origin);

            base.Draw(
                position,
                rotation,
                scale,
                flip,
                color,
                scroll,
                shader,
                shaderParameters,
                origin,
                layerDepth
            );
        }

        /// <summary>
        /// Process key before using it to retrieve a Track.
        /// </summary>
        protected virtual KeyType HandleKey(KeyType key) {
            return key;
        }

        protected void CalculateFrameOrigin(ImageFlip flip, ref Vector2 origin) {
            if (CurrentTrack == null || CurrentTrack.Frames.Length <= 0) {
                return;
            }

            // frame destination works like a guide to where render at local space
            // 'frameDestination.position' should be interpreted as 'origin'
            ref Track.Frame frame = ref CurrentTrack.CurrentFrame;

            if (!frame.FrameDestination.HasValue || frame.FrameDestination.Value.Position == Vector2.Zero) {
                return;
            }

            // frame region defines the crop rectangle at it's texture
            //ref Rectangle frameRegion = ref CurrentTrack.CurrentFrameRegion;

            if (flip.IsHorizontal()) {
                if (flip.IsVertical()) {
                    float rightSideSpacing = frame.FrameDestination.Value.Width - frame.FrameRegion.Width + frame.FrameDestination.Value.X,
                          bottomSideSpacing = frame.FrameDestination.Value.Height - frame.FrameRegion.Height + frame.FrameDestination.Value.Y;

                    origin = new Vector2(
                        (frame.FrameRegion.Width - origin.X) - rightSideSpacing,
                        (frame.FrameRegion.Height - origin.Y) - bottomSideSpacing
                    );
                } else {
                    float rightSideSpacing = frame.FrameDestination.Value.Width - frame.FrameRegion.Width + frame.FrameDestination.Value.X;

                    origin = new Vector2(
                        (frame.FrameRegion.Width - origin.X) - rightSideSpacing,
                        origin.Y + frame.FrameDestination.Value.Y
                    );
                }
            } else if (flip.IsVertical()) {
                float bottomSideSpacing = frame.FrameDestination.Value.Height - frame.FrameRegion.Height + frame.FrameDestination.Value.Y;

                origin = new Vector2(
                    origin.X + frame.FrameDestination.Value.X,
                    (frame.FrameRegion.Height - origin.Y) - bottomSideSpacing
                );
            } else {
                origin += frame.FrameDestination.Value.Position;
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

        #endregion Protected Methods

        #region Private Methods

        private void UpdateClippingRegion() {
            if (CurrentTrack != null) {
                ClippingRegion = CurrentTrack.CurrentFrame.FrameRegion;
            }
        }

        private Track.Frame[] GenerateFrames(IList<int> frames) {
            Track.Frame[] framesResult = new Track.Frame[frames.Count];
            Track allFramesTrack = AllFramesTrack;

            if (allFramesTrack != null) {
                for (int i = 0; i < frames.Count; i++) {
                    framesResult[i] = allFramesTrack.Frames[frames[i]];
                }
            } else {
                int columns = (int) (SourceRegion.Width / ClippingRegion.Width);
                //int rows = (int) (SourceRegion.Height / ClippingRegion.Height);

                for (int i = 0; i < frames.Count; i++) {
                    int frameGlobalId = frames[i];

                    framesResult[i] = new Track.Frame(
                        frameGlobalId,
                        0,
                        new Rectangle(
                            (frameGlobalId % columns) * ClippingRegion.Width,
                            (frameGlobalId / columns) * ClippingRegion.Height,
                            ClippingRegion.Width,
                            ClippingRegion.Height
                        ),
                        Raccoon.Rectangle.Empty
                    );
                }
            }

            return framesResult;
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
        #region Public Members

        public const string AllFramesTrackDefaultKey = "all";

        #endregion Public Members

        #region Constructors

        public Animation() : base() { }
        public Animation(Texture texture, Size frameSize) : base(texture, frameSize) { }
        public Animation(string filename, Size frameSize) : base(filename, frameSize) { }
        public Animation(AtlasSubTexture subTexture, Size frameSize) : base(subTexture, frameSize) { }
        public Animation(AtlasAnimation animTexture) : base(animTexture) { }
        public Animation(Atlas atlas, string name) : base(atlas, name) { }
        public Animation(Animation animation) : base(animation) { }

        #endregion Constructors

        #region Public Properties

        public override string AllFramesTrackKey {
            get {
                return AllFramesTrackDefaultKey;
            }
        }

        #endregion Public Properties

        #region Protected Methods

        protected override string HandleKey(string key) {
            return key.ToLowerInvariant();
        }

        #endregion Protected Methods
    }
}
