param([string]$OutputPath)
$ErrorActionPreference='Stop'
Add-Type -AssemblyName PresentationCore,PresentationFramework,WindowsBase
$frames=@()
foreach($size in @(16,24,32,48,64,256)) {
    $asset=if($size -lt 48){'logo-small.xaml'}else{'logo.xaml'}
    $canvas=[Windows.Markup.XamlReader]::Parse([IO.File]::ReadAllText((Join-Path $PSScriptRoot "assets/$asset")))
    $view=New-Object Windows.Controls.Viewbox
    $view.Width=$size;$view.Height=$size;$view.Child=$canvas
    $view.Measure((New-Object Windows.Size $size,$size))
    $view.Arrange((New-Object Windows.Rect 0,0,$size,$size));$view.UpdateLayout()
    $bitmap=New-Object Windows.Media.Imaging.RenderTargetBitmap $size,$size,96,96,([Windows.Media.PixelFormats]::Pbgra32)
    $bitmap.Render($view)
    $encoder=New-Object Windows.Media.Imaging.PngBitmapEncoder
    $encoder.Frames.Add([Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
    $stream=New-Object IO.MemoryStream;$encoder.Save($stream)
    $frames+=,@{Size=$size;Bytes=$stream.ToArray()};$stream.Dispose()
}
$file=[IO.File]::Create($OutputPath)
$writer=New-Object IO.BinaryWriter $file
try {
    $writer.Write([uint16]0);$writer.Write([uint16]1);$writer.Write([uint16]$frames.Count)
    $offset=6+16*$frames.Count
    foreach($frame in $frames) {
        $dimension=if($frame.Size -eq 256){0}else{$frame.Size}
        $writer.Write([byte]$dimension);$writer.Write([byte]$dimension);$writer.Write([byte]0);$writer.Write([byte]0)
        $writer.Write([uint16]1);$writer.Write([uint16]32);$writer.Write([uint32]$frame.Bytes.Length);$writer.Write([uint32]$offset)
        $offset+=$frame.Bytes.Length
    }
    foreach($frame in $frames){$writer.Write([byte[]]$frame.Bytes)}
} finally {$writer.Dispose();$file.Dispose()}
