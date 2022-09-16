# Posh Predictive Text
<img src="./Assets/PoshPredictiveText%20Plain.png"
     alt="Powershell predictive text logo"
     width=128
     align="right"/>

> PowerShell module providing predictive text completions for common CLI tools.

Most people will be familiar with predictive text on mobile phones. Posh-predictive-text
brings the same capability to the PowerShell command line interface for common CLI tools
used within the software development and data science community.

Predictive text is available for the following commands line tools.

- conda

## Sponsorship

t.b.a

## Installing / Getting started

Install PoshPredictive text from the PowerShell Gallery. This will download the module to
the local user account. You may be asked for permission if you have not already set PowerShell
gallery as trusted source.

```shell
Install-Module -name PoshPredictiveText
```

Add the following commands to the PowerShell profile. To locate the PowerShell profile open
a command prompt and type `$PROFILE`.

```powershell
Set-PredictiveTextOption -RemoveCondaTabExpansion
Install-PredictiveText
```

The first command removes tab-expansion that is already installed by conda, and which prevents
Posh Predictive Text from providing completions. The second command installs Posh Predictive
Text.

## Contributing

Contributions to the project are welcome. No developer is an island, there is never enough time
to do everything and there is always another way to do things which may be better.

If you have a problem then raise an issue on the project issue tracker. If you have a suggested
code improvement then submit a pull request.

If you wish to add support for another command line tool then read the [developer documentation
on the project web site](https://posh-predictive-text.readthedocs.io/en/latest/pages/development.html).
This will explain how the syntax tree files are developed, processed and integrated into the tool.

## Links

Even though this information can be found inside the project on machine-readable
format like in a .json file, it's good to include a summary of most useful
links to humans using your project. You can include links like:

- Project homepage: <https://posh-predictive-text.readthedocs.io/en/latest/>
- Repository: <https://github.com/DiaAzul/Posh-Predictive-Text>
- Issue tracker: <https://github.com/DiaAzul/Posh-Predictive-Text/issues>
  - In case of sensitive bugs like security vulnerabilities, please contact
    me using the contact form on PowerShell Gallery (LINK TODO).

## Licensing

The code in this project is licensed under the Apache-2.0 license.

The name Posh Predictive Text and associated trade marks are the property of Tanzo Creative Ltd.
