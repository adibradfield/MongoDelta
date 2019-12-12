using MongoDB.Bson;
using MongoDelta.UpdateStrategies;

namespace MongoDelta.ChangeTracking.ElementChangeTrackers
{
    internal interface IDocumentElementChangeTracker
    {
        void ApplyChangesToDefinition(UpdateDefinition updateDefinition, BsonDocument original, BsonDocument current);
    }
}