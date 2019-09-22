﻿using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace MongoDelta
{
    class MongoCollectionToQueryableConverter : IMongoCollectionToQueryableConverter
    {
        public IMongoQueryable<T> GetQueryable<T>(IMongoCollection<T> collection)
        {
            return collection.AsQueryable();
        }
    }
}
