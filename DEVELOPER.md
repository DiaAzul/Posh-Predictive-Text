
# Developer

PoshPredictiveText is developed using Visual Studio. The majority of the code is C# with a small amount of PowerShell scripting to register the native argument completer and remove conda tab-expansion code, if it is installed. 

## Coding Standards

Coding must follow Microsoft conventions <https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions>.

Additional guidance provided by Google is used where approrpriate <https://google.github.io/styleguide/csharp-style.html>. 

The following standards are used in the code which may conflict with the above:

- `var` should be used sparingly and only when the type is **very** obvious. Use of `var` can make code difficult to review if the type is not clear.
- Indentation is four spaces (no tabs).
- Don't use underscore to indicate private, protected, internal and protected internal fields. It adds noise.
- `const` are indicated in capital case. Though this should change to follow Google guidelines.
- Opening braces do start on a new line.
- Do not use braces for single statements where it improves readability, e.g. if statements follwed by return to exit a method early `if (x is null) return null;`.

## Testing

Testing uses Xunit, which should install from NuGet. Test coverage uses Fine Code Coverage, which is installed as an extension in Visual Studio.

Code should be covered with reasonable unit tests to validate that it works on multiple platforms.

All tests must pass before code is accepted.

## PowerShell Help Documents

PowerShell has traditionally used MAML XML files for console help. These are difficult to prepare by hand. As an alternative, help files can be written in Markdown and then exported in required formats using the PowerShell PlatyPS module. 

The PlatyPS module is described on GitHub <https://github.com/PowerShell/platyps>

The PlatyPS module must be downloaded and installed on the local development machine.

```PowerShell
Install-Module -Name platyPS -Scope CurrentUser
Import-Module platyPS
```

The base documentation is already created within the folder `PowerShellHelpDocs`.

To create the external helpfiles run the following command whilist in the `PowerShellHelpDocs` folder.

```PowerShell
New-ExternalHelp .\ -OutputPath en-US\
```

