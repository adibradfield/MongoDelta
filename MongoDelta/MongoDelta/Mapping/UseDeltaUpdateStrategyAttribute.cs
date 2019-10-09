using System;
using MongoDB.Bson.Serialization;

namespace MongoDelta.Mapping
{
    /// <summary>
    /// When an item of this type is updated, mapped members should only be updated if they have changed
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class UseDeltaUpdateStrategyAttribute : Attribute, IBsonClassMapAttribute
    {
        /// <inheritdoc />
        public void Apply(BsonClassMap classMap)
        {
            classMap.UseDeltaUpdateStrategy();
        }
    }
}
