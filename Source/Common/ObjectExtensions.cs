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
using System.Globalization;
using IntelliMedia;

namespace System
{
    /// <summary>
    /// Additional helper methods for DateTime class
    /// </summary>
	public static class ObjectExtensions
    {
        public static object ConvertTo(this object value, Type type)
        {
            Contract.ArgumentNotNull("type", type);
           
            return System.Convert.ChangeType(
                        value, 
                        type);             
        }

        public static bool TryConvertTo(Type type, object originalObj, out object convertedObj)
        {
            Contract.ArgumentNotNull("type", type);

            convertedObj = null;

            try
            {                    
                convertedObj = System.Convert.ChangeType(
                    originalObj, 
                    type);
                return true;
            }
            catch(Exception)
            {
            }

            return false;
        }

        /// <summary>
        /// Tries to convert value by parsing string.
        /// </summary>
        /// <returns>True if the value was successfully parsed.</returns>
        /// <param name="value">Value.</param>
        public static bool ValueByParsing(this object value, Type desiredType, out object typedValue)
        {          
            typedValue = null;

            if (typeof(object) == desiredType)
            {
                typedValue = value;
                return true;
            }
            else if (value == null)
            {
                typedValue = value;
                return !desiredType.IsValueType;
            }

            string valueAsString = value.ToString();

            if (typeof(string) == desiredType)
            {
                typedValue = valueAsString;
                return true;
            }
            else if (typeof(bool) == desiredType)
            {
                bool boolValue = false;
                if (bool.TryParse(valueAsString, out boolValue))
                {
                    typedValue = boolValue;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (typeof(int) == desiredType)
            {
                int integer = 0;
                if (int.TryParse(valueAsString, out integer))
                {
                    typedValue = integer;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (typeof(double) == desiredType)
            {
                double floatingPoint = 0d;
                if (double.TryParse(valueAsString, out floatingPoint))
                {
                    typedValue = floatingPoint;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            // Guid.TryParse() is not implemented in Unity 3D Mono runtime
            #if TOOL
            else if (typeof(Guid) == desiredType)
            {
                Guid guid;
                if (Guid.TryParse(valueAsString, out guid))
                {
                    typedValue = guid;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            #endif
            else if (typeof(DateTime) == desiredType)
            {
                DateTime timestamp;
                if (DateTime.TryParse(valueAsString, out timestamp))
                {
                    typedValue = timestamp;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                throw new Exception("Unsupported parsing of type: " + desiredType.Name);
            }
        }
	}
}

