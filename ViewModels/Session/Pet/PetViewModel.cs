namespace ViewModels;
using LazyMagic.Client.FactoryGenerator; // do not put in global using. Causes runtime error.

[Factory]
public class PetViewModel : LzItemViewModelAuthNotifications<Pet, PetModel>
{
    public PetViewModel(
        [FactoryInject] ILoggerFactory loggerFactory,   
        ISessionViewModel sessionViewModel,
        ILzParentViewModel parentViewModel,
        Pet pet,
        bool? isLoaded = null
        ) : base(loggerFactory, sessionViewModel, pet, model: null, isLoaded) 
    {
        _sessionViewModel = sessionViewModel;   
        ParentViewModel = parentViewModel;
        _DTOCreateAsync = sessionViewModel.Store.AddPetAsync;
        _DTOReadAsync = sessionViewModel.Public.GetPetByIdAsync;
        _DTOUpdateAsync = sessionViewModel.Store.UpdatePetAsync;
        _DTODeleteAsync = sessionViewModel.Store.DeletePetAsync;   
    }
    private ISessionViewModel _sessionViewModel;
    public override string Id => Data?.Id ?? string.Empty;
    public override long UpdatedAt => Data?.UpdateUtcTick ?? long.MaxValue;

}
