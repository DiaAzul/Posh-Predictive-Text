# Script to import and install the release version of the module.

Import-Module ".\PoshPredictiveText\bin\Release\net6.0\PoshPredictiveText.dll"
Set-PredictiveTextOption -RemoveCondaTabExpansion
Install-PredictiveText 