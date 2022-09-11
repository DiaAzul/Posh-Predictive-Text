# Script to import and install the debug version of the module.

Import-Module ".\PoshPredictiveText\bin\Debug\net6.0\PoshPredictiveText.dll"
Set-PredictiveTextOption -LogFile "C:\templogfiles\logfile.txt" -LogLevel "INFO"
Set-PredictiveTextOption -RemoveCondaTabExpansion
Install-PredictiveText 
