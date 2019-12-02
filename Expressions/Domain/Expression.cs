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
        /// <summary>
        /// First partial expression, that needs to be solved separately, before solving this expression
        /// </summary>
        public Expression ExpressionA { get; set; }

        /// <summary>
        /// Second partial expression, that needs to be solved separately, before solving this expression
        /// </summary>
        public Expression ExpressionB { get; set; }

        /// <summary>
        /// Defines mathematical operation, that needs to be executed between the results of ExpressionA and ExpressionB
        /// </summary>
        public Operation Operation { get; set; }

        /// <summary>
        /// Position of the operator character in the expression string (FullExpression) 
        /// </summary>
        private int? OperatorPosition { get; set; }

        /// <summary>
        /// String that is going to be validated, converted to the expression and be solved
        /// </summary>
        private string FullExpression { get; set; }

        /// <summary>
        /// Constructor includes validation of the expression string as well as defining the sub expressions and the operation if possible 
        /// </summary>
        /// <param name="expression">String representation of the Expression</param>
        public Expression(string expression)
        {
            //remove all whitespace or set expression to 0 if empty
            FullExpression = string.IsNullOrWhiteSpace(expression) ? "0" : Regex.Replace(expression, @"\s+", "");

            SetOperation();
            SetSubExpressions();
            Validate();
        }

        /// <summary>
        /// Validates the expression srings and handles error messages
        /// </summary>
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
                if(character == '(' && nextCharacter == ')')
                {
                    throw new ExpressionException("The expression cannot contain empty parethesis");
                }
            }
        }

        /// <summary>
        /// Calculates the actual result of the expression
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// If none further partial expressions exist, this function tries to convert the expression string to float value
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Executes a mathematical operation defined in Operation parameter, between two given numbers
        /// </summary>
        /// <param name="a">First number</param>
        /// <param name="b">Second number</param>
        /// <returns></returns>
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

        /// <summary>
        /// Checks whether given character is corensponding to a mathematical operator
        /// </summary>
        /// <param name="character">Given character</param>
        /// <returns></returns>
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

        /// <summary>
        /// Finds the operation, with a lowest priority in the expression, assings it to the parameter together with its position in the string
        /// </summary>
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

        /// <summary>
        /// Tries to separate the expression to partial expressions ExpressionA and Expression B, based on the position of the lowest priority operator
        /// </summary>
        private void SetSubExpressions()
        {
            if (Operation != Operation.None || OperatorPosition.HasValue)
            {
                ExpressionA = new Expression(FullExpression.Substring(0, OperatorPosition.Value));
                ExpressionB = new Expression(FullExpression.Substring(OperatorPosition.Value + 1, FullExpression.Length - (OperatorPosition.Value + 1)));
            }
        }

        /// <summary>
        /// Finds the last of each operator in the expression, that is not surrounded by the parethesis. 
        /// </summary>
        /// <returns></returns>
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
                                //the last occurence of any operator has the lowest priority and that's what we need
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
