using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDelta.AspNetCore3.Example.Data;
using MongoDelta.AspNetCore3.Example.Data.Models;
using MongoDelta.AspNetCore3.Example.Models;

namespace MongoDelta.AspNetCore3.Example.Controllers
{
    [Route("api/[controller]/{phoneBookId}/People")]
    [ApiController]
    public class PhoneBookController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public PhoneBookController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IEnumerable<PersonResponseDto>> GetAsync(Guid phoneBookId)
        {
            var people = await _unitOfWork.People.QueryAsync(person => person.PhoneBookId == phoneBookId);
            return people.Select(p => new PersonResponseDto()
            {
                Id = p.Id,
                Name = p.Name,
                PhoneNumber = p.PhoneNumber
            });
        }

        [HttpGet("{id}", Name = "Get")]
        public async Task<PersonResponseDto> Get(Guid phoneBookId, Guid id)
        {
            var person = await _unitOfWork.People.QuerySingleAsync(p => p.PhoneBookId == phoneBookId && p.Id == id);

            if (person == null)
            {
                HttpContext.Response.StatusCode = (int) HttpStatusCode.NotFound;
                return null;
            }

            return new PersonResponseDto()
            {
                Id = person.Id,
                Name = person.Name,
                PhoneNumber = person.PhoneNumber
            };
        }

        [HttpPost]
        public async Task<PersonResponseDto> Post(Guid phoneBookId, [FromBody] PersonRequestDto personRequest)
        {
            var person = new Person()
            {
                PhoneBookId = phoneBookId,
                Name = personRequest.Name,
                PhoneNumber = personRequest.PhoneNumber
            };

            _unitOfWork.People.Add(person);
            await _unitOfWork.CommitAsync();

            return new PersonResponseDto()
            {
                Id = person.Id,
                Name = person.Name,
                PhoneNumber = person.PhoneNumber
            };
        }

        [HttpPut("{id}")]
        public async Task Put(Guid phoneBookId, Guid id, [FromBody] PersonRequestDto personRequest)
        {
            var person = await _unitOfWork.People.QuerySingleAsync(p => p.PhoneBookId == phoneBookId && p.Id == id);

            if (person == null)
            {
                HttpContext.Response.StatusCode = (int) HttpStatusCode.NotFound;
                return;
            }

            person.Name = personRequest.Name;
            person.PhoneNumber = personRequest.PhoneNumber;

            await _unitOfWork.CommitAsync();
        }

        [HttpDelete("{id}")]
        public async Task Delete(Guid phoneBookId, Guid id)
        {
            var person = await _unitOfWork.People.QuerySingleAsync(p => p.PhoneBookId == phoneBookId && p.Id == id);

            if (person == null)
            {
                HttpContext.Response.StatusCode = (int) HttpStatusCode.NotFound;
                return;
            }

            _unitOfWork.People.Remove(person);

            await _unitOfWork.CommitAsync();
        }
    }
}
