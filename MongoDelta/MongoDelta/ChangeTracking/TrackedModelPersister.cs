using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDelta.MongoDbHelpers;
using MongoDelta.UpdateStrategies;

namespace MongoDelta.ChangeTracking
{
    internal class TrackedModelPersister<T> where T : class
    {
        public async Task PersistChangesAsync(IMongoCollection<T> collection,
            IClientSessionHandle existingSession, IEnumerable<WriteModel<T>> writeModels)
        {
            await ExecuteWithClientSession(collection, existingSession, async session =>
            {
                await collection.BulkWriteAsync(session, writeModels);
            });
        }

        private IEnumerable<WriteModel<T>> InsertNewModels(TrackedModelCollection<T> trackedModels)
        {
            var newModels = trackedModels.OfState(TrackedModelState.New).Select(m => m.Model).ToArray();
            return newModels.Select(m => new InsertOneModel<T>(m));
        }

        private IEnumerable<WriteModel<T>> DeleteRemovedModels(TrackedModelCollection<T> trackedModels)
        {
            var removedModels = trackedModels.OfState(TrackedModelState.Removed).Select(m => m.Model).ToArray();
            return removedModels.Select(m => new DeleteOneModel<T>(GenericBsonFilters.MatchSingleById(m)));
        }

        private IEnumerable<WriteModel<T>> UpdateChangedModels(TrackedModelCollection<T> trackedModels)
        {
            var updatedModels = trackedModels.OfState(TrackedModelState.Existing).Where(m => m.IsDirty).ToArray();
            var updateStrategy = UpdateStrategy.ForType<T>();

            return updatedModels.Select(m => updateStrategy.GetWriteModelForUpdate(m));
        }

        private async Task ExecuteWithClientSession(IMongoCollection<T> collection, IClientSessionHandle session, 
            Func<IClientSessionHandle, Task> action)
        {
            var closeSession = false;
            if (session == null)
            {
                session = collection.Database.Client.StartSession();
                closeSession = true;
            }

            try
            {
                await action(session);
            }
            finally
            {
                if (closeSession)
                {
                    session.Dispose();
                }
            }
        }

        public IEnumerable<WriteModel<T>> GetChangesForWrite(TrackedModelCollection<T> trackedModels)
        {
            return InsertNewModels(trackedModels)
                .Concat(DeleteRemovedModels(trackedModels))
                .Concat(UpdateChangedModels(trackedModels));
        }
    }
}