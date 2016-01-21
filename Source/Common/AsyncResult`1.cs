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
using System.Threading;

namespace IntelliMedia
{
    public delegate void AsyncCallback<T>(AsyncResult<T> ar);

    /// <summary>
    /// Implementation of IAsyncResult interface that simplifies returning an object T as Result property
    /// from AsyncCallback method.
    /// </summary>
    /// <typeparam name="T">Type of object to return</typeparam>
    public class AsyncResult<T> : IAsyncResult
    { 
        private ManualResetEvent manualResetEvent = new ManualResetEvent(false);

        public AsyncResult()
        {
        }

        public AsyncResult(T result, object state = null)
        {
            Result = result;
			this.AsyncState = (state != null ? state : result);
        }

        public T Result { get; set; }

        public object AsyncState { get; internal set; }
        public WaitHandle AsyncWaitHandle { get { return manualResetEvent; }}
		
        public bool CompletedSynchronously { get; internal set; }
		
        private bool isCompleted;
        public bool IsCompleted 
        {
            get { return isCompleted; }
            internal set
            {
                if (isCompleted)
                {
                    throw new InvalidOperationException("DataServiceResult has already been marked as completed. This probably due to an error occuring in the callback. Check inner exception.");
                }

                if (value)
                {
                    manualResetEvent.Set();
                }

                isCompleted = value;
            } 
        }
    }
}
