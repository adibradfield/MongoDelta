using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDelta.UpdateStrategies;

namespace MongoDelta
{
    /// <summary>
    /// An abstract base class for a UnitOfWork
    /// </summary>
    public abstract class UnitOfWorkBase
    {
        private readonly IMongoDatabase _database;
        private readonly bool _useTransactions;
        private readonly Dictionary<Type, MongoDeltaRepository> _repositories = new Dictionary<Type, MongoDeltaRepository>();
        private bool _hasCommitted;

        /// <summary>
        /// Constructor for the base class
        /// </summary>
        /// <param name="database">An instance of the MongoDatabase to connect to</param>
        /// <param name="useTransactions">Set to true to use multi-document transactions</param>
        protected UnitOfWorkBase(IMongoDatabase database, bool useTransactions = true)
        {
            _database = database;
            _useTransactions = useTransactions;
        }

        /// <summary>
        /// Creates a repository and links it to a collection
        /// </summary>
        /// <param name="collectionName">The name of the collection to link to</param>
        /// <typeparam name="TAggregate">The type of model to use for the collection</typeparam>
        protected void RegisterRepository<TAggregate>(string collectionName) where TAggregate : class
        {
            var repository = new MongoDeltaRepository<TAggregate>(_database.GetCollection<TAggregate>(collectionName));
            _repositories.Add(typeof(TAggregate), repository);
        }

        /// <summary>
        /// Gets a repository that has previously been registered
        /// </summary>
        /// <typeparam name="TAggregate">The type of model to get a repository for</typeparam>
        /// <returns>The repository</returns>
        protected MongoDeltaRepository<TAggregate> GetRepository<TAggregate>() where TAggregate : class
        {
            return (MongoDeltaRepository<TAggregate>) _repositories[typeof(TAggregate)];
        }

        /// <summary>
        /// Commits changes on all repositories that have been registered
        /// </summary>
        /// <returns>void</returns>
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
                    var preparedChanges = new Dictionary<MongoDeltaRepository, PreparedWriteModel>();

                    foreach (var repository in _repositories.Values)
                    {
                        preparedChanges.Add(repository, repository.PrepareChangesForWrite());
                    }

                    foreach (var repository in _repositories.Values)
                    {
                        await repository.CommitChangesAsync(session, preparedChanges[repository]);
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
