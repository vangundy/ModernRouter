using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace ModernRouter.Animations;

public interface IRouteAnimationService
{
    Task<AnimationResult> BeginExitAsync(ComponentBase component, RouteAnimationContext context);
    Task<AnimationResult> BeginEnterAsync(ComponentBase component, RouteAnimationContext context);
    void RegisterAnimation(string name, RouteAnimation animation);
    RouteAnimation? GetAnimation(string name);
    bool IsAnimationEnabled { get; }
}

public sealed record RouteAnimationContext
{
    public string FromRoute { get; init; } = "";
    public string ToRoute { get; init; } = "";
    public Dictionary<string, object?> RouteValues { get; init; } = new();
    public NavigationType NavigationType { get; init; } = NavigationType.Push;
    public CancellationToken CancellationToken { get; init; }
    public string? AnimationOverride { get; init; }
}

public enum NavigationType
{
    Push,       // Forward navigation
    Pop,        // Back navigation  
    Replace,    // Replace current route
    Redirect    // Programmatic redirect
}

public sealed record AnimationResult
{
    public bool Success { get; init; }
    public TimeSpan Duration { get; init; }
    public string? ErrorMessage { get; init; }
    public string? GeneratedCss { get; init; }
    
    public static AnimationResult Successful(TimeSpan duration, string? css = null) 
        => new() { Success = true, Duration = duration, GeneratedCss = css };
        
    public static AnimationResult Failed(string error) 
        => new() { Success = false, ErrorMessage = error };
        
    public static AnimationResult None 
        => new() { Success = true, Duration = TimeSpan.Zero };
}

public sealed record RouteAnimation
{
    public string EnterKeyframes { get; init; } = "";
    public string ExitKeyframes { get; init; } = "";
    public TimeSpan Duration { get; init; } = TimeSpan.FromMilliseconds(300);
    public AnimationEasing Easing { get; init; } = AnimationEasing.EaseInOut;
    public bool EnableViewTransitions { get; init; } = true;
    public string? CustomCss { get; init; }
}

public enum AnimationEasing
{
    [Description("linear")]
    Linear,
    [Description("ease-in")]
    EaseIn,
    [Description("ease-out")]
    EaseOut,
    [Description("ease-in-out")]
    EaseInOut,
    [Description("cubic-bezier(0.68, -0.55, 0.265, 1.55)")]
    EaseInBack,
    [Description("cubic-bezier(0.175, 0.885, 0.32, 1.275)")]
    EaseOutBack
}