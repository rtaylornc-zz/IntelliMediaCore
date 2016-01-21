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
using System.Text;

namespace IntelliMedia
{
    public class RepositoryLogger : FileSystemRepository<LogEntry>, ILogger
    {   
        public RepositoryLogger(string pathToDataDirectory, ISerializer serializer, bool enabled = true) : base(pathToDataDirectory, serializer)
        {
            Enabled = enabled;
        }

        #region ILogger implementation

        private bool enabled;
        public bool Enabled
        {
            get { return enabled; } 
            set { if (enabled != value) { enabled = value; OnEnabledChanged(); } }
        }
        
        private void OnEnabledChanged()
        {
            DebugLog.Info("RepositoryLogger: {0} Enabled={1}", DataDirectory, Enabled); 
        }

        public void Write(LogEntry entry)
        {
            if (Enabled)
            {
                Insert(entry, (Response response) =>
                {
                    if (!response.Success)
                    {
                        DebugLog.Error("RepositoryLogger unabled to log. " + response.Error);
                    }
                });
            }
        }

        public void Dispose()
        {
        }
        
        #endregion
    }
}

