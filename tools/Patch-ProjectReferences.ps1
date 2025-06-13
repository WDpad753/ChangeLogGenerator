$pattern1 = '..\..\BaseClassLibrary\BaseClass\BaseClass.csproj'
$pattern2 = '..\..\BaseClassLibrary\Common.Abstractions\Common.Abstractions.csproj'
# $pattern3 = '..\..\BaseClassLibrary\BaseLogger\BaseLogger.csproj'

$replace1 = '..\BaseClassLibrary\BaseClass\BaseClass.csproj'
$replace2 = '..\BaseClassLibrary\Common.Abstractions\Common.Abstractions.csproj'
#$replace3 = '..\BaseClassLibrary\BaseLogger\BaseLogger.csproj'

$csprojFiles = Get-ChildItem -Recurse -Filter *.csproj

foreach ($file in $csprojFiles) {
    $content = Get-Content $file.FullName -Raw
    $patched = $content.Replace($pattern1, $replace1).Replace($pattern2, $replace2)

    if ($patched -ne $content) {
        Write-Host "Patching: $($file.FullName)"
        Set-Content $file.FullName $patched
    }
}