
param($Configuration, $solution, $project, $OutDir, $version)

# Exit script if we are not in release (reduce time in build-test-debug cycle)
if ($Configuration -ne "Release") {
    Write-Output "***** Skipping Post Build Script. *****"
    return
}

Write-Output "***** Starting Post Build Script. *****" 

Write-Host "Solution path: ${solution}"
Write-Host "Project path: ${project}"
Write-Host "Output path: ${OutDir}"

# Create module output directories if they do not exist
$moduleRoot = Join-Path $solution "Module"
if(!(Test-Path -Path $moduleRoot )){
    $null = New-Item -ItemType directory -Path $moduleRoot
}
$moduleOutput = Join-Path $moduleRoot "PoshPredictiveText"
if(!(Test-Path -Path $moduleOutput )){
    $null = New-Item -ItemType directory -Path $moduleOutput 
}
Write-Host "Module output: ${moduleOutput}"
# Clear previous build if there are files.
Remove-Item (Join-Path $moduleOutput "*.*")

# Copy built artifacts to module folder
$buildDirectory = Join-Path $project $OutDir
Write-Host "Build path: ${buildDirectory}"
Copy-Item (Join-Path $buildDirectory "PoshPredictiveText.dll") -Destination $moduleOutput

# Update version number when copying psd file.
$psdSourceFile = Join-Path $buildDirectory "AdditionalFiles" "PoshPredictiveText.psd1"
$psdDestinationFile = Join-Path $moduleOutput "PoshPredictiveText.psd1"
((Get-Content -path $psdSourceFile -Raw) -replace '!{version}', $version) | Set-Content -Path $psdDestinationFile

# Create PS Help documentation
$documentationSource = join-path $solution "PowerShellHelpDocs"
Write-Host "PS Help documentation source: ${documentationSource}"

Write-Host "Building PS Help documentation."
Install-Module -Name platyPS -Scope CurrentUser -force
Import-Module platyPS
$null = New-ExternalHelp -Path $documentationSource -OutputPath $moduleOutput -Force
Write-Host "PS Help Documentation complete."

# Test module manifest and whatif publishing (without key).
Write-Host "----- Test-Module Manifest. ----"
$testResults = Test-ModuleManifest (Join-Path $moduleOutput "PoshPredictiveText.psd1")
$testResults.PSObject.Properties | ForEach-Object {
    $_.Name + " : " + $_.Value
}
Write-Host "----- End Module Manifest. ----"

# Output hint to publish module (without the NugetAPIKey).
Write-Host "Publish-Module -Path ""${moduleOutput}"" -NugetAPIKey ""EnterKeyHere"" -WhatIf -Verbose"

Write-Output "***** Post Build Script complete. *****"

