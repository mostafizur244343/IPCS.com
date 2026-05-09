using System.ComponentModel.DataAnnotations;

namespace IPCS_Model.DTOs
{
    /// <summary>
    /// User login input model (DTO).
    /// Allows login using either Email or Mobile Number.
    /// </summary>
    public class LoginModel
    {
        [Required(ErrorMessage = "Email or Mobile Number is Required")]
        public string EmailOrMobile { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// If true, the token/session should be persisted on the client side.
        /// </summary>
        public bool RememberMe { get; set; }
    }
}
