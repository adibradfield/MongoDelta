using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MongoDelta
{
    class MongoQueryRunner : IMongoQueryRunner
    {
        public async Task<IReadOnlyCollection<T>> RunAsync<T>(IMongoQueryable<T> query)
        {
            return await query.ToListAsync();
        }
    }
}
