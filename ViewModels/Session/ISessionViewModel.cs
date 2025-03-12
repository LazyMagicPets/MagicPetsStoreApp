
using System.ComponentModel;

namespace ViewModels;

public interface ISessionViewModel : ILzSessionViewModelAuthNotifications
{
    IStoreApi Store { get; set; }
    IConsumerApi Consumer { get; set; }
    IPublicApi Public { get; set; } 
    PetsViewModel PetsViewModel { get; set; }
    CategoriesViewModel CategoriesViewModel { get; set; }
    TagsViewModel TagsViewModel { get; set; }   

    public string TenantName { get; set; }
}