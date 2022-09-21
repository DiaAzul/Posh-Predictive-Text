===============
Module Features
===============

The following options are applicable to the Posh Predictive Text module. 
Options specific to each command line tool are listed on the page
for each command.

List of Support CLI commands
--------------------------------

To get a list of CLI commands supported by Posh Predictive Text.

.. code-block:: powershell

    Get-PredictiveTextOption -ListCommands

Version
-------

To get the version of Posh Predictive Text

.. code-block:: powershell

    Get-PredictiveTextOption -Version


Initialisation Script
---------------------

When posh predictive text is installed it runs a PowerShell script to register an
argument completer. This script is executed by ``Install-PredictiveText`` and can
be printed using the following command so that the code can be reviewed.

.. code-block:: powershell

    Get-PredictiveTextOption -PrintScript

Logging
-------

**Logging is not recommended.**

The release version of Posh Predictive Text supports limited logging to a file to
facilitate problem resolution. To set up logging the following command identifies
the log file to which records are written and the level of logging required
(INFO, WARN, ERROR).

.. code-block:: powershell

    Set-PredictiveTextOption -LogFile C:\temp\logfile.txt -LogLevel INFO

The path to the log file must exist - folders will are not created automatically
and logging will not start if the log file cannot be created.
