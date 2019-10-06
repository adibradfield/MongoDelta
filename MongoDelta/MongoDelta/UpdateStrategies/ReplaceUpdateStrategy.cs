using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDelta.ChangeTracking;
using MongoDelta.MongoDbHelpers;

namespace MongoDelta.UpdateStrategies
{
    internal class ReplaceUpdateStrategy<T> : UpdateStrategy<T> where T : class
    {
        public override async Task Update(IClientSessionHandle session, IMongoCollection<T> collection,
            TrackedModel<T> trackedModel)
        {
            await collection.ReplaceOneAsync(session, GenericBsonFilters.MatchSingleById(trackedModel.Model), trackedModel.Model);
        }
    }
}
