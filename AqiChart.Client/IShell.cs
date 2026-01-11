using Caliburn.Micro;

namespace AqiChart.Client
{
    public interface IShell { }

    public interface IChildViewModel : Caliburn.Micro.IScreen
    {
        string PageName { get; set; }
    }

    public interface IChatViewModel : Caliburn.Micro.IScreen
    {
        string UserId { get; set; }
    }

    
}
