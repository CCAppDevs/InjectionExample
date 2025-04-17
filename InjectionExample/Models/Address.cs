using Microsoft.AspNetCore.Identity;

namespace InjectionExample.Models
{
    public class Address
    {
        public int AddressId { get; set; }
        public string StreetNumber { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
    }
}
