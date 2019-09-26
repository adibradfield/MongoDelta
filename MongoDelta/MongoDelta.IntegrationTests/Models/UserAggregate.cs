using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NUnit.Framework;

namespace MongoDelta.IntegrationTests.Models
{
    class UserAggregate
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }

        public static async Task<UserAggregate> AddTestUser(IMongoDatabase database, string collectionName)
        {
            var unitOfWork = new UserUnitOfWork(database, collectionName);
            unitOfWork.Users.Add(new UserAggregate()
            {
                FirstName = "John",
                Surname = "Smith"
            });
            await unitOfWork.CommitAsync();

            var queryResult = await unitOfWork.Users.QuerySingleAsync(query => query.Where(user => user.FirstName == "John"));
            Assert.IsNotNull(queryResult);
            return queryResult;
        }
    }

    class UserUnitOfWork : UnitOfWorkBase
    {
        public UserUnitOfWork(IMongoDatabase database, string collectionName = null) : base(database, false)
        {
            collectionName ??= Guid.NewGuid().ToString();
            RegisterRepository<UserAggregate>(collectionName);
        }
        public MongoDeltaRepository<UserAggregate> Users => GetRepository<UserAggregate>();
    }
}
