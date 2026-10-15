<#
.SYNOPSIS
    把编译好的 ACR dll 部署到 PromeRotation 的 ACR 目录，并做一致性检查。

.DESCRIPTION
    PromeRotation 从下面这个目录加载 ACR：

        %APPDATA%\XIVLauncherCN\pluginConfigs\PromeRotation\ACR\<作者名>\<你的 dll>

    **<作者名> 必须和 RotationInfo.cs 里的 Author 一模一样**，否则 PR 不认。
    作者的官方教程原话：「这里的文件夹名必须和 RotationInfo.cs 中的 Author 一致」。

    同一个 ACR 目录里至少要有一个 dll；PR 的日志会明确说
    「跳过 ACR 包 X：目录下未找到任何 DLL 文件。」——看到这句就是这个原因。

.PARAMETER Author
    ACR 作者名（= PR 的 ACR 子目录名）。必填。

.PARAMETER Dll
    要部署的 dll 路径。默认从 src\<Project>\bin\Release\ 里找。

.PARAMETER Configuration
    Debug / Release，默认 Release。

.PARAMETER Open
    部署完后用资源管理器打开目标目录。

.EXAMPLE
    .\deploy-acr.ps1 -Author PromeAuthor
    .\deploy-acr.ps1 -Author PromeAuthor -Configuration Debug -Open
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Author,
    [string]$Dll,
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Release',
    [switch]$Open,
    # 覆盖 ACR 根目录。默认是 PR 的真实加载目录；
    # 想先看看脚本会做什么、或者往别处部署时用它（例：-AcrRoot $env:TEMP\acr-test）。
    [string]$AcrRoot
)

$ErrorActionPreference = 'Stop'

$ws = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($AcrRoot)) {
    $acrRoot = Join-Path $env:APPDATA 'XIVLauncherCN\pluginConfigs\PromeRotation\ACR'
} else {
    $acrRoot = $AcrRoot
}
$dest = Join-Path $acrRoot $Author

# --- 解析源 dll ---
# 宿主/引用程序集的精确文件名（**必须精确匹配**，不能用前缀——
# 实测踩过：用 `^PromeRotation` 前缀匹配会把 `PromeRotation.Sample.dll` 自己也排掉，
# 脚本于是报「找不到可部署的 dll」，而文件明明就在那儿）。
$hostAssemblies = @(
    'PromeRotation.dll', 'PromeRotation.Scripting.dll', 'Dalamud.dll',
    'Dalamud.Bindings.ImGui.dll', 'Dalamud.Bindings.ImPlot.dll', 'Dalamud.Bindings.ImGuizmo.dll',
    'FFXIVClientStructs.dll', 'InteropGenerator.Runtime.dll',
    'Lumina.dll', 'Lumina.Excel.dll', 'Newtonsoft.Json.dll', 'Serilog.dll',
    'Microsoft.Extensions.ObjectPool.dll', 'ECommons.dll'
)

if ([string]::IsNullOrWhiteSpace($Dll)) {
    $candidates = Get-ChildItem (Join-Path $ws 'src') -Directory -ErrorAction SilentlyContinue | ForEach-Object {
        Get-ChildItem (Join-Path $_.FullName "bin\$Configuration") -Filter '*.dll' -File -ErrorAction SilentlyContinue
    } | Where-Object {
        $hostAssemblies -notcontains $_.Name
    }
    if (-not $candidates) {
        throw "在 src\*\bin\$Configuration\ 下找不到可部署的 dll，先 dotnet build -c $Configuration"
    }
    if ($candidates.Count -gt 1) {
        Write-Host '找到多个候选 dll，请用 -Dll 指定：' -ForegroundColor Yellow
        $candidates | ForEach-Object { Write-Host "  $($_.FullName)" }
        throw '候选不唯一'
    }
    $Dll = $candidates[0].FullName
}

if (-not (Test-Path $Dll)) { throw "找不到 dll：$Dll" }
$src = Get-Item $Dll

Write-Host ''
Write-Host "源   : $($src.FullName)" -ForegroundColor Cyan
Write-Host "目标 : $dest" -ForegroundColor Cyan
Write-Host ''

New-Item -ItemType Directory -Force -Path $dest | Out-Null

# --- 拷贝（同类文件一起走：dll + pdb + deps.json）---
Copy-Item $src.FullName -Destination $dest -Force
Write-Host "  [已部署] $($src.Name)" -ForegroundColor Green

foreach ($ext in @('.pdb', '.deps.json')) {
    $side = [System.IO.Path]::ChangeExtension($src.FullName, $null).TrimEnd('.') + $ext
    if (Test-Path $side) {
        Copy-Item $side -Destination $dest -Force
        Write-Host "  [已部署] $(Split-Path -Leaf $side)" -ForegroundColor Green
    }
}

# --- 图标资源（Resources\）---
# 热键面板的自定义图标（九宫格支援 / 清扫队列）靠【绝对路径】加载，路径按
#   <pluginConfigs>\PromeRotation\ACR\<作者名>\Resources\<文件名>
# 拼出来（见 src\...\Paladin\Hotkey\PldHotkeys.cs 的 PldHotkeyIcon），
# 所以 png 必须跟着 dll 一起进 ACR 目录 —— 只拷 dll 的话图标会静默退回「技能自带图标」。
$srcRes = Join-Path (Split-Path -Parent $src.FullName) 'Resources'
if (Test-Path $srcRes) {
    $destRes = Join-Path $dest 'Resources'
    New-Item -ItemType Directory -Force -Path $destRes | Out-Null
    Copy-Item (Join-Path $srcRes '*') -Destination $destRes -Recurse -Force
    $resCount = (Get-ChildItem $destRes -File -Recurse).Count
    Write-Host "  [已部署] Resources\（$resCount 个文件）" -ForegroundColor Green
} else {
    Write-Host "提示：产物目录里没有 Resources\ —— 热键会退回「技能自带图标」（不影响功能）。" -ForegroundColor DarkYellow
}

