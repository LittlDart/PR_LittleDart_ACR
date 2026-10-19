namespace LittleDart;

public static class RotationInfo
{
	public const uint JobId = 19u;

	public const string Name = "LittleDart";

	public const string Author = "LittleDart";

	// ★ 2026-10-18 官方口径审计 P0：同一 dll 内所有 [RotationMetadata] 的 Version 必须**逐字相同**，
	//   否则 RotationRegistry.ValidateExternalPackage 抛「同一作者包中的 Version 必须一致」
	//   并**拒绝整个 ACR 包**（不是只丢一个职业）。证据：sdk-ref:63465-63469 / impl:92950-92954；
	//   本机 PR 日志 PRError-20261018-192854-620.log 19:29:47、19:40:27 两次拒绝（1.0.5, 1.0.3）。
	//   ⇒ 全工程只保留**这一个**版本字面量；GnbRotationInfo.Version 直接引用它，禁止再各写一份。
	public const string Version = "1.0.7";
}
