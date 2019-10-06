using System;
using System.Collections.Generic;
using System.Threading;
using MongoDB.Bson.Serialization;

namespace MongoDelta.Mapping
{
    public static class BsonClassMapExtensions
    {
        private static readonly ReaderWriterLockSlim ConfigLock = new ReaderWriterLockSlim();
        private static readonly Dictionary<Type, DeltaUpdateConfiguration> UpdateConfigurations =
            new Dictionary<Type, DeltaUpdateConfiguration>();

        public static void UseDeltaUpdateStrategy(this BsonClassMap classMap)
        {
            ConfigLock.EnterWriteLock();
            try
            {
                UpdateConfigurations.Add(classMap.ClassType, new DeltaUpdateConfiguration {UseDeltaUpdateStrategy = true});
            }
            finally
            {
                ConfigLock.ExitWriteLock();
            }
        }

        internal static DeltaUpdateConfiguration GetDeltaUpdateConfiguration(this BsonClassMap classMap)
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
                var config = new DeltaUpdateConfiguration {UseDeltaUpdateStrategy = false};
                UpdateConfigurations.Add(classMap.ClassType, config);
                return config;
            }
            finally
            {
                ConfigLock.ExitWriteLock();
            }
        }
    }
}
