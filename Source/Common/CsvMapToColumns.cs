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
using System.Reflection;
using System.Collections.Generic;

namespace IntelliMedia
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CsvMapToColumns : Attribute
    { 
        public string AlternateName { get; set; }

        public CsvMapToColumns()
        {
        }

        public CsvMapToColumns(string baseName)
        {
            AlternateName = baseName;
        }            

        internal static CsvMapToColumns FindInMember(MemberInfo member)
        {
            CsvMapToColumns csvMapToColumns = null;

            object[] attributes = member.GetCustomAttributes(typeof(CsvMapToColumns), false);
            if (attributes != null && attributes.Length > 0) 
            {
                if (attributes.Length > 1) 
                {
                    throw new Exception (string.Format ("Multiple {0} attributes assigned to propery: {1}", 
                                                        typeof(CsvMapToColumns).Name, 
                                                        member.Name));
                }

                if (member.MemberType.Equals(typeof(KeyValuePair<string, object>)))
                {
                    throw new Exception (string.Format ("{0} attribute assigned to propery {1} must be of type: KeyValuePair<string, object>", 
                                                        typeof(CsvMapToColumns).Name, 
                                                        member.Name));
                }
                csvMapToColumns = attributes[0] as CsvMapToColumns;
            }

            return csvMapToColumns;
        }
    }
}
