<#
.SYNOPSIS
    把工作区里的文本文件统一转成 UTF-8 with BOM。改完文件跑一下就行。

.DESCRIPTION
    和 D:\LittleDartACR\scripts\fix-encoding.ps1 是同一套规矩，只改了跳过清单。

    为什么需要这个脚本 —— 本机（Windows PowerShell 5.1 + 简体中文系统）会踩两个坑：

    1) PowerShell 5.1 读取**没有 BOM** 的 .ps1 时，会按系统 ANSI 代码页
       （简体中文 = GBK）解码。脚本里的中文注释和字符串会整成乱码，
       严重时直接变成 ParserError，脚本根本跑不起来。

    2) MSBuild / Roslyn 虽然默认按 UTF-8 读 .cs 和 .csproj，但只要文件里
       出现中文（ACR 名称、设置项标签、注释），带 BOM 才是最稳的。

    还有一个相反的坑：用 PowerShell 的 Get-Content 去读本来就是 UTF-8
    的文件（例如 PromeRotation 的配置 json），5.1 也会按 ANSI 解码，
    看起来像文件坏了，其实文件是好的。要读这种文件请用：
        [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)

.PARAMETER Check
    只报告哪些文件缺 BOM，不做修改。可以直接用退出码判断（0 = 全部合规）。

.PARAMETER Path
    要处理的根目录，默认是脚本上一级（工作区根）。

.EXAMPLE
    .\fix-encoding.ps1
    .\fix-encoding.ps1 -Check
#>
[CmdletBinding()]
param(
    [string]$Path,
    [switch]$Check
)

$ErrorActionPreference = 'Stop'

$here = $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($here)) {
    $commandPath = $MyInvocation.MyCommand.Path
    if (-not [string]::IsNullOrWhiteSpace($commandPath)) { $here = Split-Path -Parent $commandPath }
}
if ([string]::IsNullOrWhiteSpace($here)) { $here = (Get-Location).Path }

if ([string]::IsNullOrWhiteSpace($Path)) { $Path = Split-Path -Parent $here }
$root = (Resolve-Path $Path).Path

# 需要带 BOM 的文本类型
#
# ⚠️ 这里**故意不含 .json**：JSON 规范不允许 BOM，凡是用 JSON.parse 读文件的
#    地方都会直接炸。本工作区里 PromeRotation 的配置（AcrSelections.json、
#    PromeSettings.json）都是无 BOM UTF-8，给它们加 BOM 会让插件读不出来。
$extensions = @('.ps1', '.psm1', '.cs', '.csproj', '.props', '.targets', '.slnx',
                '.md', '.config', '.xml', '.editorconfig')

# 不动的东西：第三方源码、构建产物、二进制
$skipPatterns = @(
    '\\libs\\',
    '\\bin\\',
    '\\obj\\',
    '\\dist\\',
    '\\artifacts\\',
    # ⚠️ 备份目录：里面是历史快照，必须保持原样（包括「本来就缺 BOM」这件事）。
    #    给备份补 BOM 会让快照和源头对不上，而且很难看出来。
    '\\backups\\',
    '\\.git\\',
    '\\.vs\\',
    '\\.idea\\',
    '\\node_modules\\',
    # ⚠️ 反编译出来的参考源码：体积巨大（单个 .cs 可达 6 MB），
    #    而且不是本工作区的代码，改它只会让 -Check 永远跑不完。
    '\\refs\\',
    # ⚠️ DSH 的技能目录：给 SKILL.md 加 BOM 会让技能**静默消失**！
    #    技能加载器按「文件必须以 --- 开头」解析 YAML frontmatter，
    #    BOM 挡在前面 → frontmatter 解析失败 → 该技能从目录里凭空不见，
    #    日志里没有任何警告。所以这里必须跳过整个 .dsh\ 子树。
    '\\.dsh\\',
    # 第三方 clone 下来的参考源码（如 .tmp-pr*）
    '\\.tmp-',
    # NuGet 还原生成的锁文件：机器产物、纯 ASCII
    '\\packages\.lock\.json$'
)

$utf8Bom = New-Object System.Text.UTF8Encoding($true)

$scanned = 0
$needsFix = New-Object System.Collections.Generic.List[string]

$files = Get-ChildItem $root -Recurse -File | Where-Object {
    $extensions -contains $_.Extension
}

foreach ($file in $files) {
    $skip = $false
    foreach ($pattern in $skipPatterns) {
        if ($file.FullName -match $pattern) { $skip = $true; break }
    }
    if ($skip) { continue }

    $scanned++
    $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
    $hasBom = ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)
    if ($hasBom) { continue }

    $needsFix.Add($file.FullName.Substring($root.Length).TrimStart('\'))
}

Write-Host ''
Write-Host "扫描 $scanned 个文本文件，其中 $($needsFix.Count) 个缺 UTF-8 BOM" -ForegroundColor Cyan

if ($needsFix.Count -eq 0) {
    Write-Host '全部合规。' -ForegroundColor Green
    Write-Host ''
    exit 0
}

if ($Check) {
    Write-Host ''
    foreach ($f in $needsFix) { Write-Host "  [缺 BOM] $f" -ForegroundColor Yellow }
    Write-Host ''
    Write-Host '加上 -Check 之外不带参数运行即可修复。' -ForegroundColor DarkGray
    Write-Host ''
    exit 1
}

foreach ($relative in $needsFix) {
    $full = Join-Path $root $relative
    # 先按 UTF-8 读出（File.ReadAllText 会自动识别并吃掉已有 BOM），再带 BOM 写回
    # ★★ 必须【先整个读进来，再写回去】。
    #    历史事故：用旧版 python 单行脚本「逐行读、逐行写」处理 900 行的源文件，
    #    第一个文件被清成 3 字节，编译一次性报 39 个 CS0103。
    $text = [System.IO.File]::ReadAllText($full, [System.Text.Encoding]::UTF8)
    [System.IO.File]::WriteAllText($full, $text, $utf8Bom)
    Write-Host "  [已修复] $relative" -ForegroundColor Green
}

Write-Host ''
Write-Host "完成：$($needsFix.Count) 个文件已转为 UTF-8 with BOM。" -ForegroundColor Green
Write-Host ''
exit 0
