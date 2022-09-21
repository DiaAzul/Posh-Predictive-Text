=====
Conda
=====

Conda is an open source package management system and environment management system.
Posh Predictive Text provides suggestions for all commands and parameters as well
as arguments listed below.


Suppress Built-in Conda Tab-Expansion
-------------------------------------

The script that initialises Conda in PowerShell installs a function providing limited
tab-expansion capabilities. Unfortunately, this is not an optional install and it also
prevents other tab-expansion providers working. To remove the Conda function from PowerShell
the following option must be set prior to installing Posh Predictive Text.

.. code-block:: PowerShell

   Set-PredictiveTextOption -RemoveCondaTabExpansion

If Conda removes the tab-expansion functionality from their code in future releases
then this option will not need setting.


Environments
------------

Posh Predictive Text suggests completions for available environments. For example:

.. code-block:: powershell

    conda activate b

Pressing ``tab`` will cycle through environments starting with the letter ``b``. Pressing
``ctrl-tab`` will provide a pop-up list of environments starting with the letter ``b``,
the toolTips will show the path to the environment. If there is a space after ``activate``
and ``ctrl-tab`` is pressed, then all environments will be shown.

The environments include both those defined using the ``--Name`` and the ``--Path`` parameters.

