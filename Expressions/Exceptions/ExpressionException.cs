using System;

namespace Expressions.Exceptions
{
    public class ExpressionException : Exception
    {
        public ExpressionException(string message) : base(message) { }
    }
}
