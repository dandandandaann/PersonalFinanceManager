Get-ChildItem -Recurse -Filter deploy_function.ps1 | ForEach-Object {
    Write-Host "`nRunning: $($_.FullName)`n" -ForegroundColor Cyan
    & $_.FullName
}
Read-Host -Prompt "All done. Press Enter to exit"
