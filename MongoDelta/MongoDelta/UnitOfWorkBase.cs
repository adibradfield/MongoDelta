using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MongoDelta
{
    public abstract class UnitOfWorkBase
    {
        private readonly IMongoDatabase _database;
        private readonly bool _useTransactions;
        private readonly Dictionary<Type, MongoDeltaRepository> _repositories = new Dictionary<Type, MongoDeltaRepository>();
        private bool _hasCommitted;

        protected UnitOfWorkBase(IMongoDatabase database, bool useTransactions = true)
        {
            _database = database;
            _useTransactions = useTransactions;
        }

        protected void RegisterRepository<TAggregate>(string collectionName) where TAggregate : class
        {
            var repository = new MongoDeltaRepository<TAggregate>(_database.GetCollection<TAggregate>(collectionName));
            _repositories.Add(typeof(TAggregate), repository);
        }

        protected MongoDeltaRepository<TAggregate> GetRepository<TAggregate>() where TAggregate : class
        {
            return (MongoDeltaRepository<TAggregate>) _repositories[typeof(TAggregate)];
        }
        public async Task CommitAsync()
        {
            if (_hasCommitted)
            {
                throw new InvalidOperationException("A UnitOfWork can only be committed a single time");
            }

            using (var session = await _database.Client.StartSessionAsync())
            {
                if(_useTransactions) session.StartTransaction();
                try
                {
                    foreach (var repository in _repositories.Values)
                    {
                        await repository.CommitChangesAsync(session);
                    }

                    if(_useTransactions) await session.CommitTransactionAsync();
                    _hasCommitted = true;
                }
                catch(Exception )
                {
                    if(_useTransactions) await session.AbortTransactionAsync();
                    throw;
                }
            }
        }
    }
}
