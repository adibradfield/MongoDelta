using System;
using MongoDB.Bson.Serialization;

namespace MongoDelta.Mapping
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class UpdateIncrementallyAttribute : Attribute, IBsonMemberMapAttribute
    {
        public void Apply(BsonMemberMap memberMap)
        {
            memberMap.UpdateIncrementally();
        }
    }
}
