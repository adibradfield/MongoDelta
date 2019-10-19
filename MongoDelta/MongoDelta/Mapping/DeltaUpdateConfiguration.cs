using System.Collections.Generic;

namespace MongoDelta.Mapping
{
    internal class DeltaUpdateConfiguration
    {
        internal enum MemberUpdateStrategy
        {
            Normal,
            Incremental,
            HashSet
        }

        private readonly Dictionary<string, MemberUpdateStrategy> _memberUpdateStrategies = new Dictionary<string,MemberUpdateStrategy>();

        public bool UseDeltaUpdateStrategy { get; private set; }

        internal void EnableDeltaUpdateStrategy() => UseDeltaUpdateStrategy = true;
        internal void SetUpdateStrategyForElement(string elementName, MemberUpdateStrategy updateStrategy) => _memberUpdateStrategies[elementName] = updateStrategy;

        internal MemberUpdateStrategy GetUpdateStrategyForElement(string elementName)
        {
            return _memberUpdateStrategies.TryGetValue(elementName, out var updateStrategy) ? updateStrategy : MemberUpdateStrategy.Normal;
        }
    }
}
