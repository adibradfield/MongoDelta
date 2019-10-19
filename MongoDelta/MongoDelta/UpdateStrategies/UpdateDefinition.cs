using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDelta.UpdateStrategies
{
    class UpdateDefinition
    {
        public static UpdateDefinition Replace(object model) => new UpdateDefinition(true, model);
        public static UpdateDefinition Delta => new UpdateDefinition(false);

        private UpdateDefinition(bool alwaysReplace, object model = null)
        {
            _alwaysReplace = alwaysReplace;
            _model = model;
        }

        private readonly bool _alwaysReplace;
        private readonly object _model;

        private readonly List<ElementUpdateDefinition> _elementUpdates = new List<ElementUpdateDefinition>();

        private IReadOnlyCollection<SetUpdateDefinition> ElementsToReplace => Array.AsReadOnly(_elementUpdates.OfType<SetUpdateDefinition>().ToArray());
        private IReadOnlyCollection<IncrementUpdateDefinition> ElementsToIncrement => Array.AsReadOnly(_elementUpdates.OfType<IncrementUpdateDefinition>().ToArray());
        private IReadOnlyCollection<HashSetUpdateDefinition> HashSetUpdates => Array.AsReadOnly(_elementUpdates.OfType<HashSetUpdateDefinition>().ToArray());

        public bool HasChanges => _alwaysReplace && _model != null? true : _elementUpdates.Any();

        public void Set(string elementName, BsonValue value)
        {
            if (ElementsToReplace.Any(e => e.ElementName == elementName))
            {
                throw new InvalidOperationException();
            }

            _elementUpdates.Add(new SetUpdateDefinition(elementName, value));
        }

        public void Increment(string elementName, BsonValue incrementBy)
        {
            if (ElementsToIncrement.Any(e => e.ElementName == elementName))
            {
                throw new InvalidOperationException();
            }

            _elementUpdates.Add(new IncrementUpdateDefinition(elementName, incrementBy));
        }

        public void UpdateHashSet(string elementName, BsonValue[] itemsToAdd, BsonValue[] itemsToRemove)
        {
            if (HashSetUpdates.Any(e => e.ElementName == elementName))
            {
                throw new InvalidOperationException();
            }

            _elementUpdates.Add(new HashSetUpdateDefinition(elementName, itemsToAdd, itemsToRemove));
        }

        public void Merge(string elementNamePrefix, UpdateDefinition updateDefinition)
        {
            foreach (var elementUpdate in updateDefinition.ElementsToReplace)
            {
                var newElementName = GetElementNameWithPrefix(elementNamePrefix, elementUpdate.ElementName);
                Set(newElementName, elementUpdate.NewValue);
            }

            foreach (var elementIncrement in updateDefinition.ElementsToIncrement)
            {
                var newElementName = GetElementNameWithPrefix(elementNamePrefix, elementIncrement.ElementName);
                Increment(newElementName, elementIncrement.IncrementBy);
            }

            foreach (var hashSetUpdate in updateDefinition.HashSetUpdates)
            {
                var newElementName = GetElementNameWithPrefix(elementNamePrefix, hashSetUpdate.ElementName);
                UpdateHashSet(newElementName, hashSetUpdate.ItemsToAdd, hashSetUpdate.ItemsToRemove);
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

            foreach (var updateDefinition in ElementsToReplace)
            {
                operationSplitter.AddOperation("$set", new BsonElement(updateDefinition.ElementName, updateDefinition.NewValue));
            }

            foreach (var updateDefinition in ElementsToIncrement)
            {
                operationSplitter.AddOperation("$inc", new BsonElement(updateDefinition.ElementName, updateDefinition.IncrementBy));
            }

            foreach (var updateDefinition in HashSetUpdates)
            {
                if (updateDefinition.ItemsToAdd.Any())
                {
                    operationSplitter.AddOperation("$addToSet",
                        new BsonElement(updateDefinition.ElementName,
                            new BsonDocument("$each", new BsonArray(updateDefinition.ItemsToAdd))));
                }

                if (updateDefinition.ItemsToRemove.Any())
                {
                    operationSplitter.AddOperation("$pull",
                        new BsonElement(updateDefinition.ElementName,
                            new BsonDocument("$in", new BsonArray(updateDefinition.ItemsToRemove))));
                }
            }

            return operationSplitter.GetUpdateDefinitions().Select(d => new UpdateOneModel<T>(filter, d)).ToArray<WriteModel<T>>();
        }

        private abstract class ElementUpdateDefinition
        {
            public string ElementName { get; }

            protected ElementUpdateDefinition(string elementName)
            {
                ElementName = elementName;
            }
        }

        private class SetUpdateDefinition : ElementUpdateDefinition
        {
            public BsonValue NewValue { get; }

            public SetUpdateDefinition(string elementName, BsonValue newValue):base(elementName)
            {
                NewValue = newValue;
            }
        }

        private class IncrementUpdateDefinition : ElementUpdateDefinition
        {
            public BsonValue IncrementBy { get; }

            public IncrementUpdateDefinition(string elementName, BsonValue incrementBy):base(elementName)
            {
                IncrementBy = incrementBy;
            }
        }

        private class HashSetUpdateDefinition : ElementUpdateDefinition
        {
            public BsonValue[] ItemsToAdd { get; }
            public BsonValue[] ItemsToRemove { get; }

            public HashSetUpdateDefinition(string elementName, BsonValue[] itemsToAdd, BsonValue[] itemsToRemove):base(elementName)
            {
                ItemsToAdd = itemsToAdd;
                ItemsToRemove = itemsToRemove;
            }
        }
    }
}
