using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDelta.ChangeTracking;
using MongoDelta.MongoDbHelpers;

namespace MongoDelta.UpdateStrategies
{
    internal class DeltaUpdateStrategy<T> : UpdateStrategy<T> where T : class
    {
        public override async Task Update(IClientSessionHandle session, IMongoCollection<T> collection, TrackedModel<T> trackedModel)
        {
            var updateDocument = new BsonDocument();
            foreach (var dirtyTracker in trackedModel.MemberTrackers.Where(t => t.IsDirty))
            {
                updateDocument.Add(new BsonElement(dirtyTracker.ElementName, dirtyTracker.CurrentValue));
            }
            var updateDefinition = new BsonDocumentUpdateDefinition<T>(new BsonDocument("$set", updateDocument));

            await collection.UpdateOneAsync(session, GenericBsonFilters.MatchSingleById(trackedModel.Model),
                updateDefinition);
        }
    }
}
