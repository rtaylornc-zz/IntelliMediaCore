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
using System.Linq;
using System.IO;
using System.Collections.Specialized;
using System.Collections.Generic;

#if (SILVERLIGHT || WPF || TOOL)
using System.Web;
#endif

namespace IntelliMedia
{
    /// <summary>
    /// This class abstracts the HTTP payload that is passed to the HttpClient 
	/// object's methods.
    /// </summary>
    public class MimePart
    {
        public const string UrlEncoded = "application/x-www-form-urlencoded";
        public const string TextXml = "text/xml";
        public const string ApplicationXml = "application/xml";
        public const string ImageJpegContent = "image/jpeg";
        const string ImagePngContent = "image/png";

        public string ContentType { get; protected set; }
        public string Name { get; protected set; }
        public string Value { get; protected set; }

        public string Filename { get; protected set; }
        public Stream Stream { get; protected set; }

        public bool IsFile { get; protected set; }

        /// <summary>
        /// Constructor used to pass a string formatted by the caller 
        /// </summary>
        /// <param name="contentType">http://en.wikipedia.org/wiki/Internet_media_type</param>
        /// <param name="data">Caller formatted string data</param>
        public MimePart(string contentType, string data)
        {
            ContentType = contentType;
            Name = null;
            Value = data;
        }

        /// <summary>
        /// Constructor used to pass a variable list of name/value pairs as arguments to method
        /// </summary>
        /// <param name="contentType">http://en.wikipedia.org/wiki/Internet_media_type</param>
        /// <param name="data">Caller formatted string data</param>
        public MimePart(string contentType, params string[] namesAndValues)
        {
            Contract.ArgumentNotNull("namesAndValues", namesAndValues);
            Contract.Argument("Must have even number of string parameters (name1, value1, name2, value2, ....)", 
                              "namesAndValues", (namesAndValues.Length%2 == 0));

            ContentType = contentType;
            NameValueCollection nameValueCollection = new NameValueCollection();
            for (int index = 0; index < namesAndValues.Length; index += 2)
            {
                nameValueCollection.Add(namesAndValues[index], namesAndValues[index+1]);
            }

            ContentType = contentType;
            Value = ToUrlEncodedString(nameValueCollection);
        }

        /// <summary>
        /// Constructor used to pass name/value pair collection
        /// </summary>
        /// <param name="contentType">http://en.wikipedia.org/wiki/Internet_media_type</param>
        /// <param name="namesAndValues">First argument is a name, the second is the value</param>
        /// <param name="excludeNullValues">If the value is null, exclude it from the MIME name/value pairs</param>
        public MimePart(string contentType, NameValueCollection namesAndValues, bool excludeNullValues = false)
        {           
            ContentType = contentType;
            Value = ToUrlEncodedString(namesAndValues, excludeNullValues);
        }

        /// <summary>
        /// Constructor used to upload binary data
        /// </summary>
        /// <param name="contentType">Constructor</param>
        /// <param name="name">Name of part</param>
        /// <param name="filename">Filename</param>
        /// <param name="stream">Binary data</param>
        public MimePart(string contentType, string name, string filename, Stream stream)
        {
            ContentType = contentType;
            Name = name;
            Filename = filename;
            Stream = stream;
            IsFile = true;
        }

        private static string ToUrlEncodedString(NameValueCollection collection, bool excludeNullValues = false)
        {

            List<string> pairs = new List<string>();
            for (int index = 0; index < collection.Count; ++index)
            {
                if (collection.GetValues(index) != null)
                {
                    pairs.Add(string.Format("{0}={1}", 
                                                 HttpUtility.UrlEncode(collection.GetKey(index)), 
                                                 HttpUtility.UrlEncode(collection.GetValues(index)[0]))); 
                }
            }

            return string.Join("&", pairs.ToArray());            
        }
    }
}
