namespace MongoDelta.ChangeTracking.DirtyTracking
{
    interface IMemberDirtyTrackerTemplate
    {
        IMemberDirtyTracker ToDirtyTracker(object aggregate);
    }
}