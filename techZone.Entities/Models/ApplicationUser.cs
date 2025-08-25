namespace techZone.Entities.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage ="What's your name?")]
        public string Name { get; set; } 
        public string City { get; set; } 
        public string Address { get; set; } 
        public string Phone { get; set; }
    }
}
