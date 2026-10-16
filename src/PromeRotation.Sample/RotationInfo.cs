namespace PromeRotation.Sample;

/// <summary>
/// 这份 ACR 的基本信息。
/// </summary>
/// <remarks>
/// ⚠️ 三条来自官方指南的硬约束：
/// <list type="number">
/// <item><c>JobId</c> 不能是 0，否则 <c>RotationMetadataAttribute</c> 的构造函数直接抛异常。</item>
/// <item><c>Author</c> 后面要用来建文件夹（<c>PromeRotation\ACR\&lt;Author&gt;\</c>），
///       所以里面<b>不能有斜杠、冒号</b>这些 Windows 文件夹非法字符。</item>
/// <item><c>Version</c> 必须能被 <see cref="System.Version"/>.TryParse 解析
///       （即形如 <c>1.2.3</c> / <c>1.2.3.4</c>），否则也会在属性构造时抛异常。</item>
/// </list>
/// 这三条不是文档里的说明，是反编译 <c>RotationMetadataAttribute</c> 构造函数读出来的校验逻辑。
/// </remarks>
public static class RotationInfo
{
    /// <summary>职业 ID。24 = 白魔法师（WHM）。</summary>
    public const uint JobId = 24;

    /// <summary>这份 ACR 的名字，显示在 PR 的 ACR 列表里。</summary>
    public const string Name = "Sample";

    /// <summary>作者名。★ 必须和 PR 的 <c>ACR\</c> 子目录名一模一样。</summary>
    public const string Author = "PromeAuthor";

    /// <summary>版本号，必须能被 System.Version 解析。</summary>
    public const string Version = "0.1.0";
}
