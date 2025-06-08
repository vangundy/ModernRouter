using Microsoft.AspNetCore.Components;

namespace ModernRouter.Animations;

public interface IAnimationLifecycleHooks
{
    Task OnAnimationStartAsync(AnimationPhase phase, RouteAnimationContext context);
    Task OnAnimationCompleteAsync(AnimationPhase phase, RouteAnimationContext context, AnimationResult result);
    Task OnAnimationCancelledAsync(AnimationPhase phase, RouteAnimationContext context);
}

public enum AnimationPhase
{
    Exit,
    Enter
}

public abstract class AnimationLifecycleHooksBase : ComponentBase, IAnimationLifecycleHooks
{
    public virtual Task OnAnimationStartAsync(AnimationPhase phase, RouteAnimationContext context)
        => Task.CompletedTask;

    public virtual Task OnAnimationCompleteAsync(AnimationPhase phase, RouteAnimationContext context, AnimationResult result)
        => Task.CompletedTask;

    public virtual Task OnAnimationCancelledAsync(AnimationPhase phase, RouteAnimationContext context)
        => Task.CompletedTask;
}