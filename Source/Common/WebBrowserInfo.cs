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

namespace IntelliMedia
{
    /// <summary>
    /// This class parses the useragent string provided by web Browsers to attempt to determine
    /// the Browser type and version.
    /// </summary>
    public class WebBrowserInfo
    {
        public enum BrowserType
        {
            Opera,
            WindowsPhone,
            IE,
            Chrome,
            Sailfish,
            SeaMonkey,
            Firefox,
            Silk,
            Android,
            PhantomJS,
            BlackBerry,
            WebOS,
            Bada,
            Tizen,
            Safari,
            iPhone,
            iPad,
            iPod,
            Unknown
        }

        public bool Success { get { return Browser != BrowserType.Unknown; }}

        public BrowserType Browser { get; private set; }

        public string DisplayName { get; private set; }
        public string DisplayVersion { get; private set; }

        public bool IsiOS { get; private set; }
        public bool IsAndroid { get; private set; }
        public bool IsTablet { get; private set; }
        public bool IsMobile { get; private set; }

        public WebBrowserInfo(string userAgent)
        {
            DisplayName = "unknown";
            Browser = BrowserType.Unknown;
            DisplayVersion = "";

            if (!string.IsNullOrEmpty(userAgent))
            {
                Parse(userAgent);
            }
        }

        public bool MeetsMinimum(BrowserType browser, string minVersion)
        {                       
            return (Browser == browser && (ParseVersion(DisplayVersion) >= ParseVersion(minVersion)));
        }

