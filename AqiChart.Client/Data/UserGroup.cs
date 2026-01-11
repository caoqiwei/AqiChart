using Caliburn.Micro;
using System;
using System.Collections.ObjectModel;

namespace AqiChart.Client.Data
{
    public class UserGroup : PropertyChangedBase
    {
        private string _title;
        private int _count;
        private bool _isExpanded;
        private bool _isVisible = true;

        public string Title
        {
            get => _title;
            set { _title = value; NotifyOfPropertyChange(); }
        }

        public int Count
        {
            get => _count;
            set { _count = value; NotifyOfPropertyChange(); }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; NotifyOfPropertyChange(); }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; NotifyOfPropertyChange(); }
        }

        private UserDto _selectdItem;
        public UserDto SelectdItem
        {
            get => _selectdItem;
            set
            {
                _selectdItem = value;
                NotifyOfPropertyChange();
            }
        }
        public string _type;
        public string Type
        {
            get => _type;
            set
            {
                _type = value;
                NotifyOfPropertyChange();
            }
        }
        public ObservableCollection<UserDto> SubItems { get; set; }


    }
}
