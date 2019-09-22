namespace MongoDelta
{
    class TrackedModel
    {
        public enum TrackedModelState
        {
            New
        }
    }

    class TrackedModel<T> : TrackedModel
    {
        public TrackedModelState State { get; set; }
        public T Model { get; set; }
    }
}
