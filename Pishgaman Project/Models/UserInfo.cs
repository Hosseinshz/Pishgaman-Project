namespace Pishgaman_Project.Models
{
    public class UserInfo
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string Password { get; set; }
        public DateTime CreateDate { get; set; }
        public int? CreatedByUserID { get; set; } 

    }
}
