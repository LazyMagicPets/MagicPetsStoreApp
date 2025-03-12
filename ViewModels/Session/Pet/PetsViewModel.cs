namespace ViewModels;
using LazyMagic.Client.FactoryGenerator; // do not put in global using. Causes runtime error.
[Factory]
/// <inheritdoc/>
public class PetsViewModel : LzItemsViewModelAuthNotifications<PetViewModel, Pet, PetModel>
{
    public PetsViewModel(
        [FactoryInject]ILoggerFactory loggerFactory,
        ISessionViewModel sessionViewModel,
        [FactoryInject] IPetViewModelFactory petViewModelFactory) : base(loggerFactory, sessionViewModel)  
    { 
        _sessionViewModel = sessionViewModel;
        PetViewModelFactory = petViewModelFactory;
        _DTOReadListAsync = sessionViewModel.Public.ListPetsAsync;

    }
    private ISessionViewModel _sessionViewModel;
    public IPetViewModelFactory? PetViewModelFactory { get; init; }
    /// <inheritdoc/>
    public override (PetViewModel, string) NewViewModel(Pet dto)
        => (PetViewModelFactory!.Create(_sessionViewModel, this, dto), string.Empty);
    public Func<Task<string>>? SvcTestAsync { get; init; }
    public async Task<string> TestAsync()
    {
        if (SvcTestAsync is null)
            return string.Empty;
        return await SvcTestAsync();    
    }
    /// <inheritdoc/>
    public override async Task<(bool, string)> ReadAsync(bool forceload = false)
    => await base.ReadAsync(forceload);


}


