using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MongoDelta
{
    public class MongoDeltaRepository<T>
    {
        private readonly IMongoCollection<T> _collection;
        private readonly IMongoCollectionToQueryableConverter _collectionToQueryableConverter;
        private readonly IMongoQueryRunner _queryRunner;

        private readonly Dictionary<T, TrackedModel<T>> _trackedModels = new Dictionary<T, TrackedModel<T>>();

        public MongoDeltaRepository(IMongoCollection<T> collection) : this(collection,
            new MongoCollectionToQueryableConverter(), new MongoQueryRunner())
        {
        }

        internal MongoDeltaRepository(IMongoCollection<T> collection,
            IMongoCollectionToQueryableConverter collectionToQueryableConverter,
            IMongoQueryRunner queryRunner)
        {
            _collection = collection;
            _collectionToQueryableConverter = collectionToQueryableConverter;
            _queryRunner = queryRunner;
        }

        public async Task<IReadOnlyCollection<T>> QueryAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> query)
        {
            var queryResults = query(_collectionToQueryableConverter.GetQueryable(_collection));
            return await _queryRunner.RunAsync(queryResults);
        }

        public async Task<T> QuerySingleAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> query)
        {
            var queryable = query(_collectionToQueryableConverter.GetQueryable(_collection));
            var results = await _queryRunner.RunAsync(queryable);
            return results.SingleOrDefault();
        }

        public void Add(T model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            _trackedModels.Add(model, new TrackedModel<T>
            {
                Model = model,
                State = TrackedModel.TrackedModelState.New
            });
        }

        public async Task PersistChangesAsync(IClientSessionHandle existingSession = null)
        {
            await ExecuteWithClientSession(existingSession, async session => { await InsertNewModels(session); });
        }

        private async Task InsertNewModels(IClientSessionHandle session)
        {
            var newModels = _trackedModels.Values.Where(m => m.State == TrackedModel.TrackedModelState.New)
                .Select(m => m.Model).ToArray();
            if (newModels.Any())
            {
                await _collection.InsertManyAsync(session, newModels);
            }
        }

        private async Task ExecuteWithClientSession(IClientSessionHandle session, Func<IClientSessionHandle, Task> action)
        {
            var closeSession = false;
            if (session == null)
            {
                session = _collection.Database.Client.StartSession();
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
