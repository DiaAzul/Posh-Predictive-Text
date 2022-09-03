# Script to import and install the debug version of the module.

Import-Module ".\PoshPredictiveText\bin\Debug\net6.0\PoshPredictiveText.dll"
Install-PredictiveText -Initialise -LogFile "C:\templogfiles\logfile.txt" -LogLevel "INFO"
