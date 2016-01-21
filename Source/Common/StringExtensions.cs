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
namespace System
{
    /// <summary>
    /// Extension methods for string class.
    /// </summary>
	public static class StringExtensions
	{
        /// <summary>
        /// Allows finding case insenstive substring in a string.
        /// </summary>
        /// <param name="source">Original string</param>
        /// <param name="toCheck">Target to find in string</param>
        /// <param name="comp">Type of string comparison</param>
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        /// <summary>
        /// Tries to detect the type of the object which may be represented by the string.
        /// </summary>
        /// <returns>The detect type.</returns>
        /// <param name="value">Value.</param>
        public static Type TypeByParsing(this string value)
        {
            Type type = typeof(string);

            bool boolValue;
// Guid.TryParse() is not implemented in Unity 3D Mono runtime
#if TOOL
            Guid guid;
#endif
            DateTime timestamp;
            int integer = 0;
            double floatingPoint = 0d;

            if (String.IsNullOrEmpty(value))
            {
            }
            else if (bool.TryParse(value, out boolValue))
            {
                type = typeof(bool);
            }
            else if (int.TryParse(value, out integer))
            {
                type = typeof(int);
            }                
            else if (double.TryParse(value, out floatingPoint))
            {
                type = typeof(double);
            } 
// Guid.TryParse() is not implemented in Unity 3D Mono runtime
#if TOOL
            else if (Guid.TryParse(value, out guid))
            {
                type = typeof(Guid);
            }
#endif
            else if (DateTime.TryParse(value, out timestamp))
            {
                type = typeof(DateTime);
            }               

            return type;
        }            
	}
}

