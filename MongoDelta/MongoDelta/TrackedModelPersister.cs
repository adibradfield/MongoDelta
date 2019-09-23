using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace MongoDelta
{
    class TrackedModelPersister<T> where T : class
    {
        public async Task PersistChangesAsync(IMongoCollection<T> collection, TrackedModelCollection<T> trackedModels, IClientSessionHandle existingSession = null)
        {
            await ExecuteWithClientSession(collection, existingSession, async session =>
            {
                await InsertNewModels(collection, session, trackedModels);
                await DeleteRemovedModels(collection, session, trackedModels);
            });
        }

        private async Task InsertNewModels(IMongoCollection<T> collection, IClientSessionHandle session, TrackedModelCollection<T> trackedModels)
        {
            var newModels = trackedModels.OfState(TrackedModelState.New).Select(m => m.Model).ToArray();
            if (newModels.Any())
            {
                await collection.InsertManyAsync(session, newModels);
            }
        }

        private async Task DeleteRemovedModels(IMongoCollection<T> collection, IClientSessionHandle session, TrackedModelCollection<T> trackedModels)
        {
            var removedModels = trackedModels.OfState(TrackedModelState.Removed).Select(m => m.Model).ToArray();
            if (removedModels.Any())
            {
                var mapper = BsonClassMap.LookupClassMap(typeof(T));
                var idSerializer = mapper.IdMemberMap.GetSerializer();
                var idsToRemove = removedModels.Select(m => idSerializer.ToBsonValue(mapper.IdMemberMap.Getter(m))).ToArray();
                var idElementName = mapper.IdMemberMap.ElementName;
                await collection.DeleteManyAsync(session, new BsonDocument(idElementName,
                    new BsonDocument("$in", new BsonArray(idsToRemove))));
            }
        }

        private async Task ExecuteWithClientSession(IMongoCollection<T> collection, IClientSessionHandle session, Func<IClientSessionHandle, Task> action)
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