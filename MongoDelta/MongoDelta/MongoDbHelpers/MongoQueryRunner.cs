using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MongoDelta.MongoDbHelpers
{
    class MongoQueryRunner : IMongoQueryRunner
    {
        public async Task<IReadOnlyCollection<T>> RunAsync<T>(IMongoQueryable<T> query)
        {
            return await query.ToListAsync();
        }

        public async Task<T> RunSingleAsync<T>(IMongoQueryable<T> query)
        {
            return await query.SingleOrDefaultAsync();
        }
    }
}
