

namespace AqiChart.Model.Dto
{
    public class UserDto
    {
        public string Token { get; set; }
        public string Id { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AvatarUrl { get; set; }
        public string Status { get; set; }
    }

    public enum UserStatus
    {
        online,
        offline,
        away
    }

}
