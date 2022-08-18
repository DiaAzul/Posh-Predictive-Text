# Script to import and install the debug version of the module.

Import-Module ".\Resolve-Argument\bin\Debug\net6.0\POSH_Jogger.dll"
Resolve-Argument -Initialise -LogFile "C:\templogfiles\logfile.txt" -LogLevel "INFO" | Invoke-Expression

