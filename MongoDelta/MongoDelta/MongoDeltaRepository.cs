using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDelta.ChangeTracking;
using MongoDelta.MongoDbHelpers;

namespace MongoDelta
{
    public class MongoDeltaRepository<T> where T : class
    {
        private readonly IMongoCollection<T> _collection;
        private readonly IMongoCollectionToQueryableConverter _collectionToQueryableConverter;
        private readonly IMongoQueryRunner _queryRunner;

        private readonly TrackedModelCollection<T> _trackedModels = new TrackedModelCollection<T>();
        private readonly TrackedModelPersister<T> _trackedModelPersister = new TrackedModelPersister<T>();

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
            var results = await _queryRunner.RunAsync(queryResults);

            foreach (var result in results)
            {
                _trackedModels.Existing(result);
            }

            return results;
        }

        public async Task<T> QuerySingleAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> query)
        {
            var queryable = query(_collectionToQueryableConverter.GetQueryable(_collection));
            var results = await _queryRunner.RunAsync(queryable);
            var result = results.SingleOrDefault();

            if (result != null)
            {
                _trackedModels.Existing(result);
            }

            return result;
        }

        public void Add(T model)
        {
            _trackedModels.New(model);
        }

        public void Remove(T model)
        {
            _trackedModels.Remove(model);
        }

        public async Task CommitChangesAsync(IClientSessionHandle session = null)
        {
            await _trackedModelPersister.PersistChangesAsync(_collection, _trackedModels, session);
        }
    }
}
