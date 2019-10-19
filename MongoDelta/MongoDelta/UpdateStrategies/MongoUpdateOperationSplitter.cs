using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace MongoDelta.UpdateStrategies
{
    class MongoUpdateOperationSplitter
    {
        private readonly List<SplitUpdateOperation> _writeOperationSplit = new List<SplitUpdateOperation>();

        public void AddOperation(string operation, BsonElement elementUpdateDefinition)
        {
            var nextSplit =
                _writeOperationSplit.FirstOrDefault(s => !s.ContainsConflictingEntryForElement(elementUpdateDefinition));

            if (nextSplit == null)
            {
                nextSplit = new SplitUpdateOperation();
                _writeOperationSplit.Add(nextSplit);
            }

            nextSplit.AddOperation(operation, elementUpdateDefinition);
        }

        public BsonDocument[] GetUpdateDefinitions()
        {
            return _writeOperationSplit.Select(s => s.ToUpdateDefinition()).ToArray();
        }

        private class SplitUpdateOperation : Dictionary<string, List<BsonElement>>
        {
            public bool ContainsConflictingEntryForElement(BsonElement element)
            {
                return this.SelectMany(x => x.Value.Select(y => y.Name)).Contains(element.Name);
            }

            public void AddOperation(string operation, BsonElement elementUpdateDefinition)
            {
                if (this.TryGetValue(operation, out var updateDefinitionList))
                {
                    updateDefinitionList.Add(elementUpdateDefinition);
                }
                else
                {
                    this.Add(operation, new List<BsonElement>{ elementUpdateDefinition });
                }
            }

            public BsonDocument ToUpdateDefinition()
            {
                return new BsonDocument(this.Select(kvp => new BsonElement(kvp.Key, new BsonDocument(kvp.Value))));
            }
        }
    }
}
