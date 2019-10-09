using System.Collections.Generic;
using System.Linq;

namespace MongoDelta.Mapping
{
    internal class DeltaUpdateConfiguration
    {
        public bool UseDeltaUpdateStrategy { get; private set; }

        private readonly HashSet<string> _incrementalUpdateElements = new HashSet<string>();
        public IEnumerable<string> ElementsToIncrementallyUpdate => _incrementalUpdateElements.ToList().AsReadOnly();

        internal void EnableDeltaUpdateStrategy() => UseDeltaUpdateStrategy = true;
        internal void EnableIncrementalUpdateForElement(string elementName) => _incrementalUpdateElements.Add(elementName);
    }
}
