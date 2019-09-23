using System;
using MongoDelta.ChangeTracking.DirtyTracking;

namespace MongoDelta.ChangeTracking
{
    public enum TrackedModelState
    {
        New,
        Removed,
        Existing
    }

    class TrackedModel<T> where T :class
    {
        private readonly AggregateDirtyTracker<T> _dirtyTracker;

        public static TrackedModel<T> New(T model) => new TrackedModel<T>(model, TrackedModelState.New);
        public static TrackedModel<T> Existing(T model) => new TrackedModel<T>(model, TrackedModelState.Existing);
        public bool IsDirty => _dirtyTracker.IsDirty;

        private TrackedModel(T model, TrackedModelState state)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            State = state;
            _dirtyTracker = new AggregateDirtyTracker<T>(model);
        }

        public TrackedModelState State { get; }
        public T Model { get; }

        public TrackedModel<T> WithNewState(TrackedModelState newState)
        {
            return new TrackedModel<T>(Model, newState);
        }
    }
}
