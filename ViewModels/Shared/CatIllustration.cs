namespace WebApplication1.ViewModels.Shared;

public enum CatPose
{
    Sleeping,
    Sitting,
    Stretching,
    Peeking
}

/// <summary>
/// Model for the shared line-art cat partial. A null <see cref="Title"/> renders the drawing
/// as decoration (hidden from screen readers); a title makes it a described image.
/// </summary>
public sealed record CatIllustration(CatPose Pose, string? Title = null, string? CssClass = null)
{
    public bool IsDecorative => string.IsNullOrWhiteSpace(Title);
}
