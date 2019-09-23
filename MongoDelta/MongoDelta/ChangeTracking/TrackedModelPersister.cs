using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDelta.MongoDbHelpers;

namespace MongoDelta.ChangeTracking
{
    class TrackedModelPersister<T> where T : class
    {
        public async Task PersistChangesAsync(IMongoCollection<T> collection, TrackedModelCollection<T> trackedModels, IClientSessionHandle existingSession = null)
        {
            await ExecuteWithClientSession(collection, existingSession, async session =>
            {
                await InsertNewModels(collection, session, trackedModels);
                await DeleteRemovedModels(collection, session, trackedModels);
                await UpdateChangedModels(collection, session, trackedModels);
            });
        }

        private async Task InsertNewModels(IMongoCollection<T> collection, IClientSessionHandle session, 
            TrackedModelCollection<T> trackedModels)
        {
            var newModels = trackedModels.OfState(TrackedModelState.New).Select(m => m.Model).ToArray();
            if (newModels.Any())
            {
                await collection.InsertManyAsync(session, newModels);
            }
        }

        private async Task DeleteRemovedModels(IMongoCollection<T> collection, IClientSessionHandle session, 
            TrackedModelCollection<T> trackedModels)
        {
            var removedModels = trackedModels.OfState(TrackedModelState.Removed).Select(m => m.Model).ToArray();
            if (removedModels.Any())
            {
                await collection.DeleteManyAsync(session, GenericBsonFilters.MatchMultipleById(removedModels));
            }
        }

        private async Task UpdateChangedModels(IMongoCollection<T> collection, IClientSessionHandle session,
            TrackedModelCollection<T> trackedModels)
        {
            var updatedModels = trackedModels.OfState(TrackedModelState.Existing).Where(m => m.IsDirty)
                .Select(m => m.Model).ToArray();
            
            foreach(var model in updatedModels)
            {
                await collection.ReplaceOneAsync(session, GenericBsonFilters.MatchSingleById(model), model);
            }
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
    }
}