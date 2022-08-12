# Script to import and install the release version of the module.

Import-Module ".\Resolve-Argument\bin\Release\net6.0\Resolve-Argument.dll"
Resolve-Argument -PrintScript | Invoke-Expression
