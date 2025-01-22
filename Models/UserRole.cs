using System.ComponentModel.DataAnnotations;

namespace mvccore_dotnet_app.Models
{
    public class UserRole
    {
        [Key]
        public int id { get; set; }
        [Required(ErrorMessage = "Enter the  Name")]
        public string UserName { get; set; }
        public string Email { get; set; }
        [Display(Name = "Mobile No")]
        public long Phone { get; set; }
        public string Role { get; set; }
        [DataType(DataType.Date)]
        public string DateOfBirth { get; set; }

        public string Native { get; set; }

        public string Pincode { get; set; }

        public string Password { get; set; }

        public string Cpassword { get; set; }

        public string? Rank { get; set; }
    }
}
