using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDelta.ChangeTracking;
using MongoDelta.MongoDbHelpers;
using MongoDelta.UpdateStrategies;

namespace MongoDelta
{
    /// <summary>
    /// The abstract base class for MongoDeltaRepository instances
    /// </summary>
    public abstract class MongoDeltaRepository
    {
        internal MongoDeltaRepository(){}
        internal abstract Task CommitChangesAsync(IClientSessionHandle session, PreparedWriteModel writeModel);
        internal abstract PreparedWriteModel PrepareChangesForWrite();
    }

    /// <summary>
    /// Provides methods for retrieving and modifying the items in the MongoDB collection
    /// </summary>
    /// <typeparam name="T">The type to use for the collection</typeparam>
    public class MongoDeltaRepository<T> : MongoDeltaRepository where T : class
    {
        private readonly IMongoCollection<T> _collection;
        private readonly IMongoCollectionToQueryableConverter _collectionToQueryableConverter;
        private readonly IMongoQueryRunner _queryRunner;

        private readonly TrackedModelCollection<T> _trackedModels = new TrackedModelCollection<T>();
        private readonly TrackedModelPersister<T> _trackedModelPersister = new TrackedModelPersister<T>();

        internal MongoDeltaRepository(IMongoCollection<T> collection) : this(collection,
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

        /// <summary>
        /// Queries multiple items from the collection
        /// </summary>
        /// <param name="query">The function to apply on the IMongoQueryable instance to get the desired results</param>
        /// <returns>The items that match the query</returns>
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

        /// <summary>
        /// Queries a single item from the collection. Throws an exception if there are multiple matching items
        /// </summary>
        /// <param name="query">The function to apply on the IMongoQueryable instance to get the desired results</param>
        /// <returns>A single item that matches the query</returns>
        public async Task<T> QuerySingleAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> query)
        {
            var queryable = query(_collectionToQueryableConverter.GetQueryable(_collection));
            T result = await _queryRunner.RunSingleAsync(queryable);

            if (result != null)
            {
                _trackedModels.Existing(result);
            }

            return result;
        }

        /// <summary>
        /// Queries multiple items from the collection
        /// </summary>
        /// <param name="filter">The filter for the items</param>
        /// <returns>The items that match the query</returns>
        public async Task<IReadOnlyCollection<T>> QueryAsync(Expression<Func<T, bool>> filter)
        {
            return await QueryAsync(query => query.Where(filter));
        }

        /// <summary>
        /// Queries a single item from the collection. Throws an exception if there are multiple matching items
        /// </summary>
        /// <param name="filter">The filter for the items</param>
        /// <returns>A single item that matches the query</returns>
        public async Task<T> QuerySingleAsync(Expression<Func<T, bool>> filter)
        {
            return await QuerySingleAsync(query => query.Where(filter));
        }

        /// <summary>
        /// Adds a new item to the collection
        /// </summary>
        /// <param name="model">The item to add</param>
        public void Add(T model)
        {
            _trackedModels.New(model);
        }

        /// <summary>
        /// Removes an item from the collection
        /// </summary>
        /// <param name="model">The model to remove</param>
        public void Remove(T model)
        {
            _trackedModels.Remove(model);
        }

        internal override async Task CommitChangesAsync(IClientSessionHandle session,
            PreparedWriteModel writeModel)
        {
            var typedWriteModel = (PreparedWriteModel<T>) writeModel;
            if (typedWriteModel.HasChanges)
            {
                await _trackedModelPersister.PersistChangesAsync(_collection, session, typedWriteModel.MongoWriteModels);
            }
        }

        internal override PreparedWriteModel PrepareChangesForWrite()
        {
            var preparedWriteModel = new PreparedWriteModel<T>(_trackedModelPersister.GetChangesForWrite(_trackedModels));
            preparedWriteModel.ThrowIfNotValid();
            return preparedWriteModel;
        }
    }
}
