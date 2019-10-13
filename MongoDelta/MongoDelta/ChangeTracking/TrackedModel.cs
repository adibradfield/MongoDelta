using System;
using MongoDB.Bson;

namespace MongoDelta.ChangeTracking
{
    internal enum TrackedModelState
    {
        New,
        Removed,
        Existing
    }

    internal class TrackedModel<T> where T :class
    {
        public static TrackedModel<T> New(T model) => new TrackedModel<T>(model, TrackedModelState.New);
        public static TrackedModel<T> Existing(T model) => new TrackedModel<T>(model, TrackedModelState.Existing);

        private TrackedModel(T model, TrackedModelState state)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            State = state;
            OriginalDocument = CurrentDocument;
        }

        public TrackedModelState State { get; }
        public T Model { get; }
        public BsonDocument OriginalDocument { get; }
        public BsonDocument CurrentDocument => Model.ToBsonDocument();

        public TrackedModel<T> WithNewState(TrackedModelState newState)
        {
            return new TrackedModel<T>(Model, newState);
        }
    }
}
