using System;
using System.Collections.Generic;
using System.Threading;
using MongoDB.Bson.Serialization;

namespace MongoDelta.Mapping
{
    /// <summary>
    /// Contains extension methods to use whilst created a BsonClassMap
    /// </summary>
    public static class BsonClassMapExtensions
    {
        private static readonly ReaderWriterLockSlim ConfigLock = new ReaderWriterLockSlim();
        private static readonly Dictionary<Type, DeltaUpdateConfiguration> UpdateConfigurations =
            new Dictionary<Type, DeltaUpdateConfiguration>();

        /// <summary>
        /// When an item of this type is updated, mapped members should only be updated if they have changed
        /// </summary>
        /// <param name="classMap">The class map for the item type</param>
        /// <returns>The class map</returns>
        public static BsonClassMap UseDeltaUpdateStrategy(this BsonClassMap classMap)
        {
            ChangeConfigWithWriteLock(classMap, config => config.EnableDeltaUpdateStrategy());
            return classMap;
        }

        internal static bool ShouldUseDeltaUpdateStrategy(this BsonClassMap classMap)
        {
            return GetConfigValueWithReadLock(classMap, config => config.UseDeltaUpdateStrategy);
        }

        /// <summary>
        /// Specifies that the member should be updated incrementally. Only valid for members that serialize to the
        /// BSON types: Int32, Int64, Double or Decimal128
        /// </summary>
        /// <param name="memberMap">The map for the member to update incrementally</param>
        /// <returns>The member map</returns>
        public static BsonMemberMap UpdateIncrementally(this BsonMemberMap memberMap)
        {
            ChangeConfigWithWriteLock(memberMap.ClassMap,
                config => config.SetUpdateStrategyForElement(memberMap.ElementName, DeltaUpdateConfiguration.MemberUpdateStrategy.Incremental));
            return memberMap;
        }

        /// <summary>
        /// Updates the mapped member as if it were a hash set. The mapped member must serialize to a BsonArray. The equality is checked
        /// on the serialized BsonValue of each item in the BsonArray
        /// </summary>
        /// <param name="memberMap">The member map to update as a HashSet</param>
        /// <returns>The member map</returns>
        public static BsonMemberMap UpdateAsHashSet(this BsonMemberMap memberMap)
        {
            ChangeConfigWithWriteLock(memberMap.ClassMap,
                config => config.SetUpdateStrategyForElement(memberMap.ElementName, DeltaUpdateConfiguration.MemberUpdateStrategy.HashSet));
            return memberMap;
        }

        internal static DeltaUpdateConfiguration.MemberUpdateStrategy GetUpdateStrategy(this BsonClassMap classMap, string elementName)
        {
            return GetConfigValueWithReadLock(classMap,
                config => config.GetUpdateStrategyForElement(elementName));
        }

        private static DeltaUpdateConfiguration GetDeltaUpdateConfiguration(this BsonClassMap classMap)
        {
            ConfigLock.EnterReadLock();
            try
            {
                if (UpdateConfigurations.TryGetValue(classMap.ClassType, out var config))
                {
                    return config;
                }
            }
            finally
            {
                ConfigLock.ExitReadLock();
            }

            ConfigLock.EnterWriteLock();
            try
            {
                var config = new DeltaUpdateConfiguration();
                UpdateConfigurations.Add(classMap.ClassType, config);
                return config;
            }
            finally
            {
                ConfigLock.ExitWriteLock();
            }
        }

        private static void ChangeConfigWithWriteLock(BsonClassMap classMap, Action<DeltaUpdateConfiguration> action)
        {
            var config = GetDeltaUpdateConfiguration(classMap);

            ConfigLock.EnterWriteLock();
            try
            {
                action(config);
            }
            finally
            {
                ConfigLock.ExitWriteLock();
            }
        }

        private static T GetConfigValueWithReadLock<T>(BsonClassMap classMap,
            Func<DeltaUpdateConfiguration, T> expression)
        {
            var config = GetDeltaUpdateConfiguration(classMap);

            ConfigLock.EnterReadLock();
            try
            {
                return expression(config);
            }
            finally
            {
                ConfigLock.ExitReadLock();
            }
        }
    }
}
