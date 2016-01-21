#region Copyright 2014 North Carolina State University
//---------------------------------------------------------------------------------------
// Copyright 2015 North Carolina State University
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
using System;


#endregion

using UnityEngine;
using System.Collections;
 
namespace IntelliMedia
{
    /// <summary>
    /// This class contains helper methods for interacting with the web browser hosting the Unity 3D WebPlugin
    /// and getting information about the current browser.
    /// </summary>
    public class WebBrowserUtility : MonoBehaviour
    {
        public float maxWaitTimeForResponseSeconds = 5;

        public string DisplayName { get { return (browserInfo != null ? browserInfo.DisplayName : null); }}
        public string DisplayVersion { get { return (browserInfo != null ? browserInfo.DisplayVersion : null); }}
        public Uri LaunchUrl { get; private set; }
        public string Error { get; private set; }

        private float startTime;
        private WebBrowserInfo browserInfo;

        /// <summary>
        /// Replacement method for Application.OpenURL() which can optionally disable the warning/action
        /// taken when the user switches the browser away from the page hosting the Unity 3D game. The warning
        /// is only displayed if the Unity 3D app is hosted in the Web Plugin (native apps do not display a
        /// message when opening a new web browser window).
        /// </summary>
        /// <param name="url">Destination URL</param>
        /// <param name="overrideWarning">If set to <c>true</c> override the warning message displayed by the browser (if set)</param>
        public static void OpenUrl(string url, bool overrideWarning = false)
        {
            if (UnityEngine.Application.isWebPlayer && overrideWarning)
            {
                // Clear funtion (if there is one) that is called when browser loads a new page
                string javascript = "window.onbeforeunload = null;";
                UnityEngine.Application.ExternalEval(javascript);
            }
            
            Application.OpenURL(url);
        }

        /// <summary>
        /// Replacement method for Application.OpenURL() which displays a warning message when the 
        /// user switches the browser away from the page hosting the Unity 3D game. The warning is displayed
        /// by the web browser and the message is not displayed if running as a native app.
        /// </summary>
        /// <param name="url">Destination URL</param>
        /// <param name="warningMsg">Message to be displayed by the browser when asking the user to confirm loading the new URL</param>
        public static void OpenUrl(string url, string warningMsg)
        {
            if (UnityEngine.Application.isWebPlayer)
            {
                // This message is displayed by the browser when the user navigates away from the page with the game on it:
                string javascript = System.String.Format("window.onbeforeunload = function() {{ return '{0}'; }}", warningMsg);
                UnityEngine.Application.ExternalEval(javascript);
            }
            
            Application.OpenURL(url);
        }

        public void Start()
        {
            startTime = GameTime.time;

            if (!Application.isWebPlayer)
            {
                Error = "Not running in web player.";
                return;
            }

            RequestUserAgent();
            RequestQueryString();
        }

        public bool IsInitialized
        {
            get
            {
                if (browserInfo != null && LaunchUrl != null)
                {
                    return true;
                }
                else if (GameTime.time - startTime > maxWaitTimeForResponseSeconds)
                {
                    if (browserInfo == null)
                    {
                        Error = "Web browser did not respond to UserAgent request.";                      
                    }
                    else if (LaunchUrl == null)
                    {
                        Error = "Web browser did not respond to QueryString request.";    
                    }
                    else
                    {
                        Error = "Web browser is not responding.";    
                    }
                }

                return (Error != null);
            }
        }

        public bool VersionGreaterThanOrEqualTo(WebBrowserInfo.BrowserType browser, string minVersion)
        {
            Contract.PropertyNotNull("browserInfo",  browserInfo);

            return browserInfo.MeetsMinimum(browser, minVersion);
        }

        private void RequestUserAgent()
        {
            try
            {
                // Request the launch URL's query string from the browser. OnQueryString will be called with the string.
                Application.ExternalEval(String.Format("u.getUnity().SendMessage(\"{0}\", \"UserAgentRequestCallback\", navigator.userAgent);", gameObject.name));
                // TestUserAgent();
            }
            catch(Exception e)
            {
                Error = e.Message;
            }
        }

