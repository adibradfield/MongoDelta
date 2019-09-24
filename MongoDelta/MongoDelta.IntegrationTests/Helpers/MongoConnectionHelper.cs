using System;

namespace MongoDelta.IntegrationTests.Helpers
{
    static class MongoConnectionHelper
    {
        public static string GetConnectionString()
        {
            #if CI
                return Environment.GetEnvironmentVariable("MONGO_DELTA_CONNECTION_STRING");
            #else
                return "mongodb://localhost:27017/?retryWrites=false";
            #endif
        }
    }
}
