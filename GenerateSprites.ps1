Add-Type -AssemblyName System.Drawing

$uiDir = Join-Path $PSScriptRoot "Assets\ZombieRunner\Textures\UI"
if (-not (Test-Path $uiDir)) {
    New-Item -ItemType Directory -Path $uiDir | Out-Null
}

function Save-Bmp($bmp, $name) {
    $path = Join-Path $uiDir $name
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Output "Created: $name"
}

# 1. UI_RoundedRect_64.png (128x128 white rounded rect for 9-slicing)
$bmp = New-Object System.Drawing.Bitmap(128, 128)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::Transparent)

$path = New-Object System.Drawing.Drawing2D.GraphicsPath
$r = 28; $d = $r * 2; $w = 127; $h = 127
$path.AddArc(0, 0, $d, $d, 180, 90)
$path.AddArc($w - $d, 0, $d, $d, 270, 90)
$path.AddArc($w - $d, $h - $d, $d, $d, 0, 90)
$path.AddArc(0, $h - $d, $d, $d, 90, 90)
$path.CloseFigure()

$brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 255, 255))
$g.FillPath($brush, $path)
$pen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(200, 255, 255, 255), 2)
$g.DrawPath($pen, $path)
$g.Dispose(); $brush.Dispose(); $pen.Dispose(); $path.Dispose()
Save-Bmp $bmp "UI_RoundedRect_64.png"

# 2. UI_Circle_Button.png (256x256 circular combat button)
$bmp = New-Object System.Drawing.Bitmap(256, 256)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::Transparent)

$brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 255, 255))
$g.FillEllipse($brush, 4, 4, 247, 247)
$pen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(220, 255, 255, 255), 5)
$g.DrawEllipse($pen, 6, 6, 243, 243)
$penInner = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(120, 255, 255, 255), 2)
$g.DrawEllipse($penInner, 18, 18, 219, 219)
$g.Dispose(); $brush.Dispose(); $pen.Dispose(); $penInner.Dispose()
Save-Bmp $bmp "UI_Circle_Button.png"

# 3. UI_Coin_Icon.png (128x128 gold coin with embossed star)
$bmp = New-Object System.Drawing.Bitmap(128, 128)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::Transparent)

$goldOuter = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 230, 160, 20))
$goldInner = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 215, 60))
$goldDark = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 180, 110, 10))

$g.FillEllipse($goldOuter, 6, 6, 115, 115)
$g.FillEllipse($goldInner, 14, 14, 99, 99)

# Draw central star
$starPath = New-Object System.Drawing.Drawing2D.GraphicsPath
$cx = 64; $cy = 64; $rOuter = 32; $rInner = 14
$pts = @()
for ($i = 0; $i -lt 10; $i++) {
    $angle = $i * [Math]::PI / 5 - [Math]::PI / 2
    $dist = if ($i % 2 -eq 0) { $rOuter } else { $rInner }
    $x = $cx + $dist * [Math]::Cos($angle)
    $y = $cy + $dist * [Math]::Sin($angle)
    $pts += New-Object System.Drawing.PointF($x, $y)
}
$g.FillPolygon($goldDark, $pts)

$pen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 255, 240, 150), 3)
$g.DrawEllipse($pen, 14, 14, 99, 99)
$g.Dispose(); $goldOuter.Dispose(); $goldInner.Dispose(); $goldDark.Dispose(); $pen.Dispose()
Save-Bmp $bmp "UI_Coin_Icon.png"

# 4. UI_Crosshair_Icon.png (128x128 white crosshair)
$bmp = New-Object System.Drawing.Bitmap(128, 128)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.Clear([System.Drawing.Color]::Transparent)

$pen = New-Object System.Drawing.Pen([System.Drawing.Color]::White, 5)
$g.DrawEllipse($pen, 28, 28, 71, 71)
$g.DrawLine($pen, 64, 12, 64, 40)
$g.DrawLine($pen, 64, 88, 64, 116)
$g.DrawLine($pen, 12, 64, 40, 64)
$g.DrawLine($pen, 88, 64, 116, 64)
$brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
$g.FillEllipse($brush, 59, 59, 10, 10)
$g.Dispose(); $pen.Dispose(); $brush.Dispose()
Save-Bmp $bmp "UI_Crosshair_Icon.png"

# 5. UI_Arrow_Left.png & UI_Arrow_Right.png
function Draw-Chevron($name, $flip) {
    $b = New-Object System.Drawing.Bitmap(128, 128)
    $gr = [System.Drawing.Graphics]::FromImage($b)
    $gr.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $gr.Clear([System.Drawing.Color]::Transparent)

    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    if ($flip) {
        $p.AddLine(42, 28, 80, 64)
        $p.AddLine(80, 64, 42, 100)
    } else {
        $p.AddLine(86, 28, 48, 64)
        $p.AddLine(48, 64, 86, 100)
    }
    $pn = New-Object System.Drawing.Pen([System.Drawing.Color]::White, 12)
    $pn.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pn.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pn.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $gr.DrawPath($pn, $p)
    $gr.Dispose(); $pn.Dispose(); $p.Dispose()
    Save-Bmp $b $name
}

Draw-Chevron "UI_Arrow_Left.png" $false
Draw-Chevron "UI_Arrow_Right.png" $true
