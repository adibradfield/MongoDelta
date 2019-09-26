using System;

namespace MongoDelta.AspNetCore3.Example.Data.Models
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public Guid PhoneBookId { get; set; }
    }
}
