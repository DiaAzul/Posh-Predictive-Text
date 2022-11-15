# Remove conda tab-expansion if installed.
if (Test-Path Function:\TabExpansion) {
    $testForConda = Get-Item Function:\TabExpansion
    if ($testForConda.Source -eq "conda") {
        Remove-Item Function:\TabExpansion
        if (Test-Path Function:\CondaTabExpansionBackup) {
            Rename-Item Function:\CondaTabExpansionBackup Function:\TabExpansion
        }
    }
}

$callBack = @"
param(
    [string]`$wordToComplete,
    [System.Management.Automation.Language.CommandAst]`$commandAst,
    [int]`$cursorPosition)

try {
   `$suggestions = Get-PredictiveText -WordToComplete `$wordToComplete -CommandAst `$commandAst -CursorPosition `$cursorPosition
}
catch {
    Write-Error "PoshPredictiveText had an error resolving !cmdName!."
}
`$suggestions
"@

foreach($cmdName in $cmdNames) {
    $callBackScript = [scriptblock]::Create($callBack.Replace("!cmdName!", $cmdName))
    try {
        Register-ArgumentCompleter -CommandName $cmdName -Native -ScriptBlock $callBackScript
    }
    catch {
        Write-Error $"Unable to register argument completer for {$cmdName}"
    }
}
