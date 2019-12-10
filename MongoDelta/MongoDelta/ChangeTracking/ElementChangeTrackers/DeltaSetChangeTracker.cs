using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDelta.UpdateStrategies;

namespace MongoDelta.ChangeTracking.ElementChangeTrackers
{
    class DeltaSetChangeTracker : DocumentElementChangeTrackerBase
    {
        private readonly Type _itemType;
        private readonly DocumentChangeTracker _itemChangeTracker;
        private readonly BsonClassMap _itemClassMap;

        public DeltaSetChangeTracker(BsonMemberMap memberMap, Type itemType) : base(memberMap)
        {
            _itemType = itemType;
            _itemChangeTracker = new DocumentChangeTracker(itemType);
            _itemClassMap = BsonClassMap.LookupClassMap(itemType);
        }

        protected override void ApplyChangesToDefinition(UpdateDefinition updateDefinition, BsonValue originalValue, BsonValue currentValue)
        {
            var (addedValues, removedValues, existingValues) = GetCollectionValuesByState(originalValue, currentValue);
            var idElementName = _itemClassMap.IdMemberMap.ElementName;
            var valuesToUpdate = existingValues.Select(v => new
            {
                Id = v.newValue[idElementName],
                UpdateDefinition = _itemChangeTracker.GetUpdatesForChanges(v.oldValue, v.newValue, updateDefinition.ArrayFilterNamingStrategy)
            }).Where(v => v.UpdateDefinition.HasChanges);

            updateDefinition.AddAndRemoveFromDeltaSet(MemberMap.ElementName, addedValues, removedValues, idElementName, _itemType);

            foreach (var valueToUpdate in valuesToUpdate)
            {
                var arrayFilterName = updateDefinition.CreateArrayFilter(_itemType, idElementName, valueToUpdate.Id);
                updateDefinition.Merge($"{MemberMap.ElementName}.$[{arrayFilterName}]", valueToUpdate.UpdateDefinition, new []{arrayFilterName});
            }
        }

        private (List<BsonDocument> addedValues, List<BsonDocument> removedValues, List<(BsonDocument oldValue, BsonDocument newValue)> existingValues) GetCollectionValuesByState(BsonValue originalValue,
            BsonValue currentValue)
        {
            var idElementName = _itemClassMap.IdMemberMap.ElementName;
            var originalValues = originalValue.AsBsonArray.Values.Select(v => v.AsBsonDocument)
                .ToDictionary(v => v.AsBsonDocument[idElementName]);
            var currentValues = currentValue.AsBsonArray.Values.Select(v => v.AsBsonDocument)
                .ToDictionary(v => v.AsBsonDocument[idElementName]);

            List<BsonDocument> addedValues = new List<BsonDocument>(), removedValues = new List<BsonDocument>();
            List<(BsonDocument oldValue, BsonDocument newValue)> existingValues = new List<(BsonDocument oldValue, BsonDocument newValue)>();

            var allIds = originalValues.Keys.Union(currentValues.Keys);
            foreach (var id in allIds)
            {
                var isOriginal = originalValues.ContainsKey(id);
                var isCurrent = currentValues.ContainsKey(id);

                if (isOriginal && isCurrent)
                {
                    var original = originalValues[id];
                    var current = currentValues[id];
                    existingValues.Add((original, current));
                }
                else if (isOriginal)
                {
                    removedValues.Add(originalValues[id]);
                }
                else if (isCurrent)
                {
                    addedValues.Add(currentValues[id]);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            return (addedValues, removedValues, existingValues);
        }
    }
}
