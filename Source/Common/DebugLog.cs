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
using System.Text;

namespace IntelliMedia
{
    /// <summary>
    /// This class abstracts the logging methods used in Unity and .NET.
    /// </summary>
    public class DebugLog
    {       
        public static string SafeName(object obj)
        {
            return (obj != null ? obj.ToString() : "null");
        }

        public static string GetNestedMessages(Exception e)
        {
            StringBuilder msg = new StringBuilder();

            Exception current = e;
            while (current != null)
            {
                msg.AppendLine(current.Message);
                current = current.InnerException;
            }

            return msg.ToString();
        }

        /// <summary>
        /// Record an informational message in the debug log.
        /// </summary>
        /// <param name="format">format string</param>
        /// <param name="args">zero or more parameters for format string</param>
        /// <returns>string that was logged</returns>
        public static string Info(string format, params object[] args)
        {
            string msg = TryFormat(format, args);
#if (SILVERLIGHT || WPF || TOOL)
            System.Diagnostics.Debug.WriteLine(msg);
#else
            UnityEngine.Debug.Log(msg);
#endif
            return msg;
        }

        /// <summary>
        /// Record a warning message to the debug log.
        /// </summary>
        /// <param name="format">format string</param>
        /// <param name="args">zero or more parameters for format string</param>
        /// <returns>string that was logged</returns>
        public static string Warning(string format, params object[] args)
        {
            string msg = TryFormat(format, args);
#if (SILVERLIGHT || WPF || TOOL)
            msg = "WARNING: " + msg;
            System.Diagnostics.Debug.WriteLine(msg);
#else
            UnityEngine.Debug.LogWarning(msg);
#endif
            return msg;
        }

        /// <summary>
        /// Record an error message to the debug log.
        /// </summary>
        /// <param name="format">format string</param>
        /// <param name="args">zero or more parameters for format string</param>
        /// <returns>string that was logged</returns>
        public static string Error(string format, params object[] args)
        {
            string msg = TryFormat(format, args);
#if (SILVERLIGHT || WPF || TOOL)
            msg = "ERROR: " + msg;
            System.Diagnostics.Debug.WriteLine(msg);
#else
            UnityEngine.Debug.LogError(msg);
#endif
            return msg;
        }

        /// <summary>
        /// Don't throw an exception if the message can't be formatted.
        /// </summary>
        /// <returns>The format.</returns>
        /// <param name="format">Format.</param>
        /// <param name="args">Arguments.</param>
        private static string TryFormat(string format, params object[] args)
        {
            string msg = null;
            try
            {
                msg = string.Format(format, args);
            }
            catch (Exception)
            {
                msg = "Unable to format message: " + (format != null ? format : "null");
            }

            return msg;
        }
    }
}