        private void TestUserAgent()
        {
            // TODO rgtaylor 2015-01-21 Move this test code into Unit Tests for WebBrowserInfo (in to-be-created project)
            string[] userAgentTestStrings =
            {
                // Chrome 17.0.963.66
                "Mozilla/5.0 (X11; Linux i686) AppleWebKit/535.11 (KHTML, like Gecko) Chrome/17.0.963.66 Safari/535.11",
                // Chrome 19.0.1063.0
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_0) AppleWebKit/536.3 (KHTML, like Gecko) Chrome/19.0.1063.0 Safari/536.3",
                // Chrome 41.0.2228.0
                "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36",
                // Internet Explorer 7.0
                "Mozilla/4.0 (compatible; MSIE 7.0b; Windows NT 5.1; .NET CLR 1.1.4322; .NET CLR 2.0.40607)",
                // Internet Explorer 8.0
                "Mozilla/5.0 (compatible; MSIE 8.0; Windows NT 5.1; Trident/4.0; .NET CLR 1.1.4322; .NET CLR 2.0.50727)",
                // Internet Explorer 9.0
                "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0; chromeframe/12.0.742.112)",
                // Internet Explorer 10.0
                "Mozilla/1.22 (compatible; MSIE 10.0; Windows 3.1)",
                // Internet Explorer 10.6
                "Mozilla/5.0 (compatible; MSIE 10.6; Windows NT 6.1; Trident/5.0; InfoPath.2; SLCC1; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET CLR 2.0.50727) 3gpp-gba UNTRUSTED/1.0",
                // Internet Explorer 11.0
                "Mozilla/5.0 (compatible, MSIE 11, Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko",
                // Safari 5.0.5
                "Mozilla/5.0 (Macintosh; U; Intel Mac OS X 10_6_7; da-dk) AppleWebKit/533.21.1 (KHTML, like Gecko) Version/5.0.5 Safari/533.21.1",
                // Safari 5.1
                "Mozilla/5.0 (iPad; CPU OS 5_1 like Mac OS X) AppleWebKit/534.46 (KHTML, like Gecko ) Version/5.1 Mobile/9B176 Safari/7534.48.3",
                // Safari 7.0.3
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_3) AppleWebKit/537.75.14 (KHTML, like Gecko) Version/7.0.3 Safari/7046A194A",
                // Firefox 22.0
                "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:22.0) Gecko/20130328 Firefox/22.0",
                // Firefox 23.0
                "Mozilla/5.0 (Windows NT 6.2; rv:22.0) Gecko/20130405 Firefox/23.0",
                // Firefox 33.0
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10; rv:33.0) Gecko/20100101 Firefox/33.0",
            };
            UserAgentRequestCallback(userAgentTestStrings[14]);
        }

        private void UserAgentRequestCallback(string userAgent)
        {
            try
            {
                browserInfo = new WebBrowserInfo(userAgent);
            }
            catch(Exception e)
            {
                Error = e.Message;
            }
        }
        
        private void RequestQueryString()
        {
            try
            {
                // Request the launch URL's query string from the browser. OnQueryString will be called with the string.
                Application.ExternalEval(String.Format("u.getUnity().SendMessage(\"{0}\", \"QueryStringRequestCallback\", window.location.search);", gameObject.name));
            }
            catch(Exception e)
            {
                Error = e.Message;
            }
        }
        
        // Query string returned by the browser
        private void QueryStringRequestCallback(string querystring)
        {
            try
            {
                string launchUrl = UnityEngine.Application.absoluteURL;
                if (!string.IsNullOrEmpty(querystring))
                {
                    launchUrl += querystring;
                }   

                LaunchUrl = new Uri(launchUrl, UriKind.RelativeOrAbsolute);
            }
            catch(Exception e)
            {
                Error = e.Message;
            }
        }
    }
}