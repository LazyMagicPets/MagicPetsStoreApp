
namespace ViewModels;
using LazyMagic.Client.FactoryGenerator; // do not put in global using. Causes runtime error.
[Factory]
public class CategoriesViewModel : LzItemsViewModelAuthNotifications<CategoryViewModel, Category, CategoryModel>
{
    public CategoriesViewModel(
        [FactoryInject] ILoggerFactory loggerFactory,
        ISessionViewModel sessionViewModel,
        [FactoryInject] ICategoryViewModelFactory categoryViewModelFactory) : base(loggerFactory, sessionViewModel)
    {
        _sessionViewModel = sessionViewModel;
        CategoryViewModelFactory = categoryViewModelFactory;
        _DTOReadListAsync = sessionViewModel.Public.GetPetCategoriesAsync;
    }
    private ISessionViewModel _sessionViewModel;
    public ICategoryViewModelFactory? CategoryViewModelFactory { get; init; }
    /// <inheritdoc/>
    public override (CategoryViewModel, string) NewViewModel(Category dto)
        => (CategoryViewModelFactory!.Create(_sessionViewModel, this, dto), string.Empty);
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
