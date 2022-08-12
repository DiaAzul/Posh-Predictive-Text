
namespace ResolveArgument
{
    using System.Reflection;

    internal class Tools
    {    
        internal static void WriteResourcesToLog()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();
            for (int i = 0; i < resourceNames.Length; i++)
            {
                LOGGER.Write($"Resource: {resourceNames[i]}");
            }
        }
    }

    /// <summary>
    /// Custom logger exception propagated up to the root of the cmdlet for outputing
    /// to the error stream.
    /// </summary>
    internal class LoggerException : Exception
    {
        internal LoggerException()
        {
        }

        internal LoggerException(string message)
            : base(message)
        {
        }

        internal LoggerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Print log messages with timestamp to file.
    /// </summary>
    internal static class LOGGER
    {
        internal enum LOGLEVEL : int { INFO = 0, WARN = 1, ERROR = 2 };

        internal static LOGLEVEL logLevel = LOGLEVEL.ERROR;
        internal static string? logFile = null;

        /// <summary>
        /// Initialises and enables the logfile for reporting information and errors.
        /// </summary>
        /// <param name="requesteLogFile">Path to the log file.</param>
        /// <param name="requestedLogLevel">Level at which logging required.</param>
        /// <exception cref="LoggerException">Exception thrown to output messages on the cmdlet Error stream.</exception>
        internal static void Initialise(string? requesteLogFile, string? requestedLogLevel)
        {
            if (requesteLogFile != null)
            {
                try
                {
                    string validatedPath = Path.GetFullPath(requesteLogFile)??"";
                    if (string.IsNullOrEmpty(validatedPath))
                    {
                        var errorText = Resolve_Argument.UIStrings.LOGGER_NOT_VALID_PATH + ": " + requesteLogFile;
                        throw new LoggerException(errorText);
                    }
                    if (!Directory.Exists(Path.GetDirectoryName(validatedPath)))
                    {
                        var errorText = Resolve_Argument.UIStrings.LOGGER_NO_DIRECTORY + ": " + Path.GetDirectoryName(validatedPath);
                        throw new LoggerException(errorText);
                    }

                    // If the file doesn't exist then create it.
                    if (!File.Exists(validatedPath))
                    {
                        string timestamp = DateTime.Now.ToString("s");
                        string outputText = $"[{timestamp}] {Resolve_Argument.UIStrings.LOGFILE_CREATED_HEADER}";
                        using StreamWriter sw = File.CreateText(validatedPath);
                        sw.WriteLine(outputText);
                    }

                    logFile = validatedPath;
                }
                catch (Exception ex) when (
                ex is ArgumentException
                || ex is System.Security.SecurityException
                || ex is ArgumentNullException
                || ex is NotSupportedException
                || ex is PathTooLongException
                )
                {
                    logFile = null;
                    throw new LoggerException(Resolve_Argument.UIStrings.LOGGER_NOT_VALID_PATH, ex);
                }

                if (requestedLogLevel != null)
                {
                    object? enumeratedLogLevel = null;
                    try
                    {
                        enumeratedLogLevel = Enum.Parse(typeof(LOGLEVEL), requestedLogLevel, true);
                    }
                    catch (Exception ex) when (
                    ex is ArgumentNullException
                    || ex is ArgumentException
                    || ex is OverflowException
                    )
                    {
                        throw new LoggerException(Resolve_Argument.UIStrings.LOGGER_NOT_VALID_LEVEL, ex);
                    }
                    finally
                    {
                        enumeratedLogLevel ??= LOGLEVEL.ERROR;
                        logLevel = (LOGLEVEL)enumeratedLogLevel;
                    }
                }

                Write($"Log level {logLevel}.");
            }
        }

        internal static void Write(string text, LOGLEVEL level = LOGLEVEL.INFO)
        {
            if (logFile != null & (int)level >= (int)logLevel)
            {
                string timestamp = DateTime.Now.ToString("s");
                string outputText = $"[{timestamp}] {text}";

                if (!File.Exists(logFile))
                {
                    if (Directory.Exists(logFile))
                    {
                        using StreamWriter sw = File.CreateText(logFile);
                        sw.WriteLine(outputText);
                    }
                }
                else
                {
                    using StreamWriter sw = File.AppendText(logFile);
                    sw.WriteLine(outputText);
                }
            }
        }
    }
}
