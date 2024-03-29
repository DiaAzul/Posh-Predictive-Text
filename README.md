[![Build status](https://ci.appveyor.com/api/projects/status/ei77wk9mkauty0j2/branch/main?svg=true)](https://ci.appveyor.com/project/DiaAzul/posh-predictive-text/branch/main)
[![Documentation Status](https://readthedocs.org/projects/posh-predictive-text/badge/?version=latest)](https://posh-predictive-text.readthedocs.io/en/latest/?badge=latest)

# Posh Predictive Text

<img src="./Assets/PoshPredictiveTextPlain.png"
     alt="Powershell predictive text logo"
     width=128
     align="right"/>

> PowerShell module providing predictive text completions for common CLI tools.

Most people will be familiar with predictive text on mobile phones.
Posh-predictive-text brings the same capability to the PowerShell command line
interface for common CLI tools used within the software development and data
science community.

Modern command line tools are highly configurable and have many parameters. It
is difficult to remember them all. Posh Predictive Text improves productivity,
not only by providing suggested completions, but also providing a pop-up list
of suggestions with tooltips. These tooltips remove the need to break off from
enter the command and search documentation for the correct parameter name.

In addition, Posh Predictive Text is also able to suggest a limited number of
parameter arguments. For instance when using conda to activate an environment
a list of environments is shown.

PoshPredictiveText provides suggested completions in the following situations:

- When a partial argument is entered and the `tab` key is pressed. Successive suggestion 
appear each time the `tab` key is pressed.
- Pressing `ctrl` + `space` displays a popup list of options with tooltips.
- If PSReadLine options `-PredictionSource` is set to use the plugin and `-PredictionViewStyle`
is set to `ListView` then suggestions will appear below the command line.

<https://user-images.githubusercontent.com/28156158/190742185-8842a5cf-3cc5-4945-8571-66d9ad6b4e6e.mp4>

## Supported Command Line Tools

Predictive text is available for the following commands line tools.

- conda

If a tools is not supported then please consider helping by developing the syntax tree file
needed to support it. Further information is available in the developer documentation.

## Sponsorship

We all love open-source software. It's the freedom to use it and adapt it as you like. Its the no
cost option. However, without funding open-source projects die. If you find this software useful
then consider sponsoring the developers. It doesn't have to be a lot. The cost of a cup of coffee
will do. Little contributions help and it is all very much appreciated.

GitHub Sponsors:

Please use the button at the top of the page to contribute via GitHub sponsors.

Ko-fi:

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/R5R0EC7N8)

## Installing / Getting started

PoshPredictiveText requires PowerShell version `7.2` or greater, and PSReadLine version `2.6` or
greater.

You can check the version of PowerShell using the following command.

```powershell
$PSVersionTable.PSVersion
```

The latest version of PowerShell is available on [Github](https://github.com/PowerShell/PowerShell).

You can check the version of PSReadLine using the following command.

```powershell
Get-Module PSReadLine | Format-List
```

Version `2.2.6` is pre-release and to install it the `AllowPreRelease` and `-force` options are required.

```powershell
Install-Module PSReadLine -AllowPrerelease -Force
```

Install PoshPredictiveText from the [PowerShell Gallery](https://www.powershellgallery.com/packages/PoshPredictiveText).
This will download the module to the local user account. You may be asked for permission
if you have not already set PowerShell gallery as trusted source.

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

Completions will appear when a partial argument is entered and the tab key is pressed. A longer
list of options with tooltips is available by pressing ctrl-space.

It is recommended to add the following PSReadLine options in the PowerShell profile so that
suggestions appear below the command line as command arguments are entered.

```powershell
Set-PSReadLineOption -PredictionSource HistoryAndPlugin -PredictionViewStyle ListView
```

## Contributing

Contributions to the project are welcome. No developer is an island, there is never enough time
to do everything and there is always another way to do things which may be better.

If you have a problem then raise an issue on the project issue tracker. If you have a suggested
code improvement then submit a pull request.

If you wish to add support for another command line tool then read the [developer documentation
on the project web site](https://posh-predictive-text.readthedocs.io/en/latest/pages/development.html).
This will explain how the syntax tree files are developed, processed and integrated into the tool.

## Links

- Project homepage: <https://posh-predictive-text.readthedocs.io>
- Repository: <https://github.com/DiaAzul/Posh-Predictive-Text>
- PowerShell Gallery: <https://www.powershellgallery.com/packages/PoshPredictiveText>
- Issue tracker: <https://github.com/DiaAzul/Posh-Predictive-Text/issues>
  - In case of sensitive bugs like security vulnerabilities, please contact
    me using the `contact owners` form listed under info on the PowerShell Gallery page
    for the module <https://www.powershellgallery.com/packages/PoshPredictiveText>.

## Licensing

The code in this project is licensed under the Apache-2.0 license.

The name Posh Predictive Text and associated trade marks are the property of Tanzo Creative Ltd.
