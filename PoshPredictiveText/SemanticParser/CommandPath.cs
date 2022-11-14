
namespace PoshPredictiveText.SemanticParser
{
    using System.Collections.Immutable;
    using System.Management.Automation;

    /// <summary>
    /// Commands on the CLI are made from multi-token commands.
    /// The available suggestions are determined by the command tokens
    /// entered. The CommandPath object maintains a list of command
    /// tokens entered and provides properties to add additional tokens
    /// and return the entire command path as a formatted string.
    /// </summary>
    internal class CommandPath
    {
        internal ImmutableList<string> commands;

        /// <summary>
        /// Initialise an empty command path.
        /// </summary>
        internal CommandPath()
        {
            commands = new List<string>().ToImmutableList();
        }

        /// <summary>
        /// Initialise a command path and add the first command token.
        /// </summary>
        /// <param name="command"></param>
        internal CommandPath(string command)
        {
            commands = new List<string>
            {
                command.ToLower()
            }.ToImmutableList();
        }

        /// <summary>
        /// Initialises a command path from an existing command path
        /// object (copies the input command path).
        /// </summary>
        /// <param name="commandPath"></param>
        internal CommandPath(CommandPath commandPath)
        {
            commands = new List<string>(commandPath.commands).ToImmutableList();
        }

        /// <summary>
        /// Add a command to the command path.
        /// </summary>
        /// <param name="command"></param>
        internal void Add(string command)
        {
            commands =  commands.Add(command.ToLower());
        }

        /// <summary>
        /// Return the number of commands in the command path.
        /// </summary>
        internal int Count => commands.Count;

        /// <summary>
        /// Return a string representation of the command path.
        /// 
        /// Each commmand token is separated by a period(.).
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join(".", commands);
        }

        public CommandPath DeepCopy()
        {
            CommandPath newPath = (CommandPath)MemberwiseClone();
            newPath.commands = new List<string>(commands).ToImmutableList();
            return newPath;
        }
    }
}
