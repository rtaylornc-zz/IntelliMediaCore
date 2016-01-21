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
using IntelliMedia;
using System.Collections.Generic;

namespace IntelliMedia
{
    public class Session
    {   
        public Session()
        {
            CreatedDate = System.DateTime.Now;
        }

        [RepositoryKey]
        public string Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public String UserRole { get; set; }
        public String UserId { get; set; }
        public String Country { get; set; }
        public String Region { get; set; }
        public String City { get; set; }
        public String Latitude { get; set; }
        public String Longitude { get; set; }

        public String Platform { get; set; }

        public String GameVersion  { get; set; }
        public String GameReleaseType { get; set; }          
        public String OperatingSystem { get; set; }
        public String WebBrowser { get; set; }
        public String WebBrowserVersion { get; set; }
        public String ProcessorType { get; set; }
        public String ProcessorCount { get; set; }
        public String SystemMemorySize { get; set; }
        public String GraphicsMemorySize { get; set; }
        public String GraphicsDeviceName { get; set; }
        public String GraphicsShaderLevel { get; set; }
    }
}
