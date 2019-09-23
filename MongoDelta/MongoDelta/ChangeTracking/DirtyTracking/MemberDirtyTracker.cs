using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoDelta.ChangeTracking.DirtyTracking
{
    class MemberDirtyTracker<TAggregate> : IDirtyTracker where TAggregate : class
    {
        private readonly TAggregate _aggregate;
        private readonly BsonMemberMap _memberMap;

        public MemberDirtyTracker(TAggregate aggregate, BsonMemberMap memberMap)
        {
            _aggregate = aggregate;
            _memberMap = memberMap;
            OriginalValue = GetBsonValue(aggregate);
        }

        private BsonValue GetBsonValue(TAggregate aggregate)
        {
            return _memberMap.GetSerializer().ToBsonValue(_memberMap.Getter(aggregate));
        }

        public object OriginalValue { get; }

        public object CurrentValue => GetBsonValue(_aggregate);
        public bool IsDirty => !OriginalValue.Equals(CurrentValue);
    }
}
