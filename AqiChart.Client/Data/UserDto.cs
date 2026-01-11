using Caliburn.Micro;

namespace AqiChart.Client.Data
{
    public class UserDto: PropertyChangedBase
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AvatarUrl { get; set; }
        public string Status { get; set; }

        private string lastMessage;
        public string LastMessage {
            get => lastMessage;
            set
            {
                lastMessage = value;
                NotifyOfPropertyChange(() => LastMessage);
            }
        }

        private int chartCount = 0;
        public int ChartCount { 
            get=> chartCount;
            set { 
                chartCount = value;
                NotifyOfPropertyChange(() => ChartCount);
            }
        }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; NotifyOfPropertyChange(); }
        }
    }
}
