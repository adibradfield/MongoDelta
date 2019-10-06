using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDelta.ChangeTracking;
using MongoDelta.ChangeTracking.DirtyTracking;
using MongoDelta.Mapping;
using MongoDelta.MongoDbHelpers;

namespace MongoDelta.UpdateStrategies
{
    internal class DeltaUpdateStrategy<T> : UpdateStrategy<T> where T : class
    {
        public override async Task Update(IClientSessionHandle session, IMongoCollection<T> collection, TrackedModel<T> trackedModel)
        {
            var updateDefinition = GetUpdateDefinitionForObject(trackedModel.DirtyTracker, typeof(T), trackedModel.Model);

            await collection.UpdateOneAsync(session, GenericBsonFilters.MatchSingleById(trackedModel.Model),
                updateDefinition.ToMongoUpdateDefinition<T>());
        }

        private DeltaUpdateDefinition GetUpdateDefinitionForObject(IObjectDirtyTracker dirtyTracker, Type objectType, object aggregate)
        {
            var updateDefinition = new DeltaUpdateDefinition();
            var classMap = BsonClassMap.LookupClassMap(objectType);
            foreach (var memberTracker in dirtyTracker.MemberTrackers.Where(t => t.IsDirty))
            {
                var memberMap = classMap.GetMemberMapForElement(memberTracker.ElementName);
                var memberClassMap = BsonClassMap.LookupClassMap(memberMap.MemberType);
                var memberUpdateConfiguration = memberClassMap.GetDeltaUpdateConfiguration();

                if (memberUpdateConfiguration.UseDeltaUpdateStrategy)
                {
                    var objectTracker = (IObjectDirtyTracker) memberTracker;
                    var model = memberMap.Getter(aggregate);

                    if (model == null)
                    {
                        updateDefinition.Set(memberTracker.ElementName, BsonNull.Value);
                        continue;
                    }

                    if (memberTracker.OriginalValue == BsonNull.Value)
                    {
                        updateDefinition.Set(memberTracker.ElementName, memberTracker.CurrentValue);
                        continue;
                    }

                    var subUpdateDefinition = GetUpdateDefinitionForObject(objectTracker, memberMap.MemberType, model);
                    updateDefinition.Merge(memberTracker.ElementName, subUpdateDefinition);
                }
                else
                {
                    updateDefinition.Set(memberTracker.ElementName, memberTracker.CurrentValue);
                }
            }
            return updateDefinition;
        }
    }
}
