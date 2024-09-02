param(
    [string]$indexHtmlPath,
    [string]$newBaseUrl
)

$content = Get-Content $indexHtmlPath -Raw
$newContent = $content -replace '<base href=".*?"\s*/>', "<base href=`"$newBaseUrl`" />"
$newContent | Set-Content $indexHtmlPath -NoNewline
Write-Host "Base URL in index.html has been updated to $newBaseUrl"
