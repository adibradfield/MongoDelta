using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MongoDelta.MongoDbHelpers
{
    internal interface IMongoCollectionToQueryableConverter
    {
        IMongoQueryable<T> GetQueryable<T>(IMongoCollection<T> collection);
    }
}