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
using System.IO;
using System.Net;
using System.Text;

namespace IntelliMedia
{
	/// <summary>
	/// Response associated with a HTTP GET or PUT. Make sure to dispose this object to 
    /// ensure response is properly cleaned up. 
	/// </summary>
    public class HttpResult : IDisposable
    {
        public HttpStatusCode StatusCode { get; protected set; }
        public string StatusDescription { get; protected set; }
		public string ContentType { get; protected set; }
		public Stream Stream { get; protected set; }

        public HttpResult(Stream stream, HttpStatusCode statusCode, string statusDescription, string contentType)
        {
            Stream = stream;
            StatusCode = statusCode;
            StatusDescription = statusDescription;
            ContentType = contentType;
        }

        public HttpResult(Exception e)
        {
            Error = e;
        }

		string response;
        /// <summary>
        /// The response from the web service. 
        /// </summary>
		public string Response 
		{ 
			get 
			{
				if (response == null && Stream != null)
				{
                    using (StreamReader reader = new StreamReader(Stream, Encoding.UTF8))
					{
                    	response = reader.ReadToEnd();
					}
				}
				
				return response; 
			}
		}


        Exception error;
        /// <summary>
        /// Exception object if an error occurred, NULL if the operation was successful.
        /// </summary>
        public Exception Error 
        {
            get { return error; }
            internal set
            {
                WebException webException = value as WebException;
                if (webException != null && webException.Status == WebExceptionStatus.RequestCanceled)
                {
                    error = new Exception("Request cancelled.", webException);
                }
                else
                {
                    error = value;
                }
            }
        }

		#region IDisposable implementation
		public void Dispose ()
		{
			if (Stream != null)
			{
				Stream.Dispose();
			}
		}
		#endregion
    }
}
