using MongoDB.Driver;
using MongoDelta.AspNetCore3.Example.Data.Models;

namespace MongoDelta.AspNetCore3.Example.Data
{
    public class UnitOfWork : UnitOfWorkBase, IUnitOfWork
    {
        public UnitOfWork(IMongoDatabase database) : base(database, useTransactions: false)
        {
            RegisterRepository<Person>("people");
        }

        public MongoDeltaRepository<Person> People => GetRepository<Person>();
    }
}
