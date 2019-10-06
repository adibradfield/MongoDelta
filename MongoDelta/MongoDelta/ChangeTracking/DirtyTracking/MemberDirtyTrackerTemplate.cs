using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoDelta.ChangeTracking.DirtyTracking
{
    class MemberDirtyTrackerTemplate : IMemberDirtyTrackerTemplate
    {
        private readonly BsonMemberMap _map;
        private readonly BsonValue _originalValue;

        public MemberDirtyTrackerTemplate(object aggregate, BsonMemberMap map)
        {
            _map = map;
            _originalValue = aggregate == null ? null : _map.GetSerializer().ToBsonValue(_map.Getter(aggregate));
        }

        public IMemberDirtyTracker ToDirtyTracker(object aggregate)
        {
            return new MemberDirtyTracker(aggregate, _map, _originalValue);
        }
    }
}
