using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Pishgaman_Project.ViewModels
{
    public class UserEditViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "وارد کردن نام الزامی است")]
        [Display(Name = "نام", Prompt = "نام خود را وارد کنید")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "وارد کردن نام خانوادگی الزامی است")]
        [Display(Name = "نام خانوادگی", Prompt = "نام خانوادگی خود را وارد کنید")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "وارد کردن شماره همراه الزامی است")]
        [Display(Name = "شماره همراه", Prompt = "شماره همراه خود را وارد کنید")]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره همراه باید 11 رقم و با پیش شماره 09 باشد")]
        [Remote("CheckDuplicatePhoneNumber", "User", AdditionalFields = "ID", ErrorMessage = "این شماره تلفن قبلا استفاده شده")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "ایمیل", Prompt = "(اختیاری) ایمیل خود را وارد کنید")]
        public string? Email { get; set; }
    }
}
