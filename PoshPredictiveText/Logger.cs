
using static PoshPredictiveText.LOGGER;

namespace PoshPredictiveText
{
    /// <summary>
    /// Custom logger exception propagated up to the root of the cmdlet for outputing
    /// to the error stream.
    /// </summary>
    internal class LoggerException : Exception
    {
        internal LoggerException() { }

        internal LoggerException(string message)
            : base(message) { }

        internal LoggerException(string message, Exception inner)
            : base(message, inner) { }
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
            if (requesteLogFile is not null)
            {
                try
                {
                    string validatedPath = Path.GetFullPath(requesteLogFile)??"";
                    if (string.IsNullOrEmpty(validatedPath))
                    {
                        var errorText = UIstring.Resource("LOGGER_NOT_VALID_PATH") + ": " + requesteLogFile;
                        throw new LoggerException(errorText);
                    }
                    if (!Directory.Exists(Path.GetDirectoryName(validatedPath)))
                    {
                        var errorText = UIstring.Resource("LOGGER_NO_DIRECTORY") + ": " + Path.GetDirectoryName(validatedPath);
                        throw new LoggerException(errorText);
                    }

                    // If the file doesn't exist then create it.
                    if (!File.Exists(validatedPath))
                    {
                        string timestamp = DateTime.Now.ToString("s");
                        string outputText = $"[{timestamp}] {UIstring.Resource("LOGFILE_CREATED_HEADER")}";
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
                    throw new LoggerException(UIstring.Resource("LOGGER_NOT_VALID_PATH"), ex);
                }

                if (requestedLogLevel is not null)
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
                        throw new LoggerException(UIstring.Resource("LOGGER_NOT_VALID_LEVEL"), ex);
                    }
                    finally
                    {
                        enumeratedLogLevel ??= LOGLEVEL.ERROR;
                        logLevel = (LOGLEVEL)enumeratedLogLevel;
                    }
                }
                else
                {
                    logLevel = LOGLEVEL.ERROR;
                }
                Write($"Log level {logLevel}.");
            }
        }

        internal static void Write(string text, LOGLEVEL level = LOGLEVEL.INFO)
        {
            if (logFile is not null && (int)level >= (int)logLevel)
            {
                string timestamp = DateTime.Now.ToString("s");
                string outputText = $"[{timestamp}] {text}";

                if (!File.Exists(logFile))
                {
                    if (Directory.Exists(logFile))
                    {
                        try
                        {
                            using StreamWriter sw = File.CreateText(logFile);
                            sw.WriteLine(outputText);
                        }
                        // IOException is thrown when the command line application throws an error and the callback
                        // into Predictor.OnCommandLineExecuted comes from PSReadLine originates from a second process
                        // which tries to write to the logfile whilst it is still open in the first.
                        // TODO [LOW][LOGGER] Write thread safe logger implementation.
                        catch (IOException) { }
                    }
                }
                else
                {
                    try
                    {
                        using StreamWriter sw = File.AppendText(logFile);
                        sw.WriteLine(outputText);
                    }
                    catch (IOException) { }
                }
            }
        }

        internal static void DeleteLogFile()
        {
            if (File.Exists(logFile))
            {
                try
                {
                    File.Delete(logFile);
                }
                catch { }
            }
            logFile = null;
            logLevel = LOGLEVEL.ERROR;
        }
    }
}
