namespace ModernRouter.Animations;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RouteAnimationAttribute : Attribute
{
    public string? EnterAnimation { get; set; }
    public string? ExitAnimation { get; set; }
    public int Duration { get; set; } = 300;
    public AnimationEasing Easing { get; set; } = AnimationEasing.EaseInOut;
    public bool EnableViewTransitions { get; set; } = true;

    public RouteAnimationAttribute()
    {
    }

    public RouteAnimationAttribute(string enterAnimation, string exitAnimation)
    {
        EnterAnimation = enterAnimation;
        ExitAnimation = exitAnimation;
    }

    public RouteAnimationAttribute(string animation)
    {
        EnterAnimation = animation;
        ExitAnimation = animation;
    }
}