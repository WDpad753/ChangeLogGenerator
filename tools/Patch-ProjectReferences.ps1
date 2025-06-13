$pattern1 = '..\..\BaseClassLibrary\BaseClass\BaseClass.csproj'
$pattern2 = '..\..\BaseClassLibrary\Common.Abstractions\Common.Abstractions.csproj'
$pattern3 = '..\..\BaseClassLibrary\BaseLogger\BaseLogger.csproj'

$replace1 = '..\BaseClassLibrary\BaseClass\BaseClass.csproj'
$replace2 = '..\BaseClassLibrary\Common.Abstractions\Common.Abstractions.csproj'
$replace3 = '..\BaseClassLibrary\BaseLogger\BaseLogger.csproj'

$csprojFiles = Get-ChildItem -Recurse -Filter *.csproj

foreach ($file in $csprojFiles) {
    $content = Get-Content $file.FullName -Raw
    $patched = $content.Replace($pattern1, $replace1).Replace($pattern2, $replace2).Replace($pattern3, $replace3)

    if ($patched -ne $content) {
        Write-Host "Patching: $($file.FullName)"
        Write-Host "`nUpdated content for: $($file.FullName)"
        Write-Host $patched
        Set-Content $file.FullName $patched
    }
}