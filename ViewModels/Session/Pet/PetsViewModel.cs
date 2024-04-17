namespace ViewModels;
[Factory]
public class PetsViewModel : LzItemsViewModelAuthNotifications<PetViewModel, Pet, PetModel>
{
    public PetsViewModel(
        ISessionViewModel sessionViewModel,
        [FactoryInject] IPetViewModelFactory petViewModelFactory) : base(sessionViewModel)  
    { 
        _sessionViewModel = sessionViewModel;
        PetViewModelFactory = petViewModelFactory;
        _DTOReadListAsync = sessionViewModel.Store.ListPetsAsync;
        SvcTestAsync = sessionViewModel.Store.TestAsync;

    }
    private ISessionViewModel _sessionViewModel;
    public IPetViewModelFactory? PetViewModelFactory { get; init; }
    public override (PetViewModel, string) NewViewModel(Pet dto)
        => (PetViewModelFactory!.Create(_sessionViewModel, this, dto), string.Empty);
    public Func<Task<string>>? SvcTestAsync { get; init; }
    public async Task<string> TestAsync()
    {
        if (SvcTestAsync is null)
            return string.Empty;
        return await SvcTestAsync();
    }

    public override async Task<(bool, string)> ReadAsync(bool forceload = false, StorageAPI storageAPI = StorageAPI.DTO)
    => await base.ReadAsync(string.Empty, forceload, storageAPI);
}
