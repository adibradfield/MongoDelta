using System;
using MongoDB.Driver;
using MongoDelta.Mapping;

namespace MongoDelta.Benchmarking.Models
{
    class UserModel
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = "Jane Doe";
        public Email EmailAddress { get; set; } = new Email();


        public class Email
        {
            public string EmailAddress { get; set; } = "jane.doe@example.com";
            public bool Verified { get; set; } = true;
        }
    }

    [UseDeltaUpdateStrategy]
    class DeltaUserModel
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = "Jane Doe";
        public Email EmailAddress { get; set; } = new Email();

        [UseDeltaUpdateStrategy]
        public class Email
        {
            public string EmailAddress { get; set; } = "jane.doe@example.com";
            public bool Verified { get; set; } = true;
        }
    }

    class UserUnitOfWork : UnitOfWorkBase
    {
        public UserUnitOfWork(IMongoDatabase database, string collectionName) : base(database, false)
        {
            RegisterRepository<UserModel>(collectionName);
            RegisterRepository<DeltaUserModel>(collectionName);
        }

        public MongoDeltaRepository<UserModel> Users => GetRepository<UserModel>();
        public MongoDeltaRepository<DeltaUserModel> DeltaUsers => GetRepository<DeltaUserModel>();
    }
}
