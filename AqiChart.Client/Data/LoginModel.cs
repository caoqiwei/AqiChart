using AqiChart.Client.Common;

namespace AqiChart.Client.Data
{
    public class LoginModel : NotifyBase
    {
        private string _account;
        public string Account
        {
            get { return _account; }
            set
            {
                _account = value;
                this.DoNotify();
            }
        }

        private string _password;

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                this.DoNotify();
            }
        }

        private string _validationCode;

        public string ValidationCode
        {
            get { return _validationCode; }
            set
            {
                _validationCode = value;
                this.DoNotify();
            }
        }
    }
}
