using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MongoDelta.MongoDbHelpers
{
    public interface IMongoCollectionToQueryableConverter
    {
        IMongoQueryable<T> GetQueryable<T>(IMongoCollection<T> collection);
    }
}