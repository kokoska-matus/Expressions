using Expressions.Enums;
using Expressions.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Expressions.Domain
{
    public class Expression
    {
        public Expression ExpressionA { get; set; }
        public Expression ExpressionB { get; set; }
        public Operation Operation { get; set; }
        private int? OperatorPosition { get; set; }
        private string FullExpression { get; set; }

        public Expression(string expression)
        {
            //remove all whitespace or set expression to 0 if empty
            FullExpression = string.IsNullOrWhiteSpace(expression) ? "0" : Regex.Replace(expression, @"\s+", "");

            SetOperation();
            SetSubExpressions();
            Validate();
        }

        private void Validate()
        {
            if(FullExpression.Count(c=>c=='(')!= FullExpression.Count(c => c == ')'))
            {
                throw new ExpressionException("Not all parentheses appear to be closed");
            }

            for (int i = 0; i < FullExpression.Length-1; i++)// (char c in FullExpression)
            {
                char character = FullExpression[i];
                char nextCharacter = FullExpression[i+1];
                if(TryConvertToOperation(character)!=null && TryConvertToOperation(nextCharacter) != null)
                {
                    throw new ExpressionException("These two operators cannot be followed directly by each other: " + character + nextCharacter);
                }
                if(character == '(' && nextCharacter == '(')
                {
                    throw new ExpressionException("The expression cannot contain empty parethesis");
                }
            }
        }

        public float GetResult()
        {
            float result;
            if (ExpressionA != null && ExpressionB != null)
            {
                result = ExecuteOperation(ExpressionA.GetResult(), ExpressionB.GetResult());
            }
            else
            {
                if (FullExpression.First() == '(' && FullExpression.Last() == ')')
                {
                    result = new Expression(FullExpression.Substring(1, FullExpression.Length - 2)).GetResult();
                }
                else
                {
                    result = GetNumberValue();
                }
            }
            return result;
        }

        private float GetNumberValue()
        {
            FullExpression = FullExpression.Replace(',', '.');

            bool success = float.TryParse(FullExpression, out float result);
            if (!success)
            {
                throw new ExpressionException("Following number has an invalid format: " + FullExpression);
            }
            return result;
        }

        private float ExecuteOperation(float a, float b)
        {
            switch (Operation)
            {
                case Operation.Addition:
                    return a + b;
                case Operation.Division:
                    if (b == 0)
                    {
                        throw new ExpressionException("Only Chuck Norris can divide by zero. If you are Chuck Norris, please contact the administrator");
                    }
                    return a / b;
                case Operation.Exponentiation:
                    return (float)Math.Pow(a, b);
                case Operation.Modulo:
                    if (b == 0)
                    {
                        throw new ExpressionException("Modulo operation could not be executed because of dividing by zero");
                    }
                    return a % b;
                case Operation.Subtraction:
                    return a - b;
                case Operation.Multiplication:
                default:
                    return a * b;
            }
        }

        private Operation? TryConvertToOperation(char character)
        {
            switch (character)
            {
                case '+':
                    return Operation.Addition;
                case '/':
                    return Operation.Division;
                case '^':
                    return Operation.Exponentiation;
                case '%':
                    return Operation.Modulo;
                case '*':
                    return Operation.Multiplication;
                case '-':
                    return Operation.Subtraction;
                default:
                    return null;
            }
        }

        private void SetOperation()
        {
            var availableOperatorPositiions = FindAvailableOperatorPositiions();

            if (availableOperatorPositiions.Count() > 0)
            {
                //set priority to the lowest available (lower number = higher priority)
                int priority = Constants.OperationsSortedByPriority.Count - 1;

                while (priority >= 0)
                {
                    var firstOperatoion = availableOperatorPositiions.LastOrDefault(o => Constants.OperationsSortedByPriority[priority].Contains(o.Key));
                    //if none found
                    if (firstOperatoion.Equals(default(KeyValuePair<Operation, int>)))
                    {
                        priority--;
                    }
                    else
                    {
                        Operation = firstOperatoion.Key;
                        OperatorPosition = firstOperatoion.Value;
                        break;
                    }
                }
            }
            else
            {
                Operation = Operation.None;
            }
        }

        private void SetSubExpressions()
        {
            if (Operation != Operation.None || OperatorPosition.HasValue)
            {
                ExpressionA = new Expression(FullExpression.Substring(0, OperatorPosition.Value));
                ExpressionB = new Expression(FullExpression.Substring(OperatorPosition.Value + 1, FullExpression.Length - (OperatorPosition.Value + 1)));
            }
        }

        private Dictionary<Operation, int> FindAvailableOperatorPositiions()
        {
            int leftParenthesesCount = 0;
            int rightParenthesesCount = 0;

            Dictionary<Operation, int> availableOperatorPositiions = new Dictionary<Operation, int>();

            for (int i = 0; i < FullExpression.Length; i++)// (char c in FullExpression)
            {
                char c = FullExpression[i];
                switch (c)
                {
                    case '(':
                        leftParenthesesCount++;
                        break;
                    case ')':
                        rightParenthesesCount++;
                        break;
                    default:
                        var operation = TryConvertToOperation(c);
                        if (leftParenthesesCount == rightParenthesesCount && operation.HasValue)
                        {
                            if (availableOperatorPositiions.ContainsKey(operation.Value))
                            {
                                //the last occurence of any operator is the most important
                                availableOperatorPositiions[operation.Value] = i;
                            }
                            else
                            {
                                availableOperatorPositiions.Add(operation.Value, i);
                            }
                        }
                        break;
                }
            }
            return availableOperatorPositiions;
        }
    }
}
