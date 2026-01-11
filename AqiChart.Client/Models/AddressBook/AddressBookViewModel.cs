using AqiChart.Client.Data;
using AqiChart.Client.HttpClient;
using AqiChart.Client.Services;
using Caliburn.Micro;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AqiChart.Client.Models.AddressBook
{
    public class AddressBookViewModel : Screen, IChildViewModel
    {
        public string PageName { get; set; } = "AddressBook";

        public AddressBookViewModel()
        {
            GetData();
        }

        private ObservableCollection<UserGroup> _userGroups = new ObservableCollection<UserGroup>();
        public ObservableCollection<UserGroup> UserGroups
        {
            get => _userGroups;
            set { _userGroups = value; NotifyOfPropertyChange(); }
        }

        private UserGroup _selectedGroup;
        public UserGroup SelectedGroup
        {
            get => _selectedGroup;
            set { _selectedGroup = value; NotifyOfPropertyChange(); }
        }

        private UserDto _selectedUser;
        public UserDto SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(()=> IsSelectUser);
            }
        }

        public bool IsSelectUser => SelectedUser != null;

        private bool _isFriend = false;
        public bool IsFriend {
            get => _isFriend;
            set
            {
                _isFriend = value;
                NotifyOfPropertyChange();
            }
        }


        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                NotifyOfPropertyChange();
                FilterContacts();
            }
        }

        public async void GetData()
        {
            SelectedUser = null;
            SelectedGroup = null;
            UserGroups.Clear();
            List<UserDto> list;
            //申请列表
            list = await ApiService.GetApplyFriends();
            if (list != null && list.Count > 0)
            {
                var item = new UserGroup()
                {
                    Title = "申请列表",
                    Count = list.Count,
                    Type = "Apply",
                    IsVisible = true,
                    SubItems = new ObservableCollection<UserDto>(list)
                };
                UserGroups.Add(item);
            }
            //获取被拒列表
            list = await ApiService.GetRejectFriends();
            if (list != null && list.Count > 0)
            {
                var item = new UserGroup()
                {
                    Title = "被拒列表",
                    Count = list.Count,
                    Type = "Reject",
                    IsVisible = true,
                    SubItems = new ObservableCollection<UserDto>(list)
                };
                UserGroups.Add(item);
            }
            
        }

        public void SelectdItem(UserGroup dataContext, object selectedItem)
        {
            UserGroup group  = dataContext as UserGroup;
            SelectedGroup = group;
            UserDto user = group.SelectdItem;
            //var lv = selectedItem as ListView;
            //UserDto user = lv.SelectedItem as UserDto;
            SelectedUser = user;
            IsFriend = SelectedUser != null && SelectedGroup != null && SelectedGroup.Type == "Apply";
        }

        public void Search()
        {
            FilterContacts();
        }

        private void FilterContacts()
        {
            foreach (var group in UserGroups)
            {
                if (!string.IsNullOrEmpty(SearchText))
                {
                    if (group.SubItems != null)
                    {
                        foreach (var subItem in group.SubItems)
                        {
                            subItem.IsVisible = subItem.NickName.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
                        }
                    }
                }
                else
                {
                    if (group.SubItems != null)
                    {
                        foreach (var subItem in group.SubItems)
                        {
                            subItem.IsVisible = true;
                        }
                    }
                }
                group.IsVisible = group.SubItems != null && group.SubItems.Count > 0;
            }
        }

        /// <summary>
        /// 同意
        /// </summary>
        public async void ThroughFriend()
        {
            var mes = MessageBox.Show($"同意 {SelectedUser.NickName} 为好友?", "添加好友", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mes == MessageBoxResult.Yes)
            {
                await ApiService.ThroughFriend(SelectedUser.Id);

                GetData();
            }
        }

        /// <summary>
        /// 拒绝
        /// </summary>
        public async void RejectFriend()
        {
            var mes = MessageBox.Show($"拒绝添加 {SelectedUser.NickName} 为好友?", "添加好友", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mes == MessageBoxResult.Yes)
            {
                await ApiService.RejectFriend(SelectedUser.Id);

                GetData();
            }
        }


    }
}
