using System.ComponentModel.DataAnnotations;

namespace agancywebProject.Models.DB
{
    public class loginadmin
    {
        [Required(ErrorMessage = Errormsg.RequairedMsg)]
        public string username { get; set; } = string.Empty;

        [Required(ErrorMessage = Errormsg.RequairedMsg)]
        [DataType(DataType.Password)]
        public string password { get; set; } = string.Empty;
    }
}
