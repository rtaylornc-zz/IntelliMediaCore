//---------------------------------------------------------------------------------------
// Copyright 2014 North Carolina State University
//
// Center for Educational Informatics
// http://www.cei.ncsu.edu/
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
//   * Redistributions of source code must retain the above copyright notice, this 
//     list of conditions and the following disclaimer.
//   * Redistributions in binary form must reproduce the above copyright notice, 
//     this list of conditions and the following disclaimer in the documentation 
//     and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
// OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//---------------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Collections.Generic;

namespace IntelliMedia
{
    public interface IParameters
    {
        T GetValueAs<T>(string name);
    }

    // Ensures parameter is available as a specific type

    public abstract class Operand
    {
        public abstract bool HasValue(IParameters parameters);
        public abstract object GetValue(IParameters parameters);

        public bool HasValue<T>(IParameters parameters)
        {
            if (HasValue(parameters))
            {
                if (GetValue(parameters) is T)
                {
                    return true;
                }
                else
                {
                    object value = null;
                    return GetValue(parameters).ValueByParsing(typeof(T), out value);
                }
            }

            return false;
        }

        public T GetValue<T>(IParameters parameters)
        {
            object value = GetValue(parameters);
            if (!(value is T))
            {
                if (!value.ValueByParsing(typeof(T), out value))
                {
                    throw new Exception(String.Format("Operand unable to convert '{0}'", 
                        typeof(T).Name));
                }
            }
                
            return (T)value;
        }
    }        

    public class Constant : Operand
    {
        public object Value { get; set; }

        #region IOperand implementation
        public override bool HasValue(IParameters parameters) { return true; }
        public override object GetValue(IParameters parameters) { return Value; }
        #endregion
    }

    public interface IOperandTimeline
    {
        IEnumerable<Operand> GetWindow(double leftSeconds, Operand current, string requiredName, double rightSeconds);
        IEnumerable<Operand> Before(double seconds, Operand current, string requiredName = null);
        IEnumerable<Operand> After(double seconds, Operand current, string requiredName = null);
    }

    public abstract class Operator : Operand
    {
        public const string ResultName = "OperatorResult";

        public Operand Lhs  { get; set; }
        public Operand Rhs  { get; set; }

        public abstract bool Test(IParameters parameters);

        #region IOperand implementation
        public override bool HasValue(IParameters parameters) { return true; }
        public override object GetValue(IParameters parameters) { return new bool?(Test(parameters)); }
        #endregion
    }

    public class EqualOperator : Operator
    {
        #region implemented abstract members of Operator
        public override bool Test(IParameters parameters)
        {
            return (Lhs.HasValue(parameters) 
                && Rhs.HasValue(parameters) 
                && object.Equals(Lhs.GetValue(parameters), Rhs.GetValue(parameters)));

        }
        #endregion
    }

    public class NotEqualOperator : Operator
    {
        #region implemented abstract members of Operator
        public override bool Test(IParameters parameters)
        {
            return (Lhs.HasValue(parameters) 
                && Rhs.HasValue(parameters) 
                && !object.Equals(Lhs.GetValue(parameters), Rhs.GetValue(parameters)));

        }
        #endregion
    }

    public class AndOperator : Operator
    {
        #region implemented abstract members of Operator
        public override bool Test(IParameters parameters)
        {
            return (Lhs.HasValue(parameters) 
                && Rhs.HasValue(parameters) 
                && ((Lhs.GetValue(parameters) as bool?) == true) && ((Rhs.GetValue(parameters) as bool?) == true));

        }
        #endregion
    }

    public class NotOperator : Operator
    {
        #region implemented abstract members of Operator
        public override bool Test(IParameters parameters)
        {
            Contract.Argument("Lhs must be null for unary operator", "Lhs", Lhs == null);
            return (Rhs.HasValue(parameters) 
                && ((Rhs.GetValue(parameters) as bool?) == false));

        }
        #endregion
    }

