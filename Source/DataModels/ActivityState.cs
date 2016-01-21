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
using System.Collections.Generic;
using System;

namespace IntelliMedia
{
    public class ActivityState
    {   
        public ActivityState()
        {
            CreatedDate = System.DateTime.Now;
            Clear();
        }

        [RepositoryKey]
        public string Id { get; set; }

        public string TraceId { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public bool IsComplete { get; set; }

        public int TotalLaunches { get; set; }
        public int TotalRestarts { get; set; }
               
        public string ActivityId { get; set; }
        public string StudentId { get; set; }

        public Hashtable<string, string> SummaryData { get; set; }
        public string GameData { get; set; }

        public bool CanResume
        {
            get
            {
                return GameData != null;
            }
        }

        public void Clear()
        {
            TraceId = Guid.NewGuid().ToString();
            IsComplete = false;
            GameData = null;
            SummaryData = new Hashtable<string, string>();
        }

        public void RecordLaunch()
        {
            ++TotalLaunches;
        }

        public void Restart()
        {
            Clear();
            ++TotalRestarts;
        }
    }
}
