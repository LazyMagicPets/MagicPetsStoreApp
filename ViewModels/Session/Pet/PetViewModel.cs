namespace ViewModels;

[Factory]
public class PetViewModel : LzItemViewModelAuthNotifications<Pet, PetModel>
{
    public PetViewModel(
        ISessionViewModel sessionViewModel,
        ILzParentViewModel parentViewModel,
        Pet pet,
        bool? isLoaded = null
        ) : base(sessionViewModel, pet, model: null, isLoaded) 
    {
        _sessionViewModel = sessionViewModel;   
        ParentViewModel = parentViewModel;
        _DTOCreateAsync = sessionViewModel.Store.AddPetAsync;
        _DTOReadIdAsync = sessionViewModel.Store.GetPetByIdAsync;
    }
    private ISessionViewModel _sessionViewModel;
    public override string Id => Data?.Id ?? string.Empty;
    public override long UpdatedAt => Data?.UpdateUtcTick ?? long.MaxValue;

}
