using System.Threading.Tasks;
using MongoDelta.AspNetCore3.Example.Data.Models;

namespace MongoDelta.AspNetCore3.Example.Data
{
    public interface IUnitOfWork
    {
        MongoDeltaRepository<Person> People { get; }
        Task CommitAsync();
    }
}