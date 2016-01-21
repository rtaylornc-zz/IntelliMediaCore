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
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace IntelliMedia
{
    public class RestRepository<T, R> : Repository<T> where T : class, new() where R : class, new()
    {
        private HttpClient httpClient = new HttpClient();
        private const int DefaultRetryCount = 3;
        private ISerializer Serializer { get; set; }
        
        private Uri restServerUri;
        public Uri RestServerUri 
        { 
            get { return restServerUri; } 
            private set { if (restServerUri != value) { restServerUri = value; OnRestServerUriChanged(); } }
        }

        private void OnRestServerUriChanged()
        {
            string path = string.Format("{0}/", typeof(T).Name.ToLower());

            CreateUri = new Uri(RestServerUri, path);
            ReadUri = new Uri(RestServerUri, path);
            UpdateUri = new Uri(new Uri(RestServerUri, path), "?method=put");
            DeleteUri = new Uri(RestServerUri, path);
        }
        
        protected Uri CreateUri { get; set; }
        protected Uri ReadUri { get; set; }
        protected Uri UpdateUri { get; set; }
        protected Uri DeleteUri { get; set; }

        public RestRepository(Uri restServer, ISerializer serializer = null)
        {
            Contract.ArgumentNotNull("restServer", restServer);

            RestServerUri = restServer;

            // Default to XML serializer
            Serializer = (serializer != null ? serializer : SerializerXml.Instance);
        }

        #region IRepository implementation

        public override void Insert(T instance, ResponseHandler callback)
        {
            string serializedObj = Serializer.Serialize<T>(instance);

            MimePart mimePart = new MimePart(MimePart.ApplicationXml, serializedObj);
            
            httpClient.Post(CreateUri, DefaultRetryCount, mimePart, (postResult) =>
            {
                List<T> responseInstance = null;
                string error = null;
                try
                {
                    using (HttpResult httpResult = postResult.AsyncState as HttpResult)
                    {
                        // Check server response
                        if (CheckServerResult(httpResult, ref error))
                        {
                            string responseObj = httpResult.Response;
                            if (!string.IsNullOrEmpty(responseObj))
                            {
                                RestResponse response = Serializer.Deserialize<R>(responseObj) as RestResponse;
                                responseInstance = response.ToList<T>();
                                error = response.Error;
                            }
                            else
                            {
                                error = "Empty response for: " + UpdateUri.ToString();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    error = "Unable to upload data to the cloud. " + e.Message;
                }
                finally
                {
                    if (!String.IsNullOrEmpty(error))
                    {
                        DebugLog.Error(error);
                    }

                    if (callback != null)
                    {
                        callback(new Response(responseInstance, error));
                    }
                }
            });
        }

        public override void Update(T instance, ResponseHandler callback)
        {
            string serializedObj = Serializer.Serialize<T>(instance);
            
            MimePart mimePart = new MimePart(MimePart.ApplicationXml, serializedObj);
            
            httpClient.Post(UpdateUri, DefaultRetryCount, mimePart, (postResult) =>
            {
                List<T> responseInstance = null;
                string error = null;
                try
                {
                    using (HttpResult httpResult = postResult.AsyncState as HttpResult)
                    {
                        // Check server response
                        if (CheckServerResult(httpResult, ref error))
                        {
                            string responseObj = httpResult.Response;
                            if (!string.IsNullOrEmpty(responseObj))
                            {
                                RestResponse response = Serializer.Deserialize<R>(responseObj) as RestResponse;
                                responseInstance = response.ToList<T>();
                                error = response.Error;
                            }
                            else
                            {
                                error = "Empty response for: " + UpdateUri.ToString();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    error = "Unable to update data to the cloud. " + e.Message;
                }
                finally
                {
                    if (!String.IsNullOrEmpty(error))
                    {
                        DebugLog.Error("RestRepository: {0}\nURI: {1}\nPayload: {2}", error, UpdateUri.ToString(), serializedObj);
                    }
                    
                    if (callback != null)
                    {
                        callback(new Response(responseInstance, error));
                    }
                }
            });
        }

        public override void Delete(T instance, ResponseHandler callback)
        {
            throw new System.NotImplementedException ();
        }

        public override IQuery<T> Where(System.Linq.Expressions.Expression<System.Func<T, bool>> predicate)
        {
            throw new System.NotImplementedException ();
        }

        public override void Get(System.Func<T, bool> predicate, ResponseHandler callback)
        {
            throw new System.NotImplementedException ();
        }

        public override void GetByKeys(object[] keys, ResponseHandler callback)
        {
            Get(new Uri(ReadUri, KeysToPath(keys)), callback);
        }

        public override void GetByKey(object key, ResponseHandler callback)
        {
            GetByKey(null, key, callback);
        }

        protected static string KeysToPath(object[] keys)
        {
            Contract.ArgumentNotNull("keys", keys);
            
            return String.Join(",", Array.ConvertAll(keys, k => k.ToString()));
        }

        public void GetByKey(string path, object key, ResponseHandler callback)
        {
            Uri readPath = ReadUri;
            if (path != null)
            {
                readPath = new Uri(readPath, path);
            }

            Uri objReadUri = new Uri(readPath, key.ToString());

            Get(objReadUri, callback);
        }

        protected void Get(Uri uri, ResponseHandler callback)
        {
            httpClient.Get(uri, DefaultRetryCount, (postResult) =>
            {
                List<T> instance = null;
                string error = null;
                try
                {
                    using (HttpResult httpResult = postResult.AsyncState as HttpResult)
                    {
                        // Check status of the WWW upload
                        if (CheckServerResult(httpResult, ref error))
                        {
                            string serializedObj = httpResult.Response;
                            if (!string.IsNullOrEmpty(serializedObj))
                            {
                                RestResponse response = Serializer.Deserialize<R>(serializedObj) as RestResponse;
                                instance = response.ToList<T>();
                                error = response.Error;
                            }
                            else
                            {
                                error = "Empty response for: " + uri.ToString();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    error = "Unable to download data from cloud. " + e.Message;
                }
                finally
                {
                    if (!String.IsNullOrEmpty(error))
                    {
                        DebugLog.Error(error);
                    }

                    if (callback != null)
                    {
                        callback(new Response(instance, error));
                    }
                }
            });
        }

        #endregion

        private bool CheckServerResult(HttpResult httpResult, ref string error)
        {       
            if (httpResult.Error != null)
            {
                error = ParseHttpExceptionMessage(httpErrorRegexPattern, httpResult.Error.Message);
            }
            // Accept any HTTP status code in the 2xx range as success
            else if (httpResult.StatusCode != System.Net.HttpStatusCode.OK
                     && httpResult.StatusCode != System.Net.HttpStatusCode.Created
                     && httpResult.StatusCode != System.Net.HttpStatusCode.Accepted
                     && httpResult.StatusCode != System.Net.HttpStatusCode.NonAuthoritativeInformation
                     && httpResult.StatusCode != System.Net.HttpStatusCode.NoContent
                     && httpResult.StatusCode != System.Net.HttpStatusCode.ResetContent
                     && httpResult.StatusCode != System.Net.HttpStatusCode.PartialContent)
            {
                // If this fails, confirm all "correct" status codes are listed above AND
                // check the parsing code in UnityWebResponse.
                error = string.Format("Unexpected Status Code: {0}", httpResult.StatusDescription);
            }
            
            return (error == null);
        }
        
        static readonly string httpErrorRegexPattern = @"^(?<status>\d*)\s*(?<message>.+)";
        
        static string ParseHttpExceptionMessage(string pattern, string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                return null;
            }
            
            Regex errorRegex = new Regex(pattern, RegexOptions.None);
            MatchCollection matches = errorRegex.Matches(error);
            if (matches.Count > 0 && matches[0].Success)
            {
                return matches[0].Groups["message"].Value;
            }
            
            return null;
        }

    }
}
