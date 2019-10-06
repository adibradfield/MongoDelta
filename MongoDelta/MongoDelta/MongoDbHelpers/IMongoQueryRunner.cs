using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;

namespace MongoDelta.MongoDbHelpers
{
    internal interface IMongoQueryRunner
    {
        Task<IReadOnlyCollection<T>> RunAsync<T>(IMongoQueryable<T> query);
        Task<T> RunSingleAsync<T>(IMongoQueryable<T> query);
    }
}