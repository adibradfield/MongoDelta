using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MongoDelta
{
    public interface IMongoCollectionToQueryableConverter
    {
        IMongoQueryable<T> GetQueryable<T>(IMongoCollection<T> collection);
    }
}