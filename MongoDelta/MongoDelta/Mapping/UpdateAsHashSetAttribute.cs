using System;
using MongoDB.Bson.Serialization;

namespace MongoDelta.Mapping
{
    /// <summary>
    /// Updates the mapped member as if it were a hash set. The mapped member must serialize to a BsonArray. The equality is checked
    /// on the serialized BsonValue of each item in the BsonArray
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class UpdateAsHashSetAttribute : Attribute, IBsonMemberMapAttribute
    {
        /// <inheritdoc />
        public void Apply(BsonMemberMap memberMap)
        {
            memberMap.UpdateAsHashSet();
        }
    }
}
