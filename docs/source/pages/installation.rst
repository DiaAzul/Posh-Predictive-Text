==============================
Installation and configuration
==============================

Pre-requisites
^^^^^^^^^^^^^^

PoshPredictiveText requires PowerShell version ``7.2`` or greater, and PSReadLine version ``2.6`` or
greater.

You can check the version of PowerShell using the following command.

.. code-block:: powershell

    $PSVersionTable.PSVersion


The latest version of PowerShell is available on `Github`_.

.. _GitHub: https://github.com/PowerShell/PowerShell

You can check the version of PSReadLine using the following command.

.. code-block:: powershell

    Get-Module PSReadLine | Format-List


Version `2.2.6` is pre-release and to install it the `AllowPreRelease` and `-force` options are required.

.. code-block:: powershell

    Install-Module PSReadLine -AllowPrerelease -Force


Installing Posh Predictive Text
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Install PoshPredictiveText from the `PowerShell Gallery`_.
This will download the module to the local user account. You may be asked for permission
if you have not already set PowerShell gallery as trusted source.

.. _`PowerShell Gallery`: https://www.powershellgallery.com/packages/PoshPredictiveText

.. code-block:: powershell

    Install-Module -name PoshPredictiveText

Add the following commands to the PowerShell profile. To locate the PowerShell profile open
a command prompt and type `$PROFILE`.

.. code-block:: powershell

    Set-PredictiveTextOption -RemoveCondaTabExpansion
    Install-PredictiveText

The first command removes tab-expansion that is already installed by conda, and which prevents
Posh Predictive Text from providing completions. The second command installs Posh Predictive
Text.

Completions will appear when a partial argument is entered and the tab key is pressed. A longer
list of options with tooltips is available by pressing ctrl-space.

Configuring PSReadLine
^^^^^^^^^^^^^^^^^^^^^^

It is recommended to add the following PSReadLine options in the PowerShell profile so that
suggestions appear below the command line as command arguments are entered.

.. code-block:: powershell

    Set-PSReadLineOption -PredictionSource HistoryAndPlugin -PredictionVewStyle ListView

