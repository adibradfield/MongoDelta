using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;

namespace MongoDelta.MongoDbHelpers
{
    public interface IMongoQueryRunner
    {
        Task<IReadOnlyCollection<T>> RunAsync<T>(IMongoQueryable<T> query);
        Task<T> RunSingleAsync<T>(IMongoQueryable<T> query);
    }
}