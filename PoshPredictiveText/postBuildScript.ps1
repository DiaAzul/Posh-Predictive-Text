
param($solution, $project, $OutDir)

Write-Output "***** Starting Post Build Script. *****" 

Write-Host "Solution path: ${solution}"
Write-Host "Project path: ${project}"
Write-Host "Output path: ${OutDir}"

# Create module output directories if they do not exist
$moduleRoot = Join-Path -path $solution -Childpath "Module/"
if(!(Test-Path -Path $moduleRoot )){
    $null = New-Item -ItemType directory -Path $moduleRoot
}
$moduleOutput = Join-Path -path $solution -Childpath "Module/PoshPredictiveText/"
if(!(Test-Path -Path $moduleOutput )){
    $null = New-Item -ItemType directory -Path $moduleOutput 
}
Write-Host "Module output: ${moduleOutput}"
# Clear previous build if there are files.
Remove-Item (Join-Path -path $moduleOutput -Childpath "*.*")

# Copy built artifacts to module folder
$buildDirectory = Join-Path -path $project -Childpath $OutDir
Write-Host "Build path: ${buildDirectory}"
Copy-Item (Join-Path -path $buildDirectory -Childpath "PoshPredictiveText.dll") -Destination $moduleOutput
Copy-Item (Join-Path -path $buildDirectory -Childpath "PoshPredictiveText.psd1") -Destination $moduleOutput

# Create help documentation
$documentationSource = join-path -path $solution -Childpath "PowerShellHelpDocs\"
Write-Host "Documentation source: ${documentationSource}"

Write-Host "Building documentation."
Install-Module -Name platyPS -Scope CurrentUser -force
Import-Module platyPS
$null = New-ExternalHelp -Path $documentationSource -OutputPath $moduleOutput -Force
Write-Host "Documentation complete."

# Test module manifest and whatif publishing (without key).
$testResults = Test-ModuleManifest (Join-Path -path $moduleOutput -Childpath "PoshPredictiveText.psd1")
$testResults.PSObject.Properties | ForEach-Object {
    $_.Name + " : " + $_.Value
}

# Output hint to publish module (without the NugetAPIKey).
Write-Host "Publish-Module -Path ""${moduleOutput}"" -NugetAPIKey """" -WhatIf -Verbose"

Write-Output "***** Post Build Script complete. *****"