# --- 检查：目录里到底有没有 dll ---
$deployed = Get-ChildItem $dest -Filter '*.dll' -File
Write-Host ''
Write-Host "目录内 dll 数量：$($deployed.Count)" -ForegroundColor Cyan
if ($deployed.Count -eq 0) {
    Write-Host '⚠️ 目标目录里没有 dll —— PR 会报「跳过 ACR 包：目录下未找到任何 DLL 文件」' -ForegroundColor Red
}

# --- 检查：deps.json（PR 用它解析依赖；缺了不一定炸，但少一层保障）---
if (-not (Test-Path (Join-Path $dest "$($src.BaseName).deps.json"))) {
    Write-Host "提示：没有 $($src.BaseName).deps.json。默认构建会生成；如果被 <GenerateDependencyFile>false</GenerateDependencyFile> 关掉了就补回来。" -ForegroundColor DarkYellow
}

# ============================================================================
#  ★★ 一致性校验（2026-10-12 深夜新增）
# ============================================================================
#  为什么要这一块：本工程踩过一次「源码改到 22:26，游戏里跑的却是 13:18 的旧 dll」。
#  当时的情形是：新代码只编进了 bin\Debug，而这条脚本的 -Configuration 默认 Release，
#  于是它把 bin\Release 里那份**九个钟头前**的 Release 盖到了 ACR 目录上 ——
#  而 Copy-Item 会保留源文件的 LastWriteTime，所以光看目录里 dll 的时间戳
#  根本看不出「刚盖过一次旧的」。事后是靠 PR 的 ACR 缓存 + 落盘设置存档
#  才反推出来的，白排查一小时。
#
#  所以这里做两件事：① 回读目标 dll 的 SHA256 与源比对（不一致就直接失败）；
#  ② 提醒「源码比 dll 新」= 改了但没重新编译。
# ============================================================================

$srcHash = (Get-FileHash $src.FullName -Algorithm SHA256).Hash
$dstFile = Join-Path $dest $src.Name
$dstHash = (Get-FileHash $dstFile -Algorithm SHA256).Hash
$dstItem = Get-Item $dstFile

Write-Host ''
Write-Host '--- 一致性校验 -------------------------------------------------' -ForegroundColor Cyan
Write-Host ("源   : {0}  {1}  {2} 字节  SHA256 {3}…" -f $src.Name, $src.LastWriteTime, $src.Length, $srcHash.Substring(0, 16))
Write-Host ("目标 : {0}  {1}  {2} 字节  SHA256 {3}…" -f $dstItem.Name, $dstItem.LastWriteTime, $dstItem.Length, $dstHash.Substring(0, 16))

if ($srcHash -ne $dstHash) {
    Write-Host '❌ 部署后的 dll 与源不是同一份 —— 先别进游戏，查拷贝为什么失败。' -ForegroundColor Red
    exit 1
}
Write-Host '✅ 一致：游戏将要加载的就是刚才编出来的这一份。' -ForegroundColor Green

# --- 提醒：源码比 dll 新 = 改了但没编译（同一类坑的另一半）---
$projDir = $src.Directory.Parent.Parent.FullName
$newerSource = Get-ChildItem $projDir -Recurse -File -Include '*.cs', '*.csproj', '*.props' -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' -and $_.LastWriteTime -gt $src.LastWriteTime } |
    Sort-Object LastWriteTime -Descending

if ($newerSource) {
    Write-Host ''
    Write-Host ("⚠️ 源码比 dll 新：{0} 个文件在该 dll 之后被改过，最新的是 {1}（{2}）。" -f $newerSource.Count, $newerSource[0].Name, $newerSource[0].LastWriteTime) -ForegroundColor Yellow
    Write-Host ("   ⇒ 这份 dll 编于 {0}，**不含**那之后的改动。先 dotnet build 再部署，否则你又在测旧代码。" -f $src.LastWriteTime) -ForegroundColor Yellow
}

Write-Host ''
Write-Host '下一步：进游戏，在 PR 的 ACR 页面里加载它。' -ForegroundColor Green
Write-Host '  改完代码重新 dotnet build 之后，多数情况下需要重新加载 ACR（PR 没有 AE 那种热重载保证）。' -ForegroundColor DarkGray
Write-Host ''
Write-Host '怎么确认游戏里加载的就是这一份（30 秒）：' -ForegroundColor Green
Write-Host '  1) 在 PR 里重新加载 ACR（或重进游戏）；' -ForegroundColor DarkGray
Write-Host '  2) 日志里搜 [LittleDart-GNB] 的欢迎语：<Build 时间> 应该等于上面「源」的编译时间；' -ForegroundColor DarkGray
Write-Host ("  3) 或者直接核对 ACR 目录 dll 的 SHA256 前 16 位 = {0}。" -f $srcHash.Substring(0, 16)) -ForegroundColor DarkGray
Write-Host ''

if ($Open) { Start-Process explorer.exe $dest }
