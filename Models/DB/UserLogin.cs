using System.ComponentModel.DataAnnotations;
using KeyAttribute = System.ComponentModel.DataAnnotations.KeyAttribute;
namespace agancywebProject.Models.DB
{
    public class UserLogin
    {
        [Key]
        public int User_Id { get; set; }

        [Required]
        public string username { get; set; } = string.Empty;

        [Required]
        public string password { get; set; } = string.Empty;
    }
}
