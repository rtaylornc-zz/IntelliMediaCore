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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using System.Runtime.Serialization;

namespace IntelliMedia
{   
    /// <summary>
    /// A Unity 3D WWW-based implementation of the standard .NET WebRequest class. 
	/// This allows networking code to be written using normal .NET APIs. Unfortunately,
    /// HttpWebReques in Mono was not made to inherit from (internal state isn't correct).
    /// </summary>
    public class UnityWebRequest : WebRequest
    {
        // The following singleton is used to hook into Unity's coroutine processing that
        // is required by the WWW class.
        private class UnityWebResponseSingleton : MonoBehaviourSingleton<UnityWebResponseSingleton>
        {
        }
        private static readonly MonoBehaviour MonoBehavior = UnityWebResponseSingleton.Instance;
		
        // Summary:
        //     Initializes a new instance of the System.Net.WebRequest class.
        public UnityWebRequest(Uri uri)
		{
			CheckMonoBehaviorProperty();
			Contract.ArgumentNotNull("uri", uri);

			requestUri = uri;
		}
		
        // Summary:
        //     When overridden in a descendant class, gets or sets the content type of the
        //     request data being sent.
        //
        // Returns:
        //     The content type of the request data.
        //
        // Exceptions:
        //   System.NotImplementedException:
        //     Any attempt is made to get or set the property, when the property is not
        //     overridden in a descendant class.
        public override string ContentType { get; set; }
		
        //
        // Summary:
        //     When overridden in a descendant class, gets or sets the collection of header
        //     name/value pairs associated with the request.
        //
        // Returns:
        //     A System.Net.WebHeaderCollection containing the header name/value pairs associated
        //     with this request.
        //
        // Exceptions:
        //   System.NotImplementedException:
        //     Any attempt is made to get or set the property, when the property is not
        //     overridden in a descendant class.
        public override WebHeaderCollection Headers { get; set; }
        //
        // Summary:
        //     When overridden in a descendant class, gets or sets the protocol method to
        //     use in this request.
        //
        // Returns:
        //     The protocol method to use in this request.
        //
        // Exceptions:
        //   System.NotImplementedException:
        //     If the property is not overridden in a descendant class, any attempt is made
        //     to get or set the property.
        public override string Method { get; set; }
        //
        // Summary:
        //     When overridden in a descendant class, gets the URI of the Internet resource
        //     associated with the request.
        //
        // Returns:
        //     A System.Uri representing the resource associated with the request.
        //
        // Exceptions:
        //   System.NotImplementedException:
        //     Any attempt is made to get or set the property, when the property is not
        //     overridden in a descendant class.
		private Uri requestUri;
        public override Uri RequestUri { get { return requestUri; }}
		
        // Summary:
        //     Aborts the Request.
        //
        // Exceptions:
        //   System.NotImplementedException:
        //     Any attempt is made to access the method, when the method is not overridden
        //     in a descendant class.
        public override void Abort()
		{
			throw new NotImplementedException();
		}
        //
        // Summary:
        //     When overridden in a descendant class, provides an asynchronous method to
        //     request a stream.
        //
        // Parameters:
        //   callback:
        //     The System.AsyncCallback delegate.
        //
        //   state:
        //     An object containing state information for this asynchronous request.
        //
        // Returns:
        //     An System.IAsyncResult that references the asynchronous request.
        //
        // Exceptions:
        //   System.NotImplementedException:
        //     Any attempt is made to access the method, when the method is not overridden
        //     in a descendant class.
        public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
		{
			CheckMonoBehaviorProperty();
			Contract.ArgumentNotNull("callback", callback);
			
			AsyncResult<WebRequest> result = new AsyncResult<WebRequest>(state as WebRequest);
			
			callback(result);
			
			return result;
		}
        //
        // Summary:
        //     When overridden in a descendant class, begins an asynchronous request for
        //     an Internet resource.
        //
        // Parameters:
        //   callback:
        //     The System.AsyncCallback delegate.
        //
        //   state:
        //     An object containing state information for this asynchronous request.
        //
        // Returns:
        //     An System.IAsyncResult that references the asynchronous request.
        //
        // Exceptions:
        //   System.NotImplementedException:
        //     Any attempt is made to access the method, when the method is not overridden
        //     in a descendant class.
        public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
		{
			CheckMonoBehaviorProperty();

			Dictionary<string, string> headers = new Dictionary<string, string>();
			
			headers.Add("Method", Method);
			headers.Add("Content-Type", ContentType);
			
			WWW www = null;
			if (Method == "POST")
			{
				www = new WWW(requestUri.ToString(), stream.ToArray(), headers);
			}
			else if (Method == "GET")
			{
				www = new WWW(requestUri.ToString());
			}
			else
			{
				throw new NotImplementedException();
			}
			
			AsyncResult<WWW> result = new AsyncResult<WWW>(www, state);
			
		    MonoBehavior.StartCoroutine(YieldUntilResponseReceived(callback, result));
			
			return result;
		}
		
	    private IEnumerator YieldUntilResponseReceived(AsyncCallback callback, AsyncResult<WWW> result)
	    {		
	        yield return result.Result;
			
	        callback(result);
			result.IsCompleted = true;
	    }
		
        //
        // Summary:
        //     When overridden in a descendant class, returns a System.IO.Stream for writing
        //     data to the Internet resource.
        //
        // Parameters:
        //   asyncResult:
        //     An System.IAsyncResult that references a pending request for a stream.
        //
        // Returns:
        //     A System.IO.Stream to write data to.
        //
        // Exceptions:
        //   System.NotImplementedException:
        //     Any attempt is made to access the method, when the method is not overridden
        //     in a descendant class.
        public override Stream EndGetRequestStream(IAsyncResult asyncResult)
		{
			CheckMonoBehaviorProperty();

			stream = new MemoryStream();
			
			return stream;
		}
        //
        // Summary:
        //     When overridden in a descendant class, returns a System.Net.WebResponse.
        //
        // Parameters:
        //   asyncResult:
        //     An System.IAsyncResult that references a pending request for a response.
        //
        // Returns:
        //     A System.Net.WebResponse that contains a response to the Internet request.
        //
        // Exceptions:
        //   System.NotImplementedException:
        //     Any attempt is made to access the method, when the method is not overridden
        //     in a descendant class.
        public override WebResponse EndGetResponse(IAsyncResult asyncResult)
		{
			CheckMonoBehaviorProperty();
			Contract.ArgumentOfType("asyncResult", asyncResult, typeof(AsyncResult<WWW>));
			
			AsyncResult<WWW> result = asyncResult as AsyncResult<WWW>;
			
			return new UnityWebResponse(result.Result);
		}
		
		//-------------------------------------------------------------------------------------------------------------
		private MemoryStream stream;

		/// <summary>
		/// Confirm that the MonoBehavior property has been set. This is required for using the WWW class to make
		/// web requests.
		/// </summary>
		void CheckMonoBehaviorProperty()
		{
			if (MonoBehavior == null)
			{
				throw new Exception("The UnityWebResponse class requires its static Cei.UnityWebResponse.MonoBehavior " +
					"property to be set to an instance of a MonoBehavior object to process web responses " +
					"in the Unity 3D environment. This should already be set to a singleton in UnityWebRequest.");
			}
		}
    }
}

