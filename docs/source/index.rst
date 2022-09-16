.. Posh Predictive Text 

====================
Posh Predictive Text
====================

*PowerShell predictive text for popular command line tools.*

Productive messaging on the mobile phone arrived with the introduction of predictive text. 
The reduction in time needed to compose and send a message increases efficiency and allows
the user to return to other activities more quickly.

Over time, the complexity of command line tools has increased. For new users the number of
parameters and values can be overwhelming and for existing users entering the same commands
can be repetitive and frustrating when small typing errors are made. Posh Predictive Text
addresses these problems by suggesting text completions for parameters and, in some cases,
parameter values.

Quick Start
-----------

Posh Predictive Text is available from PowerShell Gallery and is installed and activated 
using the following instructions. A more detailed set of instructions are available on the
installation page.

Prerequisites
^^^^^^^^^^^^^

Posh predictive text requires PowerShell version 7.2 or greater and PSReadLine version 2.2.6
or greater.


Download and install PowerShell module
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Install PoshPredictive text from the PowerShell Gallery. This will download the module to
the local user account. You may be asked for permission if you have not already set PowerShell
gallery as trusted source.

.. code-block:: PowerShell
   
   Install-Module -name PoshPredictiveText


Update PowerShell profiles
^^^^^^^^^^^^^^^^^^^^^^^^^^

Add the following commands to the PowerShell profile. To locate the PowerShell profile open
a command prompt and type `$PROFILE`.

.. code-block:: PowerShell

   Set-PredictiveTextOption -RemoveCondaTabExpansion
   Install-PredictiveText




.. toctree::
   :maxdepth: 3
   :caption: Contents:
   :hidden:

   pages/installation
   pages/PowerShellHelp
   pages/development
   pages/upgrade-changelog
