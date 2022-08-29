using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using System.Threading.Tasks;

namespace Resolve_Argument_Test
{
    // TODO [ ][TEST] Token
    // TODO [ ][TEST] CommandAstVisitor
 
    internal class CommandAstVisitorTest
    {
    }
}


// internal record Token
//internal string Value { get; init; } = default!;
//internal Type Type { get; init; } = typeof(StringConstantExpressionAst);
//internal bool IsCommand
//internal bool IsCommandExpression
//internal bool IsCommandParameter
//internal bool IsStringConstantExpression



// internal class CommandAstVisitor : AstVisitor
//internal CommandAstVisitor()
//private int TokenPosition
//internal string? BaseCommand
//internal Token? LastToken
//internal Token? PriorToken
//internal Dictionary<int, Token> All
//internal int Count
//internal Dictionary<int, Token> CommandParameters
//internal Token? Index(int index)
//internal (string, int) CommandPath(List<string> uniqueCommands)
//internal bool CanUse(SyntaxItem syntaxItem)
//public override AstVisitAction DefaultVisit(Ast ast)
//public override AstVisitAction VisitCommand(CommandAst commandAst)
//public override AstVisitAction VisitCommandExpression(CommandExpressionAst commandExpressionAst)
//public override AstVisitAction VisitCommandParameter(CommandParameterAst commandParameterAst)
//public override AstVisitAction VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst)

