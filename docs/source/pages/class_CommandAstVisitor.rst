Class: CommandAstVisitor
========================

**Note: Neither Doxygen nor sphinx-csharp support Python record types. Introduced
in C# 9.**

Once record types are recognised then details for Token will appear below.

The token is a record with the following values:

   - readonly string Value - Text representing a token on the command line.
   - readonly Type Type - The type of the token.

And the following read only properties which return true if the record is
the indicated type:

   - bool IsCommand
   - bool IsCommandExpression
   - bool IsCommandParameter
   - bool IsStringConstantExpression

.. doxygenclass:: PoshPredictiveText::Token
   :project: PoshPredictiveText
   :members:

.. doxygenclass:: PoshPredictiveText::CommandAstVisitor
   :project: PoshPredictiveText
   :members:
