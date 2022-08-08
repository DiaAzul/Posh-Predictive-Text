# The following script is included in the UIString resources and used to register commands.
$scriptBlock = {
    param ( $commandName,
            $parameterName,
            $wordToComplete,
            $commandAst,
            $fakeBoundParameters )

    $timestamp = Get-Date -Format HH:mm:ss
    "[$timestamp] $commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameters" | Out-File -Append "C:\workspace\csharp\POSH-Resolve-Argument\logfile.txt"
    Resolve-Argument ( $commandName,
            $parameterName,
            $wordToComplete,
            $commandAst,
            $fakeBoundParameters )
}
Register-ArgumentCompleter -CommandName $cmdNames -ScriptBlock $scriptBlock


Register-ArgumentCompleter -CommandName $cmdNames -Native -ScriptBlock {
    param(
        [string]$wordToComplete, 
        [System.Management.Automation.Language.CommandAst]$commandAst,
        [int]$cursorPosition)

    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    "[$timestamp] $cursorPosition, $wordToComplete, $commandAst" | Out-File -Append "C:\templogfiles\logfile.txt"
    $suggestions = Resolve-Argument -WordToComplete $wordToComplete -CommandAst $commandAst -CursorPosition $cursorPosition

    $suggestions | Out-File -Append "C:\templogfiles\logfile.txt"

    $suggestions 
}

Register-ArgumentCompleter -CommandName $cmdNames -Native -ScriptBlock {
    param(
        [string]$wordToComplete, 
        [System.Management.Automation.Language.CommandAst]$commandAst,
        [int]$cursorPosition)

    $suggestions = Resolve-Argument -WordToComplete $wordToComplete -CommandAst $commandAst -CursorPosition $cursorPosition

    $suggestions 
}