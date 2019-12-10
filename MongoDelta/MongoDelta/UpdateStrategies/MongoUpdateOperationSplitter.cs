using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;

namespace MongoDelta.UpdateStrategies
{
    class MongoUpdateOperationSplitter
    {
        private readonly List<SplitUpdateOperation> _writeOperationSplit = new List<SplitUpdateOperation>();

        public void AddOperation(string operation, BsonElement elementUpdateDefinition, string[] arrayFilters)
        {
            var nextSplit =
                _writeOperationSplit.FirstOrDefault(s => !s.ContainsConflictingEntryForElement(elementUpdateDefinition));

            if (nextSplit == null)
            {
                nextSplit = new SplitUpdateOperation();
                _writeOperationSplit.Add(nextSplit);
            }

            nextSplit.AddOperation(operation, elementUpdateDefinition, arrayFilters);
        }

        public SplitUpdateOperation[] GetUpdateDefinitions()
        {
            return _writeOperationSplit.ToArray();
        }

        public class SplitUpdateOperation : Dictionary<string, List<BsonElement>>
        {
            public HashSet<string> RequiredArrayFilters { get; } = new HashSet<string>();

            public bool ContainsConflictingEntryForElement(BsonElement element)
            {
                return this.SelectMany(x => x.Value.Select(y => y.Name))
                    .Any(name => name == element.Name || name.StartsWith($"{element.Name}.") || element.Name.StartsWith($"{name}."));
            }

            public void AddOperation(string operation, BsonElement elementUpdateDefinition, string[] arrayFilters)
            {
                if (this.TryGetValue(operation, out var updateDefinitionList))
                {
                    updateDefinitionList.Add(elementUpdateDefinition);
                }
                else
                {
                    this.Add(operation, new List<BsonElement>{ elementUpdateDefinition });
                }

                foreach (var arrayFilter in arrayFilters)
                {
                    RequiredArrayFilters.Add(arrayFilter);
                }
            }

            public BsonDocument ToUpdateDefinition()
            {
                return new BsonDocument(this.Select(kvp => new BsonElement(kvp.Key, new BsonDocument(kvp.Value))));
            }
        }
    }
}
