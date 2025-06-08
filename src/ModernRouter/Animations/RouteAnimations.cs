namespace ModernRouter.Animations;

public static class RouteAnimations
{
    // Slide animations
    public static RouteAnimation SlideLeft => new()
    {
        EnterKeyframes = "transform: translateX(100%); opacity: 0; => transform: translateX(0); opacity: 1;",
        ExitKeyframes = "transform: translateX(0); opacity: 1; => transform: translateX(-100%); opacity: 0;",
        Duration = TimeSpan.FromMilliseconds(300),
        Easing = AnimationEasing.EaseInOut
    };
    
    public static RouteAnimation SlideRight => new()
    {
        EnterKeyframes = "transform: translateX(-100%); opacity: 0; => transform: translateX(0); opacity: 1;",
        ExitKeyframes = "transform: translateX(0); opacity: 1; => transform: translateX(100%); opacity: 0;",
        Duration = TimeSpan.FromMilliseconds(300),
        Easing = AnimationEasing.EaseInOut
    };
    
    public static RouteAnimation SlideUp => new()
    {
        EnterKeyframes = "transform: translateY(100%); opacity: 0; => transform: translateY(0); opacity: 1;",
        ExitKeyframes = "transform: translateY(0); opacity: 1; => transform: translateY(-100%); opacity: 0;",
        Duration = TimeSpan.FromMilliseconds(300),
        Easing = AnimationEasing.EaseInOut
    };
    
    public static RouteAnimation SlideDown => new()
    {
        EnterKeyframes = "transform: translateY(-100%); opacity: 0; => transform: translateY(0); opacity: 1;",
        ExitKeyframes = "transform: translateY(0); opacity: 1; => transform: translateY(100%); opacity: 0;",
        Duration = TimeSpan.FromMilliseconds(300),
        Easing = AnimationEasing.EaseInOut
    };
    
    // Fade animations
    public static RouteAnimation FadeIn => new()
    {
        EnterKeyframes = "opacity: 0; => opacity: 1;",
        ExitKeyframes = "opacity: 1; => opacity: 0;",
        Duration = TimeSpan.FromMilliseconds(250),
        Easing = AnimationEasing.EaseInOut
    };
    
    public static RouteAnimation FadeOut => new()
    {
        EnterKeyframes = "opacity: 0; => opacity: 1;",
        ExitKeyframes = "opacity: 1; => opacity: 0;",
        Duration = TimeSpan.FromMilliseconds(250),
        Easing = AnimationEasing.EaseInOut
    };
    
    // Scale animations
    public static RouteAnimation ScaleIn => new()
    {
        EnterKeyframes = "transform: scale(0.8); opacity: 0; => transform: scale(1); opacity: 1;",
        ExitKeyframes = "transform: scale(1); opacity: 1; => transform: scale(1.1); opacity: 0;",
        Duration = TimeSpan.FromMilliseconds(300),
        Easing = AnimationEasing.EaseOutBack
    };
    
    public static RouteAnimation ScaleOut => new()
    {
        EnterKeyframes = "transform: scale(1.2); opacity: 0; => transform: scale(1); opacity: 1;",
        ExitKeyframes = "transform: scale(1); opacity: 1; => transform: scale(0.8); opacity: 0;",
        Duration = TimeSpan.FromMilliseconds(300),
        Easing = AnimationEasing.EaseInBack
    };
    
    // Zoom animations
    public static RouteAnimation ZoomIn => new()
    {
        EnterKeyframes = "transform: scale(0); opacity: 0; => transform: scale(1); opacity: 1;",
        ExitKeyframes = "transform: scale(1); opacity: 1; => transform: scale(0); opacity: 0;",
        Duration = TimeSpan.FromMilliseconds(400),
        Easing = AnimationEasing.EaseOutBack
    };
    
    public static RouteAnimation ZoomOut => new()
    {
        EnterKeyframes = "transform: scale(2); opacity: 0; => transform: scale(1); opacity: 1;",
        ExitKeyframes = "transform: scale(1); opacity: 1; => transform: scale(2); opacity: 0;",
        Duration = TimeSpan.FromMilliseconds(400),
        Easing = AnimationEasing.EaseInBack
    };
    
    // No animation
    public static RouteAnimation None => new()
    {
        EnterKeyframes = "",
        ExitKeyframes = "",
        Duration = TimeSpan.Zero,
        Easing = AnimationEasing.Linear
    };
    
    // Popular combinations
    public static RouteAnimation PageTransition => new()
    {
        EnterKeyframes = "transform: translateX(20px); opacity: 0; => transform: translateX(0); opacity: 1;",
        ExitKeyframes = "transform: translateX(0); opacity: 1; => transform: translateX(-20px); opacity: 0;",
        Duration = TimeSpan.FromMilliseconds(200),
        Easing = AnimationEasing.EaseOut
    };
    
    public static RouteAnimation ModalEntry => new()
    {
        EnterKeyframes = "transform: scale(0.9) translateY(-20px); opacity: 0; => transform: scale(1) translateY(0); opacity: 1;",
        ExitKeyframes = "transform: scale(1) translateY(0); opacity: 1; => transform: scale(0.9) translateY(-20px); opacity: 0;",
        Duration = TimeSpan.FromMilliseconds(250),
        Easing = AnimationEasing.EaseOut
    };
}