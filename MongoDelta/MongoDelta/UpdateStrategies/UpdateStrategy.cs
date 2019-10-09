using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDelta.ChangeTracking;
using MongoDelta.Mapping;

namespace MongoDelta.UpdateStrategies
{
    internal abstract class UpdateStrategy
    {
        public static UpdateStrategy<T> ForType<T>() where T : class
        {
            var classMap = BsonClassMap.LookupClassMap(typeof(T));
            if (classMap.ShouldUseDeltaUpdateStrategy())
            {
                return new DeltaUpdateStrategy<T>();
            }
            else
            {
                return new ReplaceUpdateStrategy<T>();
            }
        }
    }

    internal abstract class UpdateStrategy<T> : UpdateStrategy where T : class
    {
        public abstract Task Update(IClientSessionHandle session, IMongoCollection<T> collection, TrackedModel<T> trackedModel);
    }
}
