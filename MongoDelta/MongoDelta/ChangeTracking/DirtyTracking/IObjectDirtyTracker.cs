using System.Collections.Generic;

namespace MongoDelta.ChangeTracking.DirtyTracking
{
    interface IObjectDirtyTracker
    {
        IReadOnlyCollection<IMemberDirtyTracker> MemberTrackers { get; }
    }
}