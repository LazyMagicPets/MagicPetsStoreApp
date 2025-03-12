namespace ViewModels;
using LazyMagic.Client.FactoryGenerator; // do not put in global using. Causes runtime error.
[Factory]
public class TagViewModel : LzItemViewModelAuthNotifications<Tag, TagModel>
{
    public TagViewModel(
        [FactoryInject] ILoggerFactory loggerFactory,
        ISessionViewModel sessionViewModel,
        ILzParentViewModel parentViewModel,
        Tag tag,
        bool? isLoaded = null
        ) : base(loggerFactory, sessionViewModel, tag, model: null, isLoaded)
    {
        _sessionViewModel = sessionViewModel;
        ParentViewModel = parentViewModel;
        //_DTOCreateAsync = sessionViewModel.Store.AddTagAsync;
        //_DTOReadAsync = sessionViewModel.Public.GetTagByIdAsync;
        //_DTOUpdateAsync = sessionViewModel.Store.UpdateTagAsync;
        //_DTODeleteAsync = sessionViewModel.Store.DeleteTagAsync;
    }
    private ISessionViewModel _sessionViewModel;
    public override string Id => Data?.Id ?? string.Empty;
    public override long UpdatedAt => long.MaxValue;
}
