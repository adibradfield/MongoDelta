namespace MongoDelta.ChangeTracking.DirtyTracking
{
    internal interface IDirtyTracker
    {
        bool IsDirty { get; }
    }
}