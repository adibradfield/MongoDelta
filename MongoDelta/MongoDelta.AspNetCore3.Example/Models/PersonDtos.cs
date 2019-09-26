using System;

namespace MongoDelta.AspNetCore3.Example.Models
{
    public class PersonRequestDto
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class PersonResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
    }
}
