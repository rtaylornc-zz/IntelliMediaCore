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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace IntelliMedia
{
    public class SerializerCsv : ISerializer
    {
        public readonly static SerializerCsv Instance = new SerializerCsv();

        private class CsvColumns 
        {
            public FieldInfo[] Fields  { get; set; }
            public PropertyInfo[] Properties  { get; set; }

            public class ColumnEnumerator : IEnumerable<KeyValuePair<string, object>>
            {
                private CsvColumns csvColumns;
                private object obj;

                public ColumnEnumerator(CsvColumns csvColumns, object obj)
                {
                    this.csvColumns = csvColumns;
                    this.obj = obj;
                }

                public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
                {
                    foreach (FieldInfo field in csvColumns.Fields)
                    {                       
                        CsvMapToColumns csvMapToColumns = CsvMapToColumns.FindInMember(field);
                        if (csvMapToColumns != null)
                        {
                            IEnumerable<KeyValuePair<string, object>> columnValue = new KeyValueEnumerator(                            
                                field,
                                obj != null ? (field.GetValue(obj) as IEnumerable<KeyValuePair<string, object>>) : null);

                            foreach (KeyValuePair<string, object> keyValue in columnValue)
                            {
                                yield return keyValue;
                            }
                        }
                        else
                        {
                            yield return new KeyValuePair<string, object>(
                                field.Name,
                                (obj != null ? field.GetValue(obj) : null));
                        }
                    }
                    
                    foreach (PropertyInfo property in csvColumns.Properties)
                    {
                        if (!property.CanWrite || CsvIgnore.HasAttribute(property))
                        {
                            continue;
                        }

                        CsvMapToColumns csvMapToColumns = CsvMapToColumns.FindInMember(property);
                        if (csvMapToColumns != null)
                        {
                            IEnumerable<KeyValuePair<string, object>> columnValue = new KeyValueEnumerator(                            
                                property,
                                obj != null ? (property.GetValue(obj, null) as IEnumerable<KeyValuePair<string, object>>) : null);
                            
                            foreach (KeyValuePair<string, object> keyValue in columnValue)
                            {
                                yield return keyValue;
                            }
                        }
                        else
                        {
                            yield return new KeyValuePair<string, object>(
                                property.Name,
                                (obj != null ? property.GetValue(obj, null) : null));
                        }
                    }
                }

                private class KeyValueEnumerator : IEnumerable<KeyValuePair<string, object>>
                {
                    private MemberInfo memberInfo;
                    private IEnumerable<KeyValuePair<string, object>> collection;

                    public KeyValueEnumerator(MemberInfo memberInfo, IEnumerable<KeyValuePair<string, object>> collection)
                    {
                        this.memberInfo = memberInfo;
                        this.collection = collection;
                    }

                    #region IEnumerable implementation

                    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
                    {
                        CsvMapToColumns csvMapToColumns = CsvMapToColumns.FindInMember(memberInfo);
                        if (csvMapToColumns != null)
                        {
                            IEnumerator<KeyValuePair<string, object>> enumerator = null;
                            if (collection != null)
                            {
                                enumerator = collection.GetEnumerator();
                            }

                            for (int index = 0; index < LogEntry.MaxAttributes; ++index)
                            {
                                // If there is no enumerator for data, return null values since the header creation
                                // doesn't need the values, just the names.
                                string name = null;
                                object value = null;
                                if (enumerator != null)
                                {
                                    if (enumerator.MoveNext())
                                    {
                                        // Get the context attribute name/value
                                        name = enumerator.Current.Key;
                                        value = enumerator.Current.Value;
                                    }
                                    else
                                    {
                                        // If no attributes exist for all slots, use empty strings to
                                        // ensure the commas are added to the csv.                                       
                                        name = "";
                                        value = "";
                                    }
                                }
                                    
                                yield return new KeyValuePair<string, object>(
                                    String.Format("{0}Name{1}", csvMapToColumns.AlternateName, (index + 1)),
                                    name);
                                
                                yield return new KeyValuePair<string, object>(
                                    String.Format("{0}Value{1}", csvMapToColumns.AlternateName, (index + 1)),
                                    value);
                                
                            }
                        }
                    }

                    #endregion

                    #region IEnumerable implementation

                    IEnumerator IEnumerable.GetEnumerator()
                    {
                        throw new NotImplementedException();
                    }

                    #endregion
                }

                #region IEnumerable implementation

                IEnumerator IEnumerable.GetEnumerator()
                {
                    throw new NotImplementedException();
                }

                #endregion
            }

            public ColumnEnumerator GetColumnEnumerator(object obj)
            {
                return new ColumnEnumerator(this, obj);
            }
        }
        Dictionary<Type, CsvColumns> cache = new Dictionary<Type, CsvColumns>();

        #region ISerializer implementation

        public T Clone<T>(T obj) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public string Serialize<T>(T obj, bool readableOutput = false) where T : class, new()
        {
            CsvColumns csvColumns = GetPublicFieldsAndProperties<T>();

            StringBuilder text = new StringBuilder();

            bool prependComma = false;
            foreach (KeyValuePair<string, object> columnData in csvColumns.GetColumnEnumerator(obj))
            {
                if (prependComma)
                {
                    text.Append(",");
                }
                text.Append(ToCsvSafe(columnData.Value));
                prependComma = true;
            }

            text.AppendLine();

            return text.ToString();
        }

        public T Deserialize<T>(string json) where T : class, new()
        {
            throw new NotImplementedException();
        }

        public string FilenameExtension { get { return "csv"; } }

        public string GetHeader<T>() where T : class, new()
        {
            CsvColumns csvColumns = GetPublicFieldsAndProperties<T>();
            
            StringBuilder text = new StringBuilder();
            
            bool prependComma = false;
            foreach (KeyValuePair<string, object> columnData in csvColumns.GetColumnEnumerator(null))
            {
                if (prependComma)
                {
                    text.Append(",");
                }
                text.Append(ToCsvSafe(columnData.Key));
                prependComma = true;
            }
            
            text.AppendLine();
            
            return text.ToString();
        }

        #endregion

        private CsvColumns GetPublicFieldsAndProperties<T>()
        {
            if (!cache.ContainsKey(typeof(T)))
            {
                cache[typeof(T)] = new CsvColumns()
                {
                    Fields = typeof(T).GetFields(System.Reflection.BindingFlags.ExactBinding
                                                     | System.Reflection.BindingFlags.Public
                                                     | System.Reflection.BindingFlags.Instance),
                    Properties = typeof(T).GetProperties(System.Reflection.BindingFlags.ExactBinding
                                                             | System.Reflection.BindingFlags.Public
                                                             | System.Reflection.BindingFlags.Instance)

                };
            }
        
            return cache[typeof(T)];
        }
        
        private static string ToCsvSafe(object value)
        {
            if (value == null)
            {
                return "NULL";
            }
            
            // TODO rgtaylor 2015-03-27 Consider using regex or 3rd party CSV assembly 
            // to sanitize data to be suitable for CSV output.
            string valueAsString = null;
            if (value is DateTime)
            {
                valueAsString = ((DateTime)value).ToIso8601();
            }
            else
            {
                valueAsString = value.ToString();
            }
            
            // Remove newlines
            string safeValue = valueAsString.ToString().Replace("\r", " ").Replace("\n", " ");
            
            // Wrap in quotation marks if value contains a comma
            if (safeValue.Contains(","))
            {
                safeValue = safeValue.Replace("\"", "'");
                safeValue = string.Format("\"{0}\"", safeValue);
            }
            
            return safeValue;
        }
    }
}
