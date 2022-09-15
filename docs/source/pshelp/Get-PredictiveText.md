---
external help file: PoshPredictiveText.dll-Help.xml
Module Name: PoshPredictiveText
online version:
schema: 2.0.0
---

# Get-PredictiveText

## SYNOPSIS

Gets a list of suggested completions for command line arguments.

## SYNTAX

```powershell
Get-PredictiveText [[-WordToComplete] <String>] [[-CommandAst] <CommandAst>] [[-CursorPosition] <Int32>]
 [<CommonParameters>]
```

## DESCRIPTION

Native argument completer used to provide tab-expansion suggestions on the PowerShell command line.

A list of supported commands is provided by `Get-PredictiveTextOption -ListCommands`.

## EXAMPLES

### Example 1

```powershell
Register-ArgumentCompleter -CommandName $cmdNames -Native -ScriptBlock {
    param(
        [string]$wordToComplete, 
        [System.Management.Automation.Language.CommandAst]$commandAst,
        [int]$cursorPosition)

    try {
        $suggestions = Install-PredictiveText -WordToComplete $wordToComplete -CommandAst $commandAst -CursorPosition $cursorPosition
    }
    catch {
        Write-Host "Error."
    }
    
    $suggestions 
}

```

Registers `Install-PredicitiveText` as a native argument completer for the commands listed in `$cmdNames`.

## PARAMETERS

### -CommandAst

Abstract Syntax Tree for current input line.

```yaml
Type: CommandAst
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CursorPosition

Command enered by user at the prompt.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WordToComplete

Value provided by the user before they pressed tab.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### System.String

### System.Management.Automation.CompletionResult

## NOTES

This cmdlet is intended for uses as a registered argument completer and is not intended for use as a stand-alone cmdlet.

## RELATED LINKS

[GitHub PoshPredictiveText](https://github.com/DiaAzul/Posh-Predictive-Text)
