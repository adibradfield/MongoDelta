using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoDelta.MongoDbHelpers
{
    internal static class GenericBsonFilters
    {
        public static BsonDocument MatchSingleById<TAggregate>(TAggregate aggregate)
        {
            var id = GetIdFromAggregate(aggregate, out var idElementName);
            return new BsonDocument(idElementName, id);
        }

        public static BsonDocument MatchMultipleById<TAggregate>(IEnumerable<TAggregate> aggregates)
        {
            var idCollection = GetIdsFromAggregates(aggregates, out var idElementName);
            return new BsonDocument(idElementName, new BsonDocument("$in", new BsonArray(idCollection)));
        }

        private static BsonValue[] GetIdsFromAggregates<TAggregate>(IEnumerable<TAggregate> aggregates, out string elementName)
        {
            var mapper = BsonClassMap.LookupClassMap(typeof(TAggregate));
            var idSerializer = mapper.IdMemberMap.GetSerializer();
            elementName = mapper.IdMemberMap.ElementName;
            return aggregates.Select(m => idSerializer.ToBsonValue(mapper.IdMemberMap.Getter(m))).ToArray();
        }

        private static BsonValue GetIdFromAggregate<TAggregate>(TAggregate aggregate, out string elementName)
        {
            return GetIdsFromAggregates(new[] {aggregate}, out elementName).Single();
        }
    }
}
