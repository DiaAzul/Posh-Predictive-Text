# Argument Completer Notes

<https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/register-argumentcompleter?view=powershell-5.1>

Parameters for PowerShell argument completer scriptBlock:

- `$commandName` (Position 0) - This parameter is set to the name of the command for which the script block is providing tab completion.
- `$parameterName` (Position 1) - This parameter is set to the parameter whose value requires tab completion.
- `$wordToComplete` (Position 2) - This parameter is set to value the user has provided before they pressed Tab. Your script block should use this value to determine tab completion values.
- `$commandAst` (Position 3) - This parameter is set to the Abstract Syntax Tree (AST) for the current input line.
- `$fakeBoundParameters` (Position 4) - This parameter is set to a hashtable containing the $PSBoundParameters for the cmdlet, before the user pressed Tab.

Completion results
<https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.completionresult?view=powershellsdk-7.0.0>

Completion results returned as a PowerShell CompletionResults object with properties:

- **CompletionText**: Gets the text to be used as the auto completion result
- **ListItemText**: Gets the text to be displayed in a list
- **ResultType**: Gets the type of completion result
- **ToolTip**: Gets the text for the tooltip with details to be displayed about the object

Example:

| CompletionText     |ListItemText        | ResultType     | ToolTip             |
|--------------------|--------------------|----------------|---------------------|
| AarSvc_65d7eb8     | AarSvc_65d7eb8     | ParameterValue | AarSvc_65d7eb8      |
| AdobeARMservice    | AdobeARMservice    | ParameterValue | AdobeARMservice     |
| AdobeUpdateService | AdobeUpdateService | ParameterValue | AdobeUpdateService  |
| AGMService         | AGMService         | ParameterValue | AGMService          |
| AGSService         | AGSService         | ParameterValue | AGSService          |
| Appinfo            | Appinfo            | ParameterValue | Appinfo             |
| asComSvc           | asComSvc           | ParameterValue | asComSvc            |
| AsSysCtrlService   | AsSysCtrlService   | ParameterValue | AsSysCtrlService    |
| AsusCertService    | AsusCertService    | ParameterValue | AsusCertService     |
