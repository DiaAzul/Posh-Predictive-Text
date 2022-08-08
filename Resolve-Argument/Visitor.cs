using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResolveArgument
{
    using System.Management.Automation.Language;
    internal class ResolvingVisitor : AstVisitor
    {
        private List<string> elements;

        internal ResolvingVisitor()
        {
            this.elements = new List<string>();
        }

        internal string getFirstCommand()
        {
            return this.elements[0];
        }

        internal string getLastCommand()
        {
            return this.elements[this.elements.Count - 1];
        }

        internal string getPriorCommand()
        {
            return this.elements[this.elements.Count - 2];
        }

        public override AstVisitAction DefaultVisit(Ast ast)
        {
            string text = ast.ToString();
            this.elements.Add(text);
            LOGGER.Write($"Hello AST: {text}");

            return AstVisitAction.Continue;
        }

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string? ToString()
        {
            return base.ToString();
        }

        public override AstVisitAction VisitArrayExpression(ArrayExpressionAst arrayExpressionAst)
        {
            return base.VisitArrayExpression(arrayExpressionAst);
        }

        public override AstVisitAction VisitArrayLiteral(ArrayLiteralAst arrayLiteralAst)
        {
            return base.VisitArrayLiteral(arrayLiteralAst);
        }

        public override AstVisitAction VisitAssignmentStatement(AssignmentStatementAst assignmentStatementAst)
        {
            return base.VisitAssignmentStatement(assignmentStatementAst);
        }

        public override AstVisitAction VisitAttribute(AttributeAst attributeAst)
        {
            return base.VisitAttribute(attributeAst);
        }

        public override AstVisitAction VisitAttributedExpression(AttributedExpressionAst attributedExpressionAst)
        {
            return base.VisitAttributedExpression(attributedExpressionAst);
        }

        public override AstVisitAction VisitBinaryExpression(BinaryExpressionAst binaryExpressionAst)
        {
            return base.VisitBinaryExpression(binaryExpressionAst);
        }

        public override AstVisitAction VisitBlockStatement(BlockStatementAst blockStatementAst)
        {
            return base.VisitBlockStatement(blockStatementAst);
        }

        public override AstVisitAction VisitBreakStatement(BreakStatementAst breakStatementAst)
        {
            return base.VisitBreakStatement(breakStatementAst);
        }

        public override AstVisitAction VisitCatchClause(CatchClauseAst catchClauseAst)
        {
            return base.VisitCatchClause(catchClauseAst);
        }

        public override AstVisitAction VisitCommand(CommandAst commandAst)
        {

            LOGGER.Write("Yo, this is a command");

            return AstVisitAction.Continue;
        }

        public override AstVisitAction VisitCommandExpression(CommandExpressionAst commandExpressionAst)
        {
            return base.VisitCommandExpression(commandExpressionAst);
        }

        public override AstVisitAction VisitCommandParameter(CommandParameterAst commandParameterAst)
        {
            return base.VisitCommandParameter(commandParameterAst);
        }

        public override AstVisitAction VisitConstantExpression(ConstantExpressionAst constantExpressionAst)
        {
            return base.VisitConstantExpression(constantExpressionAst);
        }

        public override AstVisitAction VisitContinueStatement(ContinueStatementAst continueStatementAst)
        {
            return base.VisitContinueStatement(continueStatementAst);
        }

        public override AstVisitAction VisitConvertExpression(ConvertExpressionAst convertExpressionAst)
        {
            return base.VisitConvertExpression(convertExpressionAst);
        }

        public override AstVisitAction VisitDataStatement(DataStatementAst dataStatementAst)
        {
            return base.VisitDataStatement(dataStatementAst);
        }

        public override AstVisitAction VisitDoUntilStatement(DoUntilStatementAst doUntilStatementAst)
        {
            return base.VisitDoUntilStatement(doUntilStatementAst);
        }

        public override AstVisitAction VisitDoWhileStatement(DoWhileStatementAst doWhileStatementAst)
        {
            return base.VisitDoWhileStatement(doWhileStatementAst);
        }

        public override AstVisitAction VisitErrorExpression(ErrorExpressionAst errorExpressionAst)
        {
            return base.VisitErrorExpression(errorExpressionAst);
        }

        public override AstVisitAction VisitErrorStatement(ErrorStatementAst errorStatementAst)
        {
            return base.VisitErrorStatement(errorStatementAst);
        }

        public override AstVisitAction VisitExitStatement(ExitStatementAst exitStatementAst)
        {
            return base.VisitExitStatement(exitStatementAst);
        }

        public override AstVisitAction VisitExpandableStringExpression(ExpandableStringExpressionAst expandableStringExpressionAst)
        {
            return base.VisitExpandableStringExpression(expandableStringExpressionAst);
        }

        public override AstVisitAction VisitFileRedirection(FileRedirectionAst redirectionAst)
        {
            return base.VisitFileRedirection(redirectionAst);
        }

        public override AstVisitAction VisitForEachStatement(ForEachStatementAst forEachStatementAst)
        {
            return base.VisitForEachStatement(forEachStatementAst);
        }

        public override AstVisitAction VisitForStatement(ForStatementAst forStatementAst)
        {
            return base.VisitForStatement(forStatementAst);
        }

        public override AstVisitAction VisitFunctionDefinition(FunctionDefinitionAst functionDefinitionAst)
        {
            return base.VisitFunctionDefinition(functionDefinitionAst);
        }

        public override AstVisitAction VisitHashtable(HashtableAst hashtableAst)
        {
            return base.VisitHashtable(hashtableAst);
        }

        public override AstVisitAction VisitIfStatement(IfStatementAst ifStmtAst)
        {
            return base.VisitIfStatement(ifStmtAst);
        }

        public override AstVisitAction VisitIndexExpression(IndexExpressionAst indexExpressionAst)
        {
            return base.VisitIndexExpression(indexExpressionAst);
        }

        public override AstVisitAction VisitInvokeMemberExpression(InvokeMemberExpressionAst methodCallAst)
        {
            return base.VisitInvokeMemberExpression(methodCallAst);
        }

        public override AstVisitAction VisitMemberExpression(MemberExpressionAst memberExpressionAst)
        {
            return base.VisitMemberExpression(memberExpressionAst);
        }

        public override AstVisitAction VisitMergingRedirection(MergingRedirectionAst redirectionAst)
        {
            return base.VisitMergingRedirection(redirectionAst);
        }

        public override AstVisitAction VisitNamedAttributeArgument(NamedAttributeArgumentAst namedAttributeArgumentAst)
        {
            return base.VisitNamedAttributeArgument(namedAttributeArgumentAst);
        }

        public override AstVisitAction VisitNamedBlock(NamedBlockAst namedBlockAst)
        {
            LOGGER.Write("Named Block.");
            return base.VisitNamedBlock(namedBlockAst);
        }

        public override AstVisitAction VisitParamBlock(ParamBlockAst paramBlockAst)
        {
            return base.VisitParamBlock(paramBlockAst);
        }

        public override AstVisitAction VisitParameter(ParameterAst parameterAst)
        {
            return base.VisitParameter(parameterAst);
        }

        public override AstVisitAction VisitParenExpression(ParenExpressionAst parenExpressionAst)
        {
            return base.VisitParenExpression(parenExpressionAst);
        }

        public override AstVisitAction VisitPipeline(PipelineAst pipelineAst)
        {
            LOGGER.Write("Pipeline.");

            return base.VisitPipeline(pipelineAst);
        }

        public override AstVisitAction VisitReturnStatement(ReturnStatementAst returnStatementAst)
        {
            return base.VisitReturnStatement(returnStatementAst);
        }

        public override AstVisitAction VisitScriptBlock(ScriptBlockAst scriptBlockAst)
        {
            LOGGER.Write("SCRIPT BLOCK");

            return base.VisitScriptBlock(scriptBlockAst);
        }

        public override AstVisitAction VisitScriptBlockExpression(ScriptBlockExpressionAst scriptBlockExpressionAst)
        {
            return base.VisitScriptBlockExpression(scriptBlockExpressionAst);
        }

        public override AstVisitAction VisitStatementBlock(StatementBlockAst statementBlockAst)
        {
            return base.VisitStatementBlock(statementBlockAst);
        }

        public override AstVisitAction VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst)
        {
            LOGGER.Write("String constant expression.");

            return base.VisitStringConstantExpression(stringConstantExpressionAst);
        }

        public override AstVisitAction VisitSubExpression(SubExpressionAst subExpressionAst)
        {
            return base.VisitSubExpression(subExpressionAst);
        }

        public override AstVisitAction VisitSwitchStatement(SwitchStatementAst switchStatementAst)
        {
            return base.VisitSwitchStatement(switchStatementAst);
        }

        public override AstVisitAction VisitThrowStatement(ThrowStatementAst throwStatementAst)
        {
            return base.VisitThrowStatement(throwStatementAst);
        }

        public override AstVisitAction VisitTrap(TrapStatementAst trapStatementAst)
        {
            return base.VisitTrap(trapStatementAst);
        }

        public override AstVisitAction VisitTryStatement(TryStatementAst tryStatementAst)
        {
            return base.VisitTryStatement(tryStatementAst);
        }

        public override AstVisitAction VisitTypeConstraint(TypeConstraintAst typeConstraintAst)
        {
            return base.VisitTypeConstraint(typeConstraintAst);
        }

        public override AstVisitAction VisitTypeExpression(TypeExpressionAst typeExpressionAst)
        {
            return base.VisitTypeExpression(typeExpressionAst);
        }

        public override AstVisitAction VisitUnaryExpression(UnaryExpressionAst unaryExpressionAst)
        {
            return base.VisitUnaryExpression(unaryExpressionAst);
        }

        public override AstVisitAction VisitUsingExpression(UsingExpressionAst usingExpressionAst)
        {
            return base.VisitUsingExpression(usingExpressionAst);
        }

        public override AstVisitAction VisitVariableExpression(VariableExpressionAst variableExpressionAst)
        {
            return base.VisitVariableExpression(variableExpressionAst);
        }

        public override AstVisitAction VisitWhileStatement(WhileStatementAst whileStatementAst)
        {
            return base.VisitWhileStatement(whileStatementAst);
        }
    }
}