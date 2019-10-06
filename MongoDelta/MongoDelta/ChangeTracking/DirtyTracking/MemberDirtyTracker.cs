using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoDelta.ChangeTracking.DirtyTracking
{
    internal class MemberDirtyTracker : IMemberDirtyTracker
    {
        private readonly object _aggregate;
        private readonly BsonMemberMap _memberMap;

        public MemberDirtyTracker(object aggregate, BsonMemberMap memberMap, BsonValue originalValue)
        {
            _aggregate = aggregate;
            _memberMap = memberMap;
            OriginalValue = originalValue;
        }

        private BsonValue GetBsonValue(object aggregate)
        {
            return _memberMap.GetSerializer().ToBsonValue(_memberMap.Getter(aggregate));
        }

        public BsonValue OriginalValue { get; }
        public BsonValue CurrentValue => GetBsonValue(_aggregate);
        public virtual bool IsDirty => !OriginalValue?.Equals(CurrentValue) ?? CurrentValue != null;
        public string ElementName => _memberMap.ElementName;
    }
}
