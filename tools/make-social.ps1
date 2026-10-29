# Generates the 1280x640 social preview image (GitHub "Social preview" and og:image).
# Run: powershell -ExecutionPolicy Bypass -File tools\make-social.ps1

Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $PSScriptRoot
$out = Join-Path $root 'docs\social-preview.png'
$iconPath = Join-Path $root 'assets\SpeakerAwake-256.png'

$bmp = New-Object System.Drawing.Bitmap 1280, 640
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$g.Clear([System.Drawing.Color]::FromArgb(17, 24, 39))

$icon = [System.Drawing.Image]::FromFile($iconPath)
$g.DrawImage($icon, 96, 192, 256, 256)
$icon.Dispose()

$white = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::White)
$muted = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(156, 163, 175))
$green = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(46, 184, 92))
$title = New-Object System.Drawing.Font 'Segoe UI Semibold', 72, ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
$sub = New-Object System.Drawing.Font 'Segoe UI', 36, ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
$small = New-Object System.Drawing.Font 'Segoe UI', 26, ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)

$g.DrawString('SpeakerAwake', $title, $white, 400, 200)
$g.DrawString("Stop your Bluetooth speaker`nturning off automatically", $sub, $muted, 404, 300)
$dot = [char]0x00B7   # kept as a code point: Windows PowerShell 5.1 reads BOM-less scripts as ANSI
$g.DrawString("Free  $dot  Open source  $dot  Windows 10 & 11", $small, $green, 406, 420)

$g.Dispose()
$bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Host "Wrote $out"
