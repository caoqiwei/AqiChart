using System.Windows;
using System.Windows.Input;

namespace AqiChart.Client.Models.AddressBook
{
    /// <summary>
    /// AddFriendView.xaml 
    /// </summary>
    public partial class AddFriendView : Window
    {
        public AddFriendView()
        {
            InitializeComponent();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

    }
}
