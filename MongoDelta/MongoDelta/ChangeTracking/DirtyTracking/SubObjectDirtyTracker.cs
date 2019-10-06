using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoDelta.ChangeTracking.DirtyTracking
{
    class SubObjectDirtyTracker : MemberDirtyTracker, IObjectDirtyTracker
    {
        private readonly List<IMemberDirtyTracker> _memberTrackers;

        public SubObjectDirtyTracker(object aggregate, BsonMemberMap memberMap, BsonValue originalValue, List<IMemberDirtyTracker> memberTrackers) : base(aggregate, memberMap, originalValue)
        {
            _memberTrackers = memberTrackers;
        }

        public IReadOnlyCollection<IMemberDirtyTracker> MemberTrackers => Array.AsReadOnly(_memberTrackers.ToArray());
        public override bool IsDirty
        {
            get
            {
                if (CurrentValue == BsonNull.Value)
                {
                    return OriginalValue != BsonNull.Value;
                }

                return MemberTrackers.Any(t => t.IsDirty);
            }
        }
    }
}
