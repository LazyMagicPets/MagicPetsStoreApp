
using System.ComponentModel;

namespace ViewModels;

public interface ISessionViewModel : ILzSessionViewModelAuthNotifications, INotifyPropertyChanged
{
    IService Store { get; set; }
    PetsViewModel PetsViewModel { get; set; }
    public string TenantName { get; set; }
}