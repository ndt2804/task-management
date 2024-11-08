using System.ComponentModel.DataAnnotations;

namespace task_management.Dtos
{
    public class LoginModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
