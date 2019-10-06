using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDelta.UpdateStrategies
{
    class DeltaUpdateDefinition
    {
        private readonly Dictionary<string, ElementUpdate> _elementsToReplace = new Dictionary<string, ElementUpdate>();
        public IReadOnlyCollection<ElementUpdate> ElementsToReplace => Array.AsReadOnly(_elementsToReplace.Values.ToArray());

        public void Set(string elementName, BsonValue value)
        {
            _elementsToReplace.Add(elementName, new ElementUpdate(elementName, value));
        }

        public void Merge(string elementNamePrefix, DeltaUpdateDefinition deltaUpdateDefinition)
        {
            foreach (var elementUpdate in deltaUpdateDefinition.ElementsToReplace)
            {
                var newElementName = elementNamePrefix + "." + elementUpdate.ElementName;
                Set(newElementName, elementUpdate.NewValue);
            }
        }

        public BsonDocumentUpdateDefinition<T> ToMongoUpdateDefinition<T>()
        {
            var replaceUpdateDefinitions =
                new BsonDocument(ElementsToReplace.Select(e => new BsonElement(e.ElementName, e.NewValue)));
            return new BsonDocumentUpdateDefinition<T>(new BsonDocument("$set", replaceUpdateDefinitions));
        }

        public class ElementUpdate
        {
            public string ElementName { get; private set; }
            public BsonValue NewValue { get; private set; }

            public ElementUpdate(string elementName, BsonValue newValue)
            {
                ElementName = elementName;
                NewValue = newValue;
            }
        }
    }
}
