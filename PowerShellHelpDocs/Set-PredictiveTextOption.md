---
external help file: PoshPredictiveText.dll-Help.xml
Module Name: PoshPredictiveText
online version:
schema: 2.0.0
---

# Set-PredictiveTextOption

## SYNOPSIS

Sets options for Posh Predictive Text.

## SYNTAX

### Logging

```
Set-PredictiveTextOption -LogFile <String> [-LogLevel <String>] [<CommonParameters>]
```

### RemoveCondaTabExpansion

```
Set-PredictiveTextOption [-RemoveCondaTabExpansion] [<CommonParameters>]
```

## DESCRIPTION

Sets options for Posh Predictive Text.

## EXAMPLES

### Example 1

```powershell
Set-PredictiveTextOption -RemoveCondaTabExpansion
```

Conda installs its own tab-expansion within PowerShell. The conda code has higher precedence and
prevents Posh Predictive Text providing completions for conda commands. This command removes the
conda code and allows Posh Predictive Text to provide completions for conda.

### Example 2

```powershell
Set-PredictiveTextOption -LogFile 'C:\logiles\logfile.txt' -LogLevel INFO
```

Enables logging of diagnostic messages to a file. Messages are recorded against three levels: ERROR,
WARN, INFO.

## PARAMETERS

### -LogFile

Enable logging to log file.

```yaml
Type: String
Parameter Sets: Logging
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LogLevel

Level of information to log (INFO, WARN, ERROR).
Default: ERROR.

```yaml
Type: String
Parameter Sets: Logging
Aliases:
Accepted values: INFO, WARN, ERROR

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RemoveCondaTabExpansion

Remove conda installed tab expansion.

```yaml
Type: SwitchParameter
Parameter Sets: RemoveCondaTabExpansion
Aliases:

Required: False
Position: Named
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

## RELATED LINKS

[https://github.com/DiaAzul/Posh-Predictive-Text](https://github.com/DiaAzul/Posh-Predictive-Text)
