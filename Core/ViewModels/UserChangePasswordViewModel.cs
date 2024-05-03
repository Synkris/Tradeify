using System.ComponentModel.DataAnnotations;

namespace Core.ViewModels
{
	public class UserChangePasswordViewModel
	{
		[Required]
		[DataType(DataType.Password)]
		public string OldPassword { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string NewPassword { get; set; }


		[DataType(DataType.Password)]
		[Required]
		[Compare("NewPassword", ErrorMessage = "Password and Confirm Password must be the same. ")]
		public string ConfirmNewPassword { get; set; }
	}
}
