using System;
using MongoDB.Bson.Serialization;

namespace MongoDelta.Mapping
{
    /// <summary>
    /// Specifies that the member should be updated incrementally. Only valid for members that serialize to the
    /// BSON types: Int32, Int64, Double or Decimal128
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class UpdateIncrementallyAttribute : Attribute, IBsonMemberMapAttribute
    {
        /// <inheritdoc />
        public void Apply(BsonMemberMap memberMap)
        {
            memberMap.UpdateIncrementally();
        }
    }
}
