using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using MongoDelta.AspNetCore3.Example.Models;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MongoDelta.AspNetCore3.Example.IntegrationTests
{
    [TestFixture]
    public class PhoneBookTests
    {
        private WebApplicationFactory<Startup> _factory;

        [OneTimeSetUp]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Startup>();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _factory.Dispose();
        }

        [Test]
        public async Task TestCrudMethods_Success()
        {
            var phoneBookId = Guid.NewGuid();
            var client = _factory.CreateClient();

            //Create person
            var createPersonResponse = await client.PostAsync(GetUri(phoneBookId),
                CreatePersonRequest("John Smith", "07777777777"));
            Assert.IsTrue(createPersonResponse.IsSuccessStatusCode, "Failed to create a person");
            var createdPerson = await ReadPersonResponse(createPersonResponse);

            //Update person
            var updatePersonResponse = await client.PutAsync(GetUri(phoneBookId, createdPerson.Id),
                CreatePersonRequest("Jane Doe", "07777777777"));
            Assert.IsTrue(updatePersonResponse.IsSuccessStatusCode, "Failed to update a person");

            //Get person
            var getPersonResponse = await client.GetAsync(GetUri(phoneBookId, createdPerson.Id));
            Assert.IsTrue(getPersonResponse.IsSuccessStatusCode, "Failed to get a person");
            var gotPerson = await ReadPersonResponse(getPersonResponse);
            Assert.AreEqual("Jane Doe", gotPerson.Name);

            //Delete person
            var deletePersonResponse = await client.DeleteAsync(GetUri(phoneBookId, createdPerson.Id));
            Assert.IsTrue(deletePersonResponse.IsSuccessStatusCode, "Failed to delete a person");

            //Get deleted person
            var getDeletedPersonResponse = await client.GetAsync(GetUri(phoneBookId, createdPerson.Id));
            Assert.AreEqual(HttpStatusCode.NotFound, getDeletedPersonResponse.StatusCode, "Getting a deleted person does not return NotFound");
        }

        private Uri GetUri(Guid phoneBookId, Guid? personId = null)
        {
            var uriString = $"api/PhoneBook/{phoneBookId:N}/People";
            if (personId != null)
            {
                uriString += $"/{personId:N}";
            }
            return new Uri(uriString, UriKind.Relative);
        }

        private HttpContent CreatePersonRequest(string name, string phoneNumber)
        {
            var person = new PersonRequestDto
            {
                Name = name,
                PhoneNumber = phoneNumber
            };
            return new StringContent(JsonConvert.SerializeObject(person), Encoding.UTF8, "application/json");
        }

        private async Task<PersonResponseDto> ReadPersonResponse(HttpResponseMessage response)
        {
            return JsonConvert.DeserializeObject<PersonResponseDto>(await response.Content.ReadAsStringAsync());
        }
    }
}