using ModernRouter.Routing;

namespace ModernRouterDemo.Guards;

//public sealed class UnsavedGuard : INavMiddleware
//{
//    private readonly IUiDialog _dialog;
//    private readonly EditState _edit;

//    public UnsavedGuard(IUiDialog dialog, EditState edit)
//        => (_dialog, _edit) = (dialog, edit);

//    public async Task<NavResult> InvokeAsync(NavContext ctx, Func<Task<NavResult>> next)
//    {
//        if (_edit.HasUnsavedChanges)
//        {
//            var ok = await _dialog.ConfirmAsync("Discard changes?");
//            if (!ok) return NavResult.Cancel();
//        }
//        return await next();
//    }
//}