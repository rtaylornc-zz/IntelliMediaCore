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
namespace IntelliMedia
{
    public class TaskProgress 
	{
        public delegate void UpdatedHandler(TaskProgress sender);
        public event UpdatedHandler Updated;

        public float PercentComplete { get; private set; }
        public bool IsFinished { get { return PercentComplete >= 100.0f; } }
        public string StatusMessage { get; private set; }

        public bool IsCancelled { get; private set; }

        public void Cancel()
        {
            IsCancelled = true;
        }

        public void Update(float percentComplete, string statusMessage = null)
        {
            PercentComplete = percentComplete;
            if (statusMessage != null)
            {
                StatusMessage = statusMessage;
            }

            NotifyObservers();
        }

        public void Finished()
        {
            PercentComplete = 100.0f;
            StatusMessage = "";

            NotifyObservers();

        }

        private void NotifyObservers()
        {
            if (Updated != null)
            {
                Updated(this);
            }
        }
    }
}
