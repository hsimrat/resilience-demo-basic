# Fails if 'new HttpClient(' appears outside bin/obj
$hits = Get-ChildItem -Recurse -Include *.cs -Exclude bin,obj |
    Select-String -Pattern 'new\s+HttpClient\s*\('
if ($hits) {
    Write-Host "❌ Do not 'new HttpClient'. Use IHttpClientFactory."
    $hits | Format-Table Path, LineNumber, Line -AutoSize
    exit 1
}
Write-Host "✅ No raw HttpClient usages found."