        // Parse the browser's useragent string to detect browser type and version
        // Based on:
        //      Bowser - a browser detector
        //      https://github.com/ded/bowser
        //      MIT License | (c) Dustin Diaz 2014
        private void Parse(string userAgent)
        {            
            string iosdevice = getFirstMatch(userAgent, @"(ipod|iphone|ipad)");
            if (iosdevice != null)
            {
                iosdevice = iosdevice.ToLower();
                IsiOS = true;
            }
            bool likeAndroid = contains(userAgent, @"like android");
            IsAndroid = !likeAndroid && contains(userAgent, @"android");
            string versionIdentifier = getNamedGroup(userAgent, "version", @"version\/(?<version>\d+(\.\d+)?)");
            IsTablet = contains(userAgent, @"tablet");
            IsMobile = !IsTablet && contains(userAgent, @"[^-]mobi");

            if (contains(userAgent, @"opera|opr"))
            {
                DisplayName = "Opera";
                Browser = BrowserType.Opera;
                DisplayVersion = (versionIdentifier != null ? versionIdentifier : getFirstMatch(userAgent, @"(?:opera|opr)[\s\/](\d+(\.\d+)?)"));
            } 
            else if (contains(userAgent, @"windows phone"))
            {
                DisplayName = "Windows Phone";
                Browser = BrowserType.WindowsPhone;
                DisplayVersion = getFirstMatch(userAgent, @"iemobile\/(\d+(\.\d+)?)");
            } 
            else if (contains(userAgent, @"msie|trident"))
            {
                DisplayName = "Internet Explorer";
                Browser = BrowserType.IE;
                DisplayVersion = getNamedGroup(userAgent, "version", @"(?:msie |rv\:)(?<version>\d+(\.\d+)?)");
            } 
            else if (contains(userAgent, @"chrome|crios|crmo"))
            {
                DisplayName = "Chrome";
                Browser = BrowserType.Chrome;
                DisplayVersion = getNamedGroup(userAgent, "version", @"(?:chrome|crios|crmo)\/(?<version>\d+(\.\d+)?)");
            } 
            else if (iosdevice != null)
            {
                if (iosdevice == "iphone")
                {
                    DisplayName = "iPhone";
                    Browser = BrowserType.iPhone;
                }
                else if (iosdevice == "ipad")
                {
                    DisplayName = "iPad";
                    Browser = BrowserType.iPad;
                }
                else
                {
                    DisplayName = "iPod";
                    Browser = BrowserType.iPod;
                }
                DisplayVersion = versionIdentifier;
            } 
            else if (contains(userAgent, @"sailfish"))
            {
                DisplayName = "Sailfish";
                Browser = BrowserType.Sailfish;
                DisplayVersion = getFirstMatch(userAgent, @"sailfish\s?Browser\/(\d+(\.\d+)?)");
            } 
            else if (contains(userAgent, @"seamonkey\/"))
            {
                DisplayName = "SeaMonkey";
                Browser = BrowserType.SeaMonkey;
                DisplayVersion = getFirstMatch(userAgent, @"seamonkey\/(\d+(\.\d+)?)");
            } 
            else if (contains(userAgent, @"firefox|iceweasel"))
            {
                DisplayName = "Firefox";
                Browser = BrowserType.Firefox;
                DisplayVersion = getNamedGroup(userAgent, "version", @"(?:firefox|iceweasel)[ \/](?<version>\d+(\.\d+)?)");
            } 
            else if (contains(userAgent, @"silk"))
            {
                DisplayName = "Amazon Silk";
                Browser = BrowserType.Silk;
                DisplayVersion = getFirstMatch(userAgent, @"silk\/(\d+(\.\d+)?)");
            } 
            else if (IsAndroid)
            {
                DisplayName = "Android";
                Browser = BrowserType.Android;
                DisplayVersion = versionIdentifier;
            } 
            else if (contains(userAgent, @"phantom"))
            {
                DisplayName = "PhantomJS";
                Browser = BrowserType.PhantomJS;
                DisplayVersion = getFirstMatch(userAgent, @"phantomjs\/(\d+(\.\d+)?)");
            } 
            else if (contains(userAgent, @"blackberry|\bbb\d+") || contains(userAgent, @"rim\stablet"))
            {
                DisplayName = "BlackBerry";
                Browser = BrowserType.BlackBerry;
                DisplayVersion = (versionIdentifier != null ? versionIdentifier : getFirstMatch(userAgent, @"blackberry[\d]+\/(\d+(\.\d+)?)"));
            } 
            else if (contains(userAgent, @"(web|hpw)os"))
            {
                DisplayName = "WebOS";
                Browser = BrowserType.WebOS;
                DisplayVersion = (versionIdentifier != null ? versionIdentifier : getNamedGroup(userAgent, "version", @"w(?:eb)?osBrowser\/(?<version>\d+(\.\d+)?)"));
            } 
            else if (contains(userAgent, @"bada"))
            {
                DisplayName = "Bada";
                Browser = BrowserType.Bada;
                DisplayVersion = getFirstMatch(userAgent, @"dolfin\/(\d+(\.\d+)?)");
            } 
            else if (contains(userAgent, @"tizen"))
            {
                DisplayName = "Tizen";
                Browser = BrowserType.Tizen;
                DisplayVersion = getNamedGroup(userAgent, "version", @"(?:tizen\s?)?Browser\/(?<version>\d+(\.\d+)?)");
                if (DisplayVersion == null)
                {
                    DisplayVersion = versionIdentifier;
                }
            } 
            else if (contains(userAgent, @"safari"))
            {
                DisplayName = "Safari";
                Browser = BrowserType.Safari;
                DisplayVersion = versionIdentifier;
            } 
        }

        private static Version ParseVersion(string version)
        {
            int major = 0;
            int minor = 0;
            int build = 0;
            int revision = 0;

            string[] versionParts = version.Split('.');
            if (versionParts.Length > 3)
            {
                int.TryParse(versionParts[3], out revision);
            }
            if (versionParts.Length > 2)
            {
                int.TryParse(versionParts[2], out build);
            }
            if (versionParts.Length > 1)
            {
                int.TryParse(versionParts[1], out minor);
            }
            if (versionParts.Length > 0)
            {
                int.TryParse(versionParts[0], out major);
            }

            return new Version(major, minor, build, revision);
        }

        private static string getFirstMatch(string source, string pattern) 
        {
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            Match match = regex.Match(source);
            if (match.Success)
            {
                return match.Value;
            }
            
            return null;
        }
        
        private static string getNamedGroup(string source, string name, string pattern) 
        {
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(source);
            if (matches.Count > 0)
            {
                return matches[0].Groups[name].Value;
            }
            
            return null;
        }
        
        private static string getMatch(int matchIndex, string source, string pattern) 
        {
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(source);
            if (matches.Count > matchIndex)
            {
                return matches[matchIndex].Value;
            }
            
            return null;
        }
        
        private static bool contains(string source, string pattern)
        {
            return getFirstMatch(source, pattern) != null;
        }

    }
}
