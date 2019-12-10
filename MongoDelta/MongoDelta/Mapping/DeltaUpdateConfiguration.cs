using System;
using System.Collections.Generic;

namespace MongoDelta.Mapping
{
    internal class DeltaUpdateConfiguration
    {
        internal enum MemberUpdateStrategyType
        {
            Normal,
            Incremental,
            HashSet,
            DeltaSet
        }

        private readonly Dictionary<string, MemberUpdateStrategy> _memberUpdateStrategies = new Dictionary<string,MemberUpdateStrategy>();

        public bool UseDeltaUpdateStrategy { get; private set; }

        internal void EnableDeltaUpdateStrategy() => UseDeltaUpdateStrategy = true;
        internal void SetUpdateStrategyForElement(string elementName, MemberUpdateStrategyType updateStrategyType, Type collectionItemType = null)
        {
            _memberUpdateStrategies[elementName] = new MemberUpdateStrategy(updateStrategyType, collectionItemType);
        }

        internal MemberUpdateStrategy GetUpdateStrategyForElement(string elementName)
        {
            return _memberUpdateStrategies.TryGetValue(elementName, out var updateStrategy) ? updateStrategy : new MemberUpdateStrategy(MemberUpdateStrategyType.Normal, null);
        }

        internal class MemberUpdateStrategy
        {
            public MemberUpdateStrategyType Type { get; }
            public Type CollectionItemType { get; }

            public MemberUpdateStrategy(MemberUpdateStrategyType strategyType, Type collectionItemType)
            {
                Type = strategyType;
                CollectionItemType = collectionItemType;
            }
        }
    }
}
