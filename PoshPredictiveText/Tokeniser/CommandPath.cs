﻿
namespace PoshPredictiveText
{
    /// <summary>
    /// Holds individual commands which define the command path.
    /// </summary>
    internal class CommandPath
    {
        private readonly List<string> commands = new();

        /// <summary>
        /// Initialise an empty command path.
        /// </summary>
        internal CommandPath() { }

        /// <summary>
        /// Initialise a command path and add the first command.
        /// </summary>
        /// <param name="command"></param>
        internal CommandPath(string command)
        {
            commands.Add(command.ToLower());
        }

        /// <summary>
        /// Add a command to the command path.
        /// </summary>
        /// <param name="command"></param>
        internal void Add(string command)
        {
            commands.Add(command.ToLower());
        }

        /// <summary>
        /// Return the number of commands in the command path.
        /// </summary>
        internal int Count { get { return commands.Count; } }

        /// <summary>
        /// Return a string representation of the command path.
        /// 
        /// Each commmand is separated by a period(.).
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join(".", commands);
        }
    }
}
