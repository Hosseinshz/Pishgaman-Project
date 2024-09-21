using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Pishgaman_Project.ViewModels
{
    public class SignInViewModel
    {
        [Required(ErrorMessage = "نام کاربری (شماره همراه) را وارد کنید")]
        [Display(Name = "نام کاربری", Prompt = "نام کاربری (شماره همراه) خود را وارد کنید")]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = "نام کاربری (شماره همراه) باید 11 رقم و با پیش شماره 09 باشد")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "رمز عبور را وارد کنید")]
        [Display(Name = "رمز عبور", Prompt = "رمز عبور خود را وارد کنید")]
        public string Password { get; set; }
    }
}
