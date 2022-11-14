

namespace PoshPredictiveText.SemanticParser
{
    using PoshPredictiveText.SyntaxTrees;
    using PoshPredictiveText.SyntaxTreeSpecs;

    /// <summary>
    /// Macine state information for command line arguments which have already been parsed.
    /// </summary>
    internal class MachineState
    {
        /// <summary>
        /// Name of the syntax tree.
        /// </summary>
        internal string? SyntaxTreeName = null;

        /// <summary>
        /// Syntax tree, loaded once the base command is identified.
        /// </summary>
        internal SyntaxTree? SyntaxTree = null;

        /// <summary>
        /// Parse mode for the command.
        /// </summary>
        internal ParseMode ParseMode = ParseMode.Windows;

        /// <summary>
        /// Permissible states within the state machine
        /// </summary>
        internal enum State
        {
            NoCommand, // No command has been identified.
            Item, // Process the token as command, option, parameter.
            Value, // Process the token as a value
            Inert, // No further processing required.
        }

        /// <summary>
        /// State of the statemachine after the argument was parsed.
        /// </summary>
        internal State CurrentState { get; set; } = State.NoCommand;

        /// <summary>
        /// Command path after the argument was parsed.
        /// </summary>
        internal CommandPath CommandPath { get; set; } = new();

        /// <summary>
        /// Number of parameter values to be entered after the argument if the parsed item
        /// was a parameter which expects values.
        /// </summary>
        internal int ParameterValues { get; set; } = 0;

        /// <summary>
        /// Parameter syntaxItem if the parsed item was a parameter expecting value arguments.
        /// </summary>
        internal SyntaxItem? ParameterSyntaxItem { get; set; } = null;

        /// <summary>
        /// Parameter sets the are in force for this command path.
        /// </summary>
        internal List<string>? ParameterSets { get; set; } = null!;

        /// <summary>
        /// The list of semantic tokens to be returned once input token parsed.
        /// </summary>
        internal List<SemanticToken>? SemanticTokens { get; set; } = null;

        /// <summary>
        /// Return a clone with a deep copy of CommandPath.
        /// </summary>
        /// <returns></returns>
        public MachineState DeepCopy()
        {
            MachineState newState = (MachineState)MemberwiseClone();
            newState.CommandPath = CommandPath.DeepCopy();
            if (SemanticTokens is not null) newState.SemanticTokens= new(SemanticTokens);
            return newState;
        }

        /// <summary>
        /// Return a clone with a shallow copy of CommandPath.
        /// </summary>
        /// <returns></returns>
        public MachineState Copy()
        {
            MachineState newState = (MachineState)MemberwiseClone();
            return newState;
        }
    }
}
