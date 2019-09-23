namespace MongoDelta.ChangeTracking.DirtyTracking
{
    public interface IDirtyTracker
    {
        bool IsDirty { get; }
    }
}