
# Command Line Arguments

## The POSIX view of the world

Opinionated article
<https://nullprogram.com/blog/2020/08/01/>

same author, writing on parsing Windows command line (assume Command Prompt not PowerShell)
<https://nullprogram.com/blog/2022/02/18/>

POSIX.1-2017 defines a standard operating system interface and environment, including a command interpreter (or "shell"), and common utility programs to support applications portability at the source code level. It is intended to be used by both application developers and system implementors.
<https://pubs.opengroup.org/onlinepubs/9699919799/basedefs/V1_chap12.html>

<https://www.gnu.org/software/libc/manual/html_node/Argument-Syntax.html>

**25.1.1 Program Argument Syntax Conventions**

POSIX recommends these conventions for command line arguments. getopt (see Parsing program options using getopt) and argp_parse (see Parsing Program Options with Argp) make it easy to implement them.

- Arguments are options if they begin with a hyphen delimiter (‘-’).
- Multiple options may follow a hyphen delimiter in a single token if the options do not take arguments. Thus, ‘-abc’ is equivalent to ‘-a -b -c’.
- Option names are single alphanumeric characters (as for isalnum; see Classification of Characters).
- Certain options require an argument. For example, the -o option of the ld command requires an argument—an output file name.
- An option and its argument may or may not appear as separate tokens. (In other words, the whitespace separating them is optional.) Thus, -o foo and -ofoo are equivalent.
- Options typically precede other non-option arguments.

The implementations of getopt and argp_parse in the GNU C Library normally make it appear as if all the option arguments were specified before all the non-option arguments for the purposes of parsing, even if the user of your program intermixed option and non-option arguments. They do this by reordering the elements of the argv array. This behavior is nonstandard; if you want to suppress it, define the _POSIX_OPTION_ORDER environment variable. See Standard Environment Variables.

- The argument -- terminates all options; any following arguments are treated as non-option arguments, even if they begin with a hyphen.
- A token consisting of a single hyphen character is interpreted as an ordinary non-option argument. By convention, it is used to specify input from or output to the standard input and output streams.
- Options may be supplied in any order, or appear multiple times. The interpretation is left up to the particular application program. 

GNU adds long options to these conventions. Long options consist of -- followed by a name made of alphanumeric characters and dashes. Option names are typically one to three words long, with hyphens to separate words. Users can abbreviate the option names as long as the abbreviations are unique.

To specify an argument for a long option, write --name=value. This syntax enables a long option to accept an argument that is itself optional.

Eventually, GNU systems will provide completion for long option names in the shell. 



# Argument Completer Notes

<https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/register-argumentcompleter?view=powershell-5.1>

Parameters for PowerShell argument completer scriptBlock:

- `$commandName` (Position 0) - This parameter is set to the name of the command for which the script block is providing tab completion.
- `$parameterName` (Position 1) - This parameter is set to the parameter whose value requires tab completion.
- `$wordToComplete` (Position 2) - This parameter is set to value the user has provided before they pressed Tab. Your script block should use this value to determine tab completion values.
- `$commandAst` (Position 3) - This parameter is set to the Abstract Syntax Tree (AST) for the current input line.
- `$fakeBoundParameters` (Position 4) - This parameter is set to a hashtable containing the $PSBoundParameters for the cmdlet, before the user pressed Tab.

Completion results
<https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.completionresult?view=powershellsdk-7.0.0>

Completion results returned as a PowerShell CompletionResults object with properties:

- **CompletionText**: Gets the text to be used as the auto completion result
- **ListItemText**: Gets the text to be displayed in a list
- **ResultType**: Gets the type of completion result
- **ToolTip**: Gets the text for the tooltip with details to be displayed about the object

Example:

| CompletionText     |ListItemText        | ResultType     | ToolTip             |
|--------------------|--------------------|----------------|---------------------|
| AarSvc_65d7eb8     | AarSvc_65d7eb8     | ParameterValue | AarSvc_65d7eb8      |
| AdobeARMservice    | AdobeARMservice    | ParameterValue | AdobeARMservice     |
| AdobeUpdateService | AdobeUpdateService | ParameterValue | AdobeUpdateService  |
| AGMService         | AGMService         | ParameterValue | AGMService          |
| AGSService         | AGSService         | ParameterValue | AGSService          |
| Appinfo            | Appinfo            | ParameterValue | Appinfo             |
| asComSvc           | asComSvc           | ParameterValue | asComSvc            |
| AsSysCtrlService   | AsSysCtrlService   | ParameterValue | AsSysCtrlService    |
| AsusCertService    | AsusCertService    | ParameterValue | AsusCertService     |
