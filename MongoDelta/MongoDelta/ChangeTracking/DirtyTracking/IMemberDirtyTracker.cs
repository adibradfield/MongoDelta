using MongoDB.Bson;

namespace MongoDelta.ChangeTracking.DirtyTracking
{
    interface IMemberDirtyTracker : IDirtyTracker
    {
        BsonValue OriginalValue { get; }
        BsonValue CurrentValue { get; }
        string ElementName { get; }
    }
}