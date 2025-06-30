$pattern1 = '..\..\BaseClassLibrary\BaseClass\BaseClass.csproj'
$pattern2 = '..\..\BaseClassLibrary\Common.Abstraction\Common.Abstraction.csproj'
$pattern3 = '..\..\BaseClassLibrary\BaseLogger\BaseLogger.csproj'
$pattern4 = '..\..\TestAPI\TestAPI.csproj'

$replace1 = '..\BaseClassLibrary\BaseClass\BaseClass.csproj'
$replace2 = '..\BaseClassLibrary\Common.Abstraction\Common.Abstraction.csproj'
$replace3 = '..\BaseClassLibrary\BaseLogger\BaseLogger.csproj'
$replace4 = '..\TestAPI\TestAPI.csproj'

$csprojFiles = Get-ChildItem -Recurse -Filter *.csproj

foreach ($file in $csprojFiles) {
    $content = Get-Content $file.FullName -Raw
    $patched = $content.Replace($pattern1, $replace1).Replace($pattern2, $replace2).Replace($pattern3, $replace3).Replace($pattern4, $replace4)

    if ($patched -ne $content) {
        Write-Host "Patching: $($file.FullName)"
        Write-Host "`nUpdated content for: $($file.FullName)"
        Write-Host $patched
        Set-Content $file.FullName $patched
    }
}