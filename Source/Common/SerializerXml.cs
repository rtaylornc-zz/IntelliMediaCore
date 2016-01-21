#region Copyright 2014 North Carolina State University
//---------------------------------------------------------------------------------------
// Copyright 2014 North Carolina State University
//
// Computer Science Department
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
using System.Collections.Generic;
using System;
using System.IO;
using System.Xml;

#endregion

namespace IntelliMedia 
{
    public class SerializerXml : ISerializer
    {
        public readonly static SerializerXml Instance = new SerializerXml();
        private XmlWriterSettings settingsForCompactOutput = new XmlWriterSettings()
        {
            OmitXmlDeclaration = true
        };

        private XmlWriterSettings settingsForReadableOutput = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\r\n",
            NewLineHandling = NewLineHandling.Replace
        };

        public readonly static List<Type> ExtraTypes = new List<Type>();

        #region ISerializer implementation

        public string FilenameExtension { get { return "xml"; }}

        public T Clone<T>(T obj) where T : class, new()
        {
            return Deserialize<T>(Serialize<T>(obj));
        }

        public string Serialize<T> (T obj, bool readableOutput = false) where T : class, new()
        {
            try
            {
                System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(obj.GetType(), ExtraTypes.ToArray());
                using (StringWriter writer = new StringWriter())
                {
                    XmlWriterSettings settings = (readableOutput ? settingsForReadableOutput : settingsForCompactOutput);
                    using (XmlWriter xmlWriter = XmlWriter.Create(writer, settings))
                    {
                        xmlSerializer.Serialize(xmlWriter, obj);
                        string xml = writer.ToString();
                        //DebugLog.Info("To XML: " + xml);
                        return xml;
                    }
                }
            }
            catch (Exception e)
            {
                DebugLog.Error("Unable to serialize {0}. XML serialize error: {1}", 
                               typeof(T).Name,
                               DebugLog.GetNestedMessages(e));
                throw new Exception("Unable to serialize to XML: " + DebugLog.SafeName(obj), e);
            }
        }

        public T Deserialize<T>(string xml) where T : class, new()
        {
            try
            {
                //DebugLog.Info("From XML: " + xml);
                System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                using (TextReader reader = new StringReader(xml))
                {
                    T obj = (T)xmlSerializer.Deserialize(reader);
                    return obj;
                }
            }
            catch (Exception e)
            {
                DebugLog.Error("Unable to deserialize {0}. XML deserialize error: {1}\n{2}", 
                               typeof(T).Name,
                               DebugLog.GetNestedMessages(e), 
                               xml);
                throw new Exception("Unable to deserialize from XML: " + typeof(T).GetType().Name, e);
            }
        }

        public string GetHeader<T>() where T : class, new()
        {
            return "";
        }

        #endregion
    }
}
