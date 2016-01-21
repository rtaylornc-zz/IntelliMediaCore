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

namespace IntelliMedia
{
	/// <summary>
	/// Conveinence methods to verify method arguments and class properties. These methods throw argument
	/// exceptions if the condition is not met.
	/// </summary>
    public static class Contract
    {
        /// <summary>
        /// Throw exception if paramter is null.
        /// </summary>
        public static void ArgumentNotNull(string paramName, object parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Throw exception if type of parameter does not match type.
        /// </summary>
        public static void ArgumentOfType(string paramName, object parameter, Type type)
        {
            if (parameter != null && parameter.GetType() != type)
            {
                throw new ArgumentException("Parameter is not " + type.Name, paramName);
            }
        }
        
        public static void Argument(string message, string paramName, bool valid)
        {
            if (!valid)
            {
                throw new ArgumentException(message, paramName);
            }
        }
        
        /// <summary>
        /// Throw exception if property is null.
        /// </summary>
        public static void PropertyNotNull(string propName, object property)
        {
            if (property == null)
            {
                throw new Exception(string.Format("Property {0} is null", propName));
            }
        }

        /// <summary>
        /// Throw exception if property is NOT null.
        /// </summary>
        public static void PropertyNull(string propName, object property)
        {
            if (property != null)
            {
                throw new Exception(string.Format("Property {0} is NOT null", propName));
            }
        } 
    }
}
