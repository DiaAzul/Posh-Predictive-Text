
namespace PoshPredictiveText.Test
{
    using PoshPredictiveText.Cmdlets;
    using System.Management.Automation;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using Xunit;

    /// <summary>
    /// Create PowerShell shells for testing.
    /// </summary>
    public class PowerShellMock
    {
        /// <summary>
        /// Create the initial session state.
        /// </summary>
        /// <returns></returns>
        private static InitialSessionState ISS()
        {
            var sessionState = InitialSessionState.CreateDefault();

            // Create a permissive authorisation manager.
            sessionState.AuthorizationManager = new("Microsoft.PowerShell");

            string[] moduleDependencies = new string[]
            {
                "PSReadLine"
            };
            sessionState.ImportPSModule(moduleDependencies);

            // Add cmdlets
            SessionStateCmdletEntry cmdletToTestIPT = new("Install-PredictiveText", typeof(InstallPredictiveText), null);
            sessionState.Commands.Add(cmdletToTestIPT);
            SessionStateCmdletEntry cmdletToTestGPT = new("Get-PredictiveText", typeof(GetPredictiveText), null);
            sessionState.Commands.Add(cmdletToTestGPT);

            SessionStateCmdletEntry cmdletToTestGPTO = new("Get-PredictiveTextOption", typeof(GetPredictiveTextOption), null);
            sessionState.Commands.Add(cmdletToTestGPTO);
            SessionStateCmdletEntry cmdletToTestSPTO = new("Set-PredictiveTextOption", typeof(SetPredictiveTextOption), null);
            sessionState.Commands.Add(cmdletToTestSPTO);

            return sessionState;
        }

        /// <summary>
        /// Configures an instance of PowerShell containing the cmdlet to test.
        /// </summary>
        /// <returns>
        /// Instance of PowerShell containing cmdlet.
        /// </returns>
        public static PowerShell GetConfiguredShell()
        {
            InitialSessionState inititalSessionState = ISS();
            //RunspacePool pool = RunspaceFactory.CreateRunspacePool(inititalSessionState);
            //pool.Open();

            var testShellInstance = PowerShell.Create(inititalSessionState);
            //testShellInstance.RunspacePool = pool;

            return testShellInstance;
        }

        /// <summary>
        /// Helper command to generate a commandAst class given a string of commands as if
        /// they were entered at the command prompt.
        /// </summary>
        public static CommandAst CreateCommandAst(string promptText)
        {
            // Generate a script block abstract syntax tree from the provided prompt text.
            ScriptBlockAst scriptBlock = Parser.ParseInput(promptText, out _, out ParseError[] errors);
            Assert.Empty(errors);

            // Extract the command abstract syntax tree from the script block abstract syntax tree.
            var visitor = new GetCommandAst();
            scriptBlock.Visit(visitor);
            Assert.NotNull(visitor.commandAst);

            return visitor.commandAst;
        }

        /// <summary>
        /// Test the create CommandAst function.
        /// </summary>
        [Fact]
        public void CreateCommandAstTest()
        {
            // Arrange
            string testString = "conda env list";

            // Act
            CommandAst ast = CreateCommandAst(testString);
            var outputString = ast.ToString();
            var elementCount = ast.CommandElements.Count;

            // Assert
            Assert.Equal(3, elementCount);
            Assert.Equal(testString, outputString);
        }
    }
}

