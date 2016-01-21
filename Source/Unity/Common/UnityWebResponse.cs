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
using UnityEngine;
using System.Runtime.Serialization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace IntelliMedia
{
    /// <summary>
    /// A Unity 3D WWW-based implementation of the standard .NET WebResponse class. 
	/// This allows networking code to be written using normal .NET APIs.
    /// NOTE: Ideally, we would inherit from HttpWebResponse since this is used for WWW access, but
    /// HttpWebResponse in Mono does not expose several key members (like StatusCode) to child classes.
    /// </summary>
	public class UnityWebResponse : WebResponse
	{
		private WWW www;
        private static readonly PropertyInfo ResponseHeadersProperty = 
            typeof(WWW).GetProperty("responseHeadersString", BindingFlags.NonPublic |BindingFlags.GetProperty | BindingFlags.Instance);
		
        public UnityWebResponse(WWW www)
		{			
            Contract.ArgumentNotNull("www", www);
			this.www = www;
		}

        public HttpStatusCode StatusCode { get; protected set; }
        public string StatusDescription { get; protected set; }
        public override long ContentLength { get; set; }
        public override string ContentType { get; set; }


		public override Stream GetResponseStream()
		{
			if (!string.IsNullOrEmpty(www.error))
			{
				throw new Exception(www.error);
			}
			
			if (!www.isDone)
			{
				throw new Exception("Unity WWW web request is not done.");
			}			           

            ParseStatusCode(www);

            ContentType = www.responseHeaders.ContainsKey("CONTENT-TYPE") ? www.responseHeaders["CONTENT-TYPE"] : "";
            ContentLength = www.size;

			byte[] byteArray = www.bytes;
			return new MemoryStream(byteArray);
		}


        /// Status line as defined by the HTTP spec: http://www.w3.org/Protocols/rfc2616/rfc2616-sec6.html#sec6.1
        ///    HTTP-Version SP Status-Code SP Reason-Phrase CRLF
        static readonly string httpStatusCodeRegexPattern = @"(?<version>HTTP\S*)\s(?<code>\S+)\s(?<reason>.+)";

        /// <summary>
        /// Map status code to enum. Unfortunately, Unity 3D's WWW class returns the wrong status code
        /// if there is more than one. Only the first code is recorded in www.responseHeaders.
        /// </summary>
        void ParseStatusCode(WWW www)
        {
            Contract.PropertyNotNull("ResponseHeadersProperty", ResponseHeadersProperty);

            string rawHeaders = null;

            try
            {
                rawHeaders = ResponseHeadersProperty.GetValue(www, null) as String;
                /// In this example (see raw headers below), www.responseHeaders["STATUS"] returns "HTTP/1.1 100 Continue"
                ///                HTTP/1.1 100 Continue                           
                ///
                ///                HTTP/1.1 200 OK
                ///                
                ///                Content-Type: application/xml; charset=UTF-8
                ///                
                ///                Date: Thu, 11 Dec 2014 05:04:56 GMT
                ///                    
                ///                    Accept-Ranges: bytes
                ///                    
                ///                    Server: Development/1.0
                ///                    
                ///                    Cache-Control: no-cache
                ///                    
                ///                    Expires: Fri, 01 Jan 1990 00:00:00 GMT
                ///                    
                ///                    Content-Length: 3905
            }
            catch(Exception e)
            {
                DebugLog.Warning("Unable to access private WWW response headers. {0}", e.Message);

                // Fallback to use public headers
                www.responseHeaders.TryGetValue("STATUS", out rawHeaders);
            }

            if (string.IsNullOrEmpty(rawHeaders))
            {
                StatusCode = HttpStatusCode.InternalServerError;
                StatusDescription = "HTTP status code is missing";
                return;
            }
            
            Regex statusRegex = new Regex(httpStatusCodeRegexPattern, RegexOptions.None);
            MatchCollection matches = statusRegex.Matches(rawHeaders);

            // Get the last HTTP status in the raw headers that matches the regex
            if (matches.Count > 0 && matches[matches.Count-1].Success)
            {
                int status = 500;
                int.TryParse(matches[matches.Count-1].Groups["code"].Value, out status);
                SetStatusCode(status);

                StatusDescription = matches[0].Groups["reason"].Value;
            }
            else
            {
                StatusCode = HttpStatusCode.InternalServerError;
                StatusDescription = String.Format("Unable to parse status: {0}", rawHeaders);
            }
        }

        void SetStatusCode(int status)
        {
            HttpStatusCode code;
            switch (status) {
            case 100:
                code = HttpStatusCode.Continue;
                break;
            case 101:
                code = HttpStatusCode.SwitchingProtocols;
                break;
            case 200:
                code = HttpStatusCode.OK;
                break;
            case 201:
                code = HttpStatusCode.Created;
                break;
            case 202:
                code = HttpStatusCode.Accepted;
                break;
            case 203:
                code = HttpStatusCode.NonAuthoritativeInformation;
                break;
            case 204:
                code = HttpStatusCode.NoContent;
                break;
            case 205:
                code = HttpStatusCode.ResetContent;
                break;
            case 206:
                code = HttpStatusCode.PartialContent;
                break;
            case 300:
                code = HttpStatusCode.MultipleChoices;
                break;
            case 301:
                code = HttpStatusCode.MovedPermanently;
                break;
            case 302:
                code = HttpStatusCode.Found;
                break;
            case 303:
                code = HttpStatusCode.SeeOther;
                break;
            case 304:
                code = HttpStatusCode.NotModified;
                break;
            case 305:
                code = HttpStatusCode.UseProxy;
                break;
            case 306:
                code = HttpStatusCode.Unused;
                break;
            case 307:
                code = HttpStatusCode.TemporaryRedirect;
                break;
            case 400:
                code = HttpStatusCode.BadRequest;
                break;
            case 401:
                code = HttpStatusCode.Unauthorized;
                break;
            case 402:
                code = HttpStatusCode.PaymentRequired;
                break;
            case 403:
                code = HttpStatusCode.Forbidden;
                break;
            case 404:
                code = HttpStatusCode.NotFound;
                break;
            case 405:
                code = HttpStatusCode.MethodNotAllowed;
                break;
            case 406:
                code = HttpStatusCode.NotAcceptable;
                break;
            case 407:
                code = HttpStatusCode.ProxyAuthenticationRequired;
                break;
            case 408:
                code = HttpStatusCode.RequestTimeout;
                break;
            case 409:
                code = HttpStatusCode.Conflict;
                break;
            case 410:
                code = HttpStatusCode.Conflict;
                break;
            case 411:
                code = HttpStatusCode.LengthRequired;
                break;
            case 412:
                code = HttpStatusCode.PreconditionFailed;
                break;
            case 413:
                code = HttpStatusCode.RequestEntityTooLarge;
                break;
            case 414:
                code = HttpStatusCode.RequestUriTooLong;
                break;
            case 415:
                code = HttpStatusCode.UnsupportedMediaType;
                break;
            case 416:
                code = HttpStatusCode.RequestedRangeNotSatisfiable;
                break;
            case 417:
                code = HttpStatusCode.ExpectationFailed;
                break;
            case 500:
                code = HttpStatusCode.InternalServerError;
                break;
            case 501:
                code = HttpStatusCode.NotImplemented;
                break;
            case 502:
                code = HttpStatusCode.BadGateway;
                break;
            case 503:
                code = HttpStatusCode.ServiceUnavailable;
                break;
            case 504:
                code = HttpStatusCode.GatewayTimeout;
                break;
            case 505:
                code = HttpStatusCode.HttpVersionNotSupported;
                break;
            default:
                code = HttpStatusCode.InternalServerError;
                break;
            }

            StatusCode = code;
        }
		
		public override void Close()
		{
			if (www != null)
			{
				www.Dispose();
			}
		}
	}
}

