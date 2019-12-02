using Expressions.Enums;
using System.Collections.Generic;

namespace Expressions
{
    public static class Constants
    {
        public static readonly List<List<Operation>> OperationsSortedByPriority = new List<List<Operation>>
        {
            new List<Operation>{  Operation.Exponentiation, Operation.Multiplication, Operation.Division, Operation.Modulo},
            new List<Operation>{Operation.Subtraction, Operation.Addition}
        };
    }
}
