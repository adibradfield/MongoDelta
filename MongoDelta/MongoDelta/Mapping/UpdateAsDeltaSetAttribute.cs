using System;
using MongoDB.Bson.Serialization;

namespace MongoDelta.Mapping
{
    /// <summary>
    /// Updates the mapped member as if it was another MongoDb Collection. The mapped member must serialize to a BsonArray, and the type contained within
    /// the collection must have a mapped Id member. Items in the collection will only be updated if they have been added, removed or modified. Whether
    /// an item is detected as modified and how it updates is determined by the update strategy set for the type contained within the collection
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class UpdateAsDeltaSetAttribute : Attribute, IBsonMemberMapAttribute
    {
        private readonly Type _collectionItemType;

        /// <summary>
        /// Updates the mapped member as if it was another MongoDb Collection. The mapped member must serialize to a BsonArray, and the type contained within
        /// the collection must have a mapped Id member. Items in the collection will only be updated if they have been added, removed or modified. Whether
        /// an item is detected as modified and how it updates is determined by the update strategy set for the type contained within the collection
        /// </summary>
        /// <param name="collectionItemType">The type of item contained within the collection</param>
        public UpdateAsDeltaSetAttribute(Type collectionItemType)
        {
            _collectionItemType = collectionItemType;
        }

        /// <inheritdoc />
        public void Apply(BsonMemberMap memberMap)
        {
            memberMap.UpdateAsDeltaSet(_collectionItemType);
        }
    }
}
