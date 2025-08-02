namespace ViewModels;


/// <summary>
/// This interface represents the current session view model.
/// It is used to allow DI to inject the current session view model.
/// The intent is for the injection to return the current 
/// SesionsViewModel.SessionViewModel.
/// </summary>
public interface ICurrentSessionViewModel : ISessionViewModel
{

}
