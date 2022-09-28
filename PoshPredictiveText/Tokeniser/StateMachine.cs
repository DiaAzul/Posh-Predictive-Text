
namespace PoshPredictiveText
{
    /// <summary>
    /// The state machine evaluates tokens and adds
    /// semantic information.
    /// </summary>
    internal class StateMachine
    {
        private readonly State state = State.NoCommand;
        /// <summary>
        /// Permissible states within the state machine
        /// </summary>
        enum State
        {
            NoCommand, // No command has been identified.
            Item, // Process the token as command, option, parameter.
            Value, // Process the token as a value
            Inert, // No further processing required.
        }

        /// <summary>
        /// Routing function passing token to evaluation method
        /// dependent upon the state of the state machine.
        /// </summary>
        /// <param name="tokens">Input tokens for enhancement</param>
        /// <returns>Enhanced tokens</returns>
        internal List<Token> Evaluate(List<Token> tokens)
        {
            return state switch
            {
                State.NoCommand => NoCommand(tokens),
                State.Item => EvaluateItem(tokens),
                State.Value => EvaluateValue(tokens),
                State.Inert => tokens,
                _ => tokens,
            };
        }

        internal static List<Token> NoCommand(List<Token> tokens)
        {
            return tokens;
        }

        internal static List<Token> EvaluateItem(List<Token> tokens)
        {
            return tokens;
        }

        internal static List<Token> EvaluateValue(List<Token> tokens)
        {
            return tokens;
        }
    }
}
