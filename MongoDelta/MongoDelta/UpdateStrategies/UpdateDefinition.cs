using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDelta.UpdateStrategies
{
    class UpdateDefinition
    {
        public static UpdateDefinition Replace(object model, IArrayFilterNamingStrategy arrayFilterNamingStrategy) =>
            new UpdateDefinition(true, arrayFilterNamingStrategy, model);
        public static UpdateDefinition Delta(IArrayFilterNamingStrategy arrayFilterNamingStrategy) => new UpdateDefinition(false, arrayFilterNamingStrategy);

        private UpdateDefinition(bool alwaysReplace, IArrayFilterNamingStrategy arrayFilterNamingStrategy, object model = null)
        {
            _alwaysReplace = alwaysReplace;
            _model = model;
            _arrayFilterNamingStrategy = arrayFilterNamingStrategy;
        }

        private readonly bool _alwaysReplace;
        private readonly object _model;

        private readonly List<ElementUpdateDefinition> _elementUpdates = new List<ElementUpdateDefinition>();
        private readonly Dictionary<string, ArrayFilterDefinition> _arrayFilters = new Dictionary<string, ArrayFilterDefinition>();
        private readonly IArrayFilterNamingStrategy _arrayFilterNamingStrategy;

        private IReadOnlyCollection<SetUpdateDefinition> ElementsToReplace => Array.AsReadOnly(_elementUpdates.OfType<SetUpdateDefinition>().ToArray());
        private IReadOnlyCollection<IncrementUpdateDefinition> ElementsToIncrement => Array.AsReadOnly(_elementUpdates.OfType<IncrementUpdateDefinition>().ToArray());
        private IReadOnlyCollection<HashSetUpdateDefinition> HashSetUpdates => Array.AsReadOnly(_elementUpdates.OfType<HashSetUpdateDefinition>().ToArray());
        private IReadOnlyCollection<DeltaSetUpdateDefinition> DeltaSetUpdates => Array.AsReadOnly(_elementUpdates.OfType<DeltaSetUpdateDefinition>().ToArray());

        public bool HasChanges => (_alwaysReplace && _model != null) || _elementUpdates.Any();
        public IArrayFilterNamingStrategy ArrayFilterNamingStrategy => _arrayFilterNamingStrategy;

        public void Set(string elementName, BsonValue value)
        {
            Set(elementName, value, new string[0]);
        }

        private void Set(string elementName, BsonValue value, string[] arrayFilters)
        {
            if (ElementsToReplace.Any(e => e.ElementName == elementName))
            {
                throw new InvalidOperationException();
            }

            _elementUpdates.Add(new SetUpdateDefinition(elementName, value, arrayFilters));
        }

        public void Increment(string elementName, BsonValue incrementBy)
        {
            Increment(elementName, incrementBy, new string[0]);
        }

        private void Increment(string elementName, BsonValue incrementBy, string[] arrayFilters)
        {
            if (ElementsToIncrement.Any(e => e.ElementName == elementName))
            {
                throw new InvalidOperationException();
            }

            _elementUpdates.Add(new IncrementUpdateDefinition(elementName, incrementBy, arrayFilters));
        }

        public void UpdateHashSet(string elementName, BsonValue[] itemsToAdd, BsonValue[] itemsToRemove)
        {
            UpdateHashSet(elementName, itemsToAdd, itemsToRemove, new string[0]);
        }

        private void UpdateHashSet(string elementName, BsonValue[] itemsToAdd, BsonValue[] itemsToRemove, string[] arrayFilters)
        {
            if (HashSetUpdates.Any(e => e.ElementName == elementName))
            {
                throw new InvalidOperationException();
            }

            _elementUpdates.Add(new HashSetUpdateDefinition(elementName, itemsToAdd, itemsToRemove, arrayFilters));
        }

        public void AddAndRemoveFromDeltaSet(string elementName, IEnumerable<BsonDocument> addedValues,
            IEnumerable<BsonDocument> removedValues, string idElementName, Type itemType)
        {
            AddAndRemoveFromDeltaSet(elementName, addedValues, removedValues, idElementName, itemType, new string[0]);
        }

        private void AddAndRemoveFromDeltaSet(string elementName, IEnumerable<BsonDocument> addedValues,
            IEnumerable<BsonDocument> removedValues, string idElementName, Type itemType, string[] arrayFilters)
        {
            if (DeltaSetUpdates.Any(e => e.ElementName == elementName))
            {
                throw new InvalidOperationException();
            }

            _elementUpdates.Add(new DeltaSetUpdateDefinition(elementName, addedValues, removedValues, idElementName, itemType, arrayFilters));
        }

        public string CreateArrayFilter(Type itemType, string matchingElementName, BsonValue value)
        {
            var filterDefinitionType = typeof(BsonDocumentArrayFilterDefinition<>).MakeGenericType(itemType);
            var arrayFilterName = _arrayFilterNamingStrategy.GetNextName();
            var arrayFilterDocument = new BsonDocument() {
                {
                    $"{arrayFilterName}.{matchingElementName}", new BsonDocument{
                        {"$eq", value}
                    }}
            };
            _arrayFilters.Add(arrayFilterName, (ArrayFilterDefinition) Activator.CreateInstance(filterDefinitionType, arrayFilterDocument));

            return arrayFilterName;
        }

        public void Merge(string elementNamePrefix, UpdateDefinition updateDefinition, string[] arrayFiltersInPrefix = null)
        {
            if (arrayFiltersInPrefix == null)
            {
                arrayFiltersInPrefix = new string[0];
            }

            if (updateDefinition._alwaysReplace)
            {
                var modelBsonValue = updateDefinition._model.ToBsonDocument();
                Set(elementNamePrefix, modelBsonValue, arrayFiltersInPrefix);
                return;
            }

            foreach (var elementUpdate in updateDefinition.ElementsToReplace)
            {
                var newElementName = GetElementNameWithPrefix(elementNamePrefix, elementUpdate.ElementName);
                Set(newElementName, elementUpdate.NewValue, elementUpdate.ArrayFilters.Concat(arrayFiltersInPrefix).ToArray());
            }

            foreach (var elementIncrement in updateDefinition.ElementsToIncrement)
            {
                var newElementName = GetElementNameWithPrefix(elementNamePrefix, elementIncrement.ElementName);
                Increment(newElementName, elementIncrement.IncrementBy, elementIncrement.ArrayFilters.Concat(arrayFiltersInPrefix).ToArray());
            }

            foreach (var hashSetUpdate in updateDefinition.HashSetUpdates)
            {
                var newElementName = GetElementNameWithPrefix(elementNamePrefix, hashSetUpdate.ElementName);
                UpdateHashSet(newElementName, hashSetUpdate.ItemsToAdd, hashSetUpdate.ItemsToRemove, hashSetUpdate.ArrayFilters.Concat(arrayFiltersInPrefix).ToArray());
            }

            foreach (var deltaSetUpdate in updateDefinition.DeltaSetUpdates)
            {
                var newElementName = GetElementNameWithPrefix(elementNamePrefix, deltaSetUpdate.ElementName);
                AddAndRemoveFromDeltaSet(newElementName, deltaSetUpdate.AddedValues, deltaSetUpdate.RemovedValues,
                    deltaSetUpdate.ItemIdElementName, deltaSetUpdate.ItemType,
                    deltaSetUpdate.ArrayFilters.Concat(arrayFiltersInPrefix).ToArray());
            }

            foreach (var arrayFilter in updateDefinition._arrayFilters)
            {
                _arrayFilters.Add(arrayFilter.Key, arrayFilter.Value);
            }
        }

        private static string GetElementNameWithPrefix(string prefix, string originalName)
        {
            return prefix + "." + originalName;
        }

        public WriteModel<T>[] ToMongoWriteModels<T>(FilterDefinition<T> filter)
        {
            if (_alwaysReplace)
            {
                return new WriteModel<T>[]{ new ReplaceOneModel<T>(filter, (T)_model) };
            }

            var operationSplitter = new MongoUpdateOperationSplitter();
            ApplyOperationsToSplitter(operationSplitter);

            return operationSplitter.GetUpdateDefinitions().Select(d => new UpdateOneModel<T>(filter, d.ToUpdateDefinition())
            {
                ArrayFilters = _arrayFilters.Where(f => d.RequiredArrayFilters.Contains(f.Key)).Select(f => f.Value)
            }).ToArray<WriteModel<T>>();
        }

        private void ApplyOperationsToSplitter(MongoUpdateOperationSplitter operationSplitter)
        {
            foreach (var updateDefinition in ElementsToReplace)
            {
                operationSplitter.AddOperation("$set", new BsonElement(updateDefinition.ElementName, updateDefinition.NewValue), updateDefinition.ArrayFilters);
            }

            foreach (var updateDefinition in ElementsToIncrement)
            {
                operationSplitter.AddOperation("$inc", new BsonElement(updateDefinition.ElementName, updateDefinition.IncrementBy), updateDefinition.ArrayFilters);
            }

            foreach (var updateDefinition in HashSetUpdates)
            {
                if (updateDefinition.ItemsToAdd.Any())
                {
                    operationSplitter.AddOperation("$addToSet",
                        new BsonElement(updateDefinition.ElementName,
                            new BsonDocument("$each", new BsonArray(updateDefinition.ItemsToAdd))), updateDefinition.ArrayFilters);
                }

                if (updateDefinition.ItemsToRemove.Any())
                {
                    operationSplitter.AddOperation("$pull",
                        new BsonElement(updateDefinition.ElementName,
                            new BsonDocument("$in", new BsonArray(updateDefinition.ItemsToRemove))), updateDefinition.ArrayFilters);
                }
            }

            foreach (var updateDefinition in DeltaSetUpdates)
            {
                //To update existing items within a DeltaSet, callers should add an array filter, then merge in the changes using the
                // array filter name as the prefix
                if (updateDefinition.AddedValues.Any())
                {
                    operationSplitter.AddOperation("$push",
                        new BsonElement(updateDefinition.ElementName,
                            new BsonDocument("$each", new BsonArray(updateDefinition.AddedValues))), updateDefinition.ArrayFilters);
                }

                if (updateDefinition.RemovedValues.Any())
                {
                    operationSplitter.AddOperation("$pull",
                        new BsonElement(updateDefinition.ElementName,
                            new BsonDocument(updateDefinition.ItemIdElementName,
                                new BsonDocument("$in",
                                    new BsonArray(updateDefinition.RemovedValues.Select(v => v[updateDefinition.ItemIdElementName]))
                                )
                            )
                        ), updateDefinition.ArrayFilters
                    );
                }
            }
        }

        private abstract class ElementUpdateDefinition
        {
            public string ElementName { get; }
            public string[] ArrayFilters { get; }

            protected ElementUpdateDefinition(string elementName, string[] arrayFilters)
            {
                ElementName = elementName;
                ArrayFilters = arrayFilters;
            }
        }

        private class SetUpdateDefinition : ElementUpdateDefinition
        {
            public BsonValue NewValue { get; }

            public SetUpdateDefinition(string elementName, BsonValue newValue, string[] arrayFilters):base(elementName, arrayFilters)
            {
                NewValue = newValue;
            }
        }

        private class IncrementUpdateDefinition : ElementUpdateDefinition
        {
            public BsonValue IncrementBy { get; }

            public IncrementUpdateDefinition(string elementName, BsonValue incrementBy, string[] arrayFilters):base(elementName, arrayFilters)
            {
                IncrementBy = incrementBy;
            }
        }

        private class HashSetUpdateDefinition : ElementUpdateDefinition
        {
            public BsonValue[] ItemsToAdd { get; }
            public BsonValue[] ItemsToRemove { get; }

            public HashSetUpdateDefinition(string elementName, BsonValue[] itemsToAdd, BsonValue[] itemsToRemove,
                string[] arrayFilters):base(elementName, arrayFilters)
            {
                ItemsToAdd = itemsToAdd;
                ItemsToRemove = itemsToRemove;
            }
        }

        private class DeltaSetUpdateDefinition : ElementUpdateDefinition
        {
            public IEnumerable<BsonDocument> AddedValues { get; }
            public IEnumerable<BsonDocument> RemovedValues { get; }
            public string ItemIdElementName { get; }
            public Type ItemType { get; }

            public DeltaSetUpdateDefinition(string elementName, IEnumerable<BsonDocument> addedValues,
                IEnumerable<BsonDocument> removedValues, string itemIdElementName, Type itemType, string[] arrayFilters) : base(elementName, arrayFilters)
            {
                AddedValues = addedValues;
                RemovedValues = removedValues;
                ItemIdElementName = itemIdElementName;
                ItemType = itemType;
            }
        }
    }
}
