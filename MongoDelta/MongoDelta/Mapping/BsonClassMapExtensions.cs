using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MongoDB.Bson.Serialization;

namespace MongoDelta.Mapping
{
    public static class BsonClassMapExtensions
    {
        private static readonly ReaderWriterLockSlim ConfigLock = new ReaderWriterLockSlim();
        private static readonly Dictionary<Type, DeltaUpdateConfiguration> UpdateConfigurations =
            new Dictionary<Type, DeltaUpdateConfiguration>();

        public static BsonClassMap UseDeltaUpdateStrategy(this BsonClassMap classMap)
        {
            ChangeConfigWithWriteLock(classMap, config => config.EnableDeltaUpdateStrategy());
            return classMap;
        }

        internal static bool ShouldUseDeltaUpdateStrategy(this BsonClassMap classMap)
        {
            return GetConfigValueWithReadLock(classMap, config => config.UseDeltaUpdateStrategy);
        }

        public static BsonMemberMap UpdateIncrementally(this BsonMemberMap memberMap)
        {
            ChangeConfigWithWriteLock(memberMap.ClassMap,
                config => config.EnableIncrementalUpdateForElement(memberMap.ElementName));
            return memberMap;
        }

        internal static bool ShouldUpdateIncrementally(this BsonClassMap classMap, string elementName)
        {
            return GetConfigValueWithReadLock(classMap,
                config => config.ElementsToIncrementallyUpdate.Contains(elementName));
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
