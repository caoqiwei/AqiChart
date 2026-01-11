using AqiChart.Client.Services;
using AqiChart.Model.Dto;
using Caliburn.Micro;
using System.Collections.ObjectModel;
using System.Windows;
using UserDto = AqiChart.Client.Data.UserDto;


namespace AqiChart.Client.Models.AddressBook
{
    public class AddFriendViewModel : Screen, IShell
    {
        private readonly IWindowManager _windowManager;
        public AddFriendViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        private ObservableCollection<UserDto> _friendList = new ObservableCollection<UserDto>();
        public ObservableCollection<UserDto> FriendList
        {
            get => _friendList;
            set
            {
                _friendList = value;
                NotifyOfPropertyChange(() => FriendList);
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                NotifyOfPropertyChange(() => SearchText);
            }
        }
        public async Task SearchFriend()
        {
            List<UserDto> list = await ApiService.SearchUserList(new SearchDto() { Search = SearchText });
            FriendList = new ObservableCollection<UserDto>(list);
        }

        public void CloseView()
        {
            this.TryCloseAsync();
        }

        public async Task AddFriend(UserDto user) 
        {
            var mes = MessageBox.Show($"添加 {user.NickName} 为好友?", "添加好友", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if( mes == MessageBoxResult.Yes)
            {
                await ApiService.AddFriend(user.Id);

                FriendList.Remove(user);
            }
        }


    }
}
