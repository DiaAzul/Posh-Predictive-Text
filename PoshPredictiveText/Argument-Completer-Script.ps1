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
Register-ArgumentCompleter -CommandName $cmdNames -Native -ScriptBlock {
    param(
        [string]$wordToComplete, 
        [System.Management.Automation.Language.CommandAst]$commandAst,
        [int]$cursorPosition)

    try {
        $suggestions = Resolve-Argument -WordToComplete $wordToComplete -CommandAst $commandAst -CursorPosition $cursorPosition
    }
    catch {
        Write-Host "Error."
    }
    
    $suggestions 
}