    public class OrOperator : Operator
    {
        #region implemented abstract members of Operator
        public override bool Test(IParameters parameters)
        {
            return ((Lhs.HasValue(parameters) || Rhs.HasValue(parameters))
                && (((Lhs.GetValue(parameters) as bool?) == true) || ((Rhs.GetValue(parameters) as bool?) == true)));

        }
        #endregion
    }

    public abstract class ComparableOperator : Operator
    {
        protected abstract bool CompareTo(IComparable comparable, object rhs);

        #region implemented abstract members of Operator
        public override bool Test(IParameters parameters)
        {
            IComparable comparable = Lhs.GetValue(parameters) as IComparable;
            if (comparable != null)
            {
                return CompareTo(comparable, Rhs.GetValue(parameters));
            }
            else if (!Lhs.HasValue(parameters))
            {
                return true;
            }

            throw new Exception("LessThanOperator left hand side does not implement IComparable");
        }
        #endregion
    }       

    public class LessThanOperator : ComparableOperator
    {
        #region implemented abstract members of ComparableOperator
        protected override bool CompareTo(IComparable comparable, object rhs)
        {
            int result = comparable.CompareTo(rhs);
            return result < 0;
        }
        #endregion
    }

    public class LessThanOrEqualToOperator : ComparableOperator
    {
        #region implemented abstract members of ComparableOperator
        protected override bool CompareTo(IComparable comparable, object rhs)
        {
            int result = comparable.CompareTo(rhs);
            return result <= 0;
        }
        #endregion
    }

    public class GreaterThanOperator : ComparableOperator
    {
        #region implemented abstract members of ComparableOperator
        protected override bool CompareTo(IComparable comparable, object rhs)
        {
            int result = comparable.CompareTo(rhs);
            return result > 0;
        }
        #endregion
    }

    public class GreaterThanOrEqualToOperator : ComparableOperator
    {
        #region implemented abstract members of ComparableOperator
        protected override bool CompareTo(IComparable comparable, object rhs)
        {
            int result = comparable.CompareTo(rhs);
            return result >= 0;
        }
        #endregion
    }

    /*
    public class Condition
    {
        public enum Operator
        {
            Equal,
            NotEqual,
            Contains,
            RateOfChangeGreaterThan,
            Duration
        }
        public string Name { get; set; }
        public Operator Comparison { get; set; }
        public string Value { get; set; }
        public double WindowSizeSecs { get; set; }
        public bool IsContinuous { get { return Comparison == Operator.RateOfChangeGreaterThan; } }

        public bool Test(IOperand operand, IOperandTimeline timeline = null)
        {
            if (operand[Name] == null)
            {
                return false;
            }

            switch (Comparison)
            {
                case Condition.Operator.Equal:
                    if (object.Equals(Value, operand[Name]))
                    {
                        return true;
                    }
                    break;
                case Condition.Operator.NotEqual:
                    if (!object.Equals(Value, operand[Name]))
                    {
                        return true;
                    }                            
                    break;
                case Condition.Operator.Contains:
                    string contextValue = operand[Name] as String;
                    if (contextValue != null && contextValue.Contains(Value))
                    {
                        return true;
                    }                                  
                    break;
                case Condition.Operator.RateOfChangeGreaterThan:
                    IEnumerable<IOperand> window = timeline.GetWindow(WindowSizeSecs / 2, operand, Name, WindowSizeSecs / 2);
                    if (window != null && window.First() != null)
                    {
                        object start = window.First()[Name];
                        object end = window.Last()[Name];
                        if (start != null && end != null)
                        {
                            double valueStart = Convert.ToDouble(start);
                            double valueEnd = Convert.ToDouble(end);
                            double rateOfChange = (valueEnd - valueStart) / WindowSizeSecs;
                            if (rateOfChange > Convert.ToDouble(Value))
                            {
                                return true;
                            }
                        }
                    }
                    break;

                case Condition.Operator.Duration:
                    if (Convert.ToDouble(operand[Name]) >= Convert.ToDouble(Value))
                    {
                        return true;
                    }                   
                    break;

                default:
                    throw new Exception("Unsupported filter condition: " + Comparison.ToString());
            }

            return false;
       }
    }
    */
}

