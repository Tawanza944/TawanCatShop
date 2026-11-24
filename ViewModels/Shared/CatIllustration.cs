namespace WebApplication1.ViewModels.Shared;

public enum CatPose
{
    Sleeping,
    Sitting,
    Stretching,
    Peeking
}

/// <summary>
/// ข้อมูลสำหรับ Partial รูปแมวลายเส้น หาก <see cref="Title"/> เป็น null จะถือว่าเป็นรูปตกแต่ง
/// รูปตกแต่งจะซ่อนจากโปรแกรมอ่านหน้าจอ หากมีชื่อจะใช้ชื่อนั้นอธิบายรูป
/// </summary>
public sealed record CatIllustration(CatPose Pose, string? Title = null, string? CssClass = null)
{
    public bool IsDecorative => string.IsNullOrWhiteSpace(Title);
}
