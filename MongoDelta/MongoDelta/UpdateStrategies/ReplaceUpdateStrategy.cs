using MongoDB.Driver;
using MongoDelta.ChangeTracking;
using MongoDelta.MongoDbHelpers;

namespace MongoDelta.UpdateStrategies
{
    internal class ReplaceUpdateStrategy<T> : UpdateStrategy<T> where T : class
    {
        public override WriteModel<T> GetWriteModelForUpdate(TrackedModel<T> trackedModel)
        {
            return new ReplaceOneModel<T>(GenericBsonFilters.MatchSingleById(trackedModel.Model), trackedModel.Model);
        }
    }
}
