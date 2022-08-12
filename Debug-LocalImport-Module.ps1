# Script to import and install the debug version of the module.

Import-Module ".\Resolve-Argument\bin\Debug\net6.0\Resolve-Argument.dll"
Resolve-Argument -Initialise -LogFile "C:\templogfiles\logfile.txt" -LogLevel "ERROR" | Invoke-Expression

