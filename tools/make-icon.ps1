# Generates assets\KeepSpeakerOn.ico (16-256 px, PNG-compressed entries) from the same
# speaker shape the tray icon uses (TrayApp.MakeIcon in src\KeepSpeakerOn.cs).
# Run: powershell -ExecutionPolicy Bypass -File tools\make-icon.ps1

Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $PSScriptRoot
$out = Join-Path $root 'assets\KeepSpeakerOn.ico'
New-Item -ItemType Directory -Force (Split-Path $out) | Out-Null

$color = [System.Drawing.Color]::FromArgb(46, 184, 92)
$sizes = 16, 24, 32, 48, 64, 128, 256

function New-IconPng([int]$size) {
    $bmp = New-Object System.Drawing.Bitmap $size, $size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)
    # Draw on the 16x16 grid used by the tray icon, scaled to the target size.
    $g.ScaleTransform($size / 16.0, $size / 16.0)

    $brush = New-Object System.Drawing.SolidBrush $color
    $pen = New-Object System.Drawing.Pen $color, 1.6
    $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round

    $points = [System.Drawing.Point[]]@(
        (New-Object System.Drawing.Point 1, 6), (New-Object System.Drawing.Point 4, 6),
        (New-Object System.Drawing.Point 8, 2), (New-Object System.Drawing.Point 8, 14),
        (New-Object System.Drawing.Point 4, 10), (New-Object System.Drawing.Point 1, 10))
    $g.FillPolygon($brush, $points)
    $g.DrawArc($pen, 6, 5, 6, 6, -55, 110)
    $g.DrawArc($pen, 4, 2, 11, 12, -55, 110)

    $pen.Dispose(); $brush.Dispose(); $g.Dispose()
    return $bmp
}

# 128+ px are stored as PNG (supported since Vista); smaller sizes as 32-bit DIBs for compatibility.
function ConvertTo-IconEntry([System.Drawing.Bitmap]$bmp) {
    $size = $bmp.Width
    $ms = New-Object System.IO.MemoryStream
    if ($size -ge 128) {
        $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
        return ,$ms.ToArray()
    }
    $rect = New-Object System.Drawing.Rectangle 0, 0, $size, $size
    $data = $bmp.LockBits($rect, [System.Drawing.Imaging.ImageLockMode]::ReadOnly,
        [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $pixels = New-Object byte[] ($data.Stride * $size)
    [System.Runtime.InteropServices.Marshal]::Copy($data.Scan0, $pixels, 0, $pixels.Length)
    $bmp.UnlockBits($data)

    $maskStride = [int]([Math]::Ceiling($size / 32.0) * 4)
    $bw = New-Object System.IO.BinaryWriter $ms
    $bw.Write([UInt32]40); $bw.Write([Int32]$size); $bw.Write([Int32]($size * 2))   # BITMAPINFOHEADER
    $bw.Write([UInt16]1); $bw.Write([UInt16]32); $bw.Write([UInt32]0)
    $bw.Write([UInt32]($size * $size * 4 + $maskStride * $size))
    $bw.Write([Int32]0); $bw.Write([Int32]0); $bw.Write([UInt32]0); $bw.Write([UInt32]0)
    for ($y = $size - 1; $y -ge 0; $y--) { $bw.Write($pixels, $y * $data.Stride, $size * 4) }  # bottom-up BGRA
    $bw.Write((New-Object byte[] ($maskStride * $size)))                                # AND mask (alpha wins)
    $bw.Flush()
    return ,$ms.ToArray()
}

$images = @()
foreach ($s in $sizes) {
    $bmp = New-IconPng $s
    $images += ,(ConvertTo-IconEntry $bmp)
    if ($s -eq 256) { $bmp.Save((Join-Path $root 'assets\KeepSpeakerOn-256.png'), [System.Drawing.Imaging.ImageFormat]::Png) }
    $bmp.Dispose()
}

$fs = [System.IO.File]::Create($out)
$w = New-Object System.IO.BinaryWriter $fs
$w.Write([UInt16]0); $w.Write([UInt16]1); $w.Write([UInt16]$sizes.Count)   # ICONDIR
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {                                  # ICONDIRENTRY
    $dim = if ($sizes[$i] -ge 256) { 0 } else { $sizes[$i] }
    $w.Write([byte]$dim); $w.Write([byte]$dim); $w.Write([byte]0); $w.Write([byte]0)
    $w.Write([UInt16]1); $w.Write([UInt16]32)
    $w.Write([UInt32]$images[$i].Length); $w.Write([UInt32]$offset)
    $offset += $images[$i].Length
}
foreach ($img in $images) { $w.Write($img) }
$w.Close()

Write-Host "Wrote $out ($((Get-Item $out).Length) bytes, sizes: $($sizes -join ', '))"
