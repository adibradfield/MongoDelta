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

        public MongoDeltaRepository(IMongoCollection<T> collection,
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
    }
}
