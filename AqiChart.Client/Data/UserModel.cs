using AqiChart.Client.Common;

namespace AqiChart.Client.Data
{
    public class UserModel : NotifyBase
    {
        public string Token {  get; set; }

        public string Id { get; set; }

        private string _avatarUrl;
        public string AvatarUrl
        {
            get { return _avatarUrl; }
            set { _avatarUrl = value; this.DoNotify(); }
        }

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set { _userName = value; this.DoNotify(); }
        }

        private string _nickName;
        public string NickName
        {
            get { return _nickName; }
            set { _nickName = value; this.DoNotify(); }
        }

        private string _email;
        public string Email
        {
            get { return _email; }
            set { _email = value; this.DoNotify(); }
        }

    }
}
