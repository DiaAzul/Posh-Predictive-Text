namespace PoshPredictiveText.Test
{
    using Xunit;

    /// <summary>
    /// Tests logging functionality
    /// </summary>
    public class LoggerTests : IDisposable
    {
        private readonly string tempLoggingFile;

        /// <summary>
        /// Setup a temporary logging file.
        /// </summary>
        public LoggerTests()
        {
            string path = Path.GetTempPath();
            string fileName = Path.ChangeExtension(Guid.NewGuid().ToString(), "txt");
            tempLoggingFile = Path.Combine(path, fileName);
        }

        /// <summary>
        /// Ensure temporary file is cleaned up after test.
        /// </summary>
        public void Dispose()
        {
            LOGGER.DeleteLogFile();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Test that we can write a message to the output file.
        /// </summary>
        [Fact]
        public void LoggingInitialisationTest()
        {
            // Arrange
            string logLevelInfo = "INFO";

            // Act
            LOGGER.Initialise(tempLoggingFile, logLevelInfo);

            // Assert
            Assert.True(File.Exists(tempLoggingFile));
        }

        /// <summary>
        /// Test throws exception is null path to log file.
        /// </summary>
        [Fact]
        public void NullPathToLoggerFile()
        {
            Assert.Throws<LoggerException>(() => LOGGER.Initialise("", "INFO"));
        }

        /// <summary>
        /// Test throws exception when path to log file is empty.
        /// </summary>
        [Fact]
        public void EmptyPathToLoggerFile()
        {
            string unlikelyPath = @"\myFictituous52345342Path\";
            Assert.Throws<LoggerException>(() => LOGGER.Initialise(unlikelyPath, "INFO"));
        }

        /// <summary>
        /// Test default logging level set to ERROR.
        /// </summary>
        [Fact]
        public void DefaultEnumerationLevelTest()
        {
            LOGGER.Initialise(tempLoggingFile, null);
            var result = LOGGER.logLevel;
            Assert.Equal(LOGGER.LOGLEVEL.ERROR, result);

        }
        /// <summary>
        /// Test the INFO enumeration of logging levels.
        /// </summary>
        [Fact]
        public void InfoEnumerationLevelTest()
        {
            LOGGER.Initialise(tempLoggingFile, "INFO");
            var result = LOGGER.logLevel;
            Assert.Equal(LOGGER.LOGLEVEL.INFO, result);
        }

        /// <summary>
        /// Test the WARN enumeration of logging levels.
        /// </summary>
        [Fact]
        public void WarnEnumerationLevelTest()
        {
            LOGGER.Initialise(tempLoggingFile, "WARN");
            var result = LOGGER.logLevel;
            Assert.Equal(LOGGER.LOGLEVEL.WARN, result);
        }

        /// <summary>
        /// Test the ERROR enumeration of logging levels.
        /// </summary>
        [Fact]
        public void ErrorEnumerationLevelTest()
        {
            LOGGER.Initialise(tempLoggingFile, "ERROR");
            var result = LOGGER.logLevel;
            Assert.Equal(LOGGER.LOGLEVEL.ERROR, result);
        }

        /// <summary>
        /// Test a successful write to log file.
        /// </summary>
        [Fact]
        public void WriteToLogFile()
        {
            LOGGER.Initialise(tempLoggingFile, "INFO");
            var startLineCount = File.ReadLines(tempLoggingFile).Count();
            LOGGER.Write("Message.");
            var endLineCount = File.ReadLines(tempLoggingFile).Count();

            Assert.Equal(1, endLineCount - startLineCount);
        }

        /// <summary>
        /// Test a write when log file doesn't exist.
        /// </summary>
        [Fact]
        public void WriteToNullLogFile()
        {
            LOGGER.Write("Message.");
        }
    }
}
