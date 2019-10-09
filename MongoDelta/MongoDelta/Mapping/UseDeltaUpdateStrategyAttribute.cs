using System;
using MongoDB.Bson.Serialization;

namespace MongoDelta.Mapping
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UseDeltaUpdateStrategyAttribute : Attribute, IBsonClassMapAttribute
    {
        public void Apply(BsonClassMap classMap)
        {
            classMap.UseDeltaUpdateStrategy();
        }
    }
}
