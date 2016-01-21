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
using System.Collections.Generic;
using Newtonsoft.Json;


namespace IntelliMedia
{
    public class CooperativeTask : ISchedulable
	{
        IEnumerator<CooperativeTaskStatus> enumerator;
        Func<IEnumerable<CooperativeTaskStatus>> workerMethod;

        public CooperativeTask()
        {
            Status = CooperativeTaskStatus.Created;
        }

        public CooperativeTask(Func<IEnumerable<CooperativeTaskStatus>> doWork) : this()
        {
            workerMethod = doWork;
        }

        internal Scheduler Scheduler { get; set; }

        [JsonIgnore]
        public CooperativeTaskStatus Status { get; private set; }

        [JsonIgnore]
        public bool IsRunning
        { 
            get 
            { 
                return Status != CooperativeTaskStatus.RanToCompletion 
                    && Status != CooperativeTaskStatus.Created
                    && Status != CooperativeTaskStatus.Canceled;
            } 
        }

        [JsonIgnore]
        public bool IsWaiting { get { return Status == CooperativeTaskStatus.Waiting; } }

        [JsonIgnore]
        public bool IsCanceled { get { return Status == CooperativeTaskStatus.Canceled; } }

        [JsonIgnore]
        public bool IsFinished { get { return !IsRunning; } }

        protected virtual void OnStart()
        {
        }

        protected virtual void OnEnd()
        {
        }

        // Schedule task to run in the scheduler
        public void Start()
        {
            Status = CooperativeTaskStatus.WaitingToRun;
            DebugLog.Info("--> Start {0}", this.GetType().Name);
            enumerator = DoWork().GetEnumerator();
            OnStart();         
            TryInlineExecute();
        }

        public virtual IEnumerable<CooperativeTaskStatus> DoWork()
        {
            if (workerMethod != null)
            {
                foreach(CooperativeTaskStatus result in workerMethod())
                {
                    yield return result;
                }
            }
            
            yield return Finished();
        }

        void TryInlineExecute()
        {
            //Execute(0);
            if (IsRunning)
            {
                Scheduler.Enqueue(this);
            }
        }

        public void Execute(double timeout)
        {
            if (enumerator != null)
            {
                enumerator.MoveNext();
                Status = enumerator.Current;
                if (Status == CooperativeTaskStatus.RanToCompletion
                 || Status == CooperativeTaskStatus.Canceled)
                {
                    End();
                }
            }
            else
            {
                throw new Exception("Attempting to execute a task that has not started.");
            }
        }

        public void End()
        {
            Scheduler.Dequeue(this);
            DebugLog.Info("<-- End {0}", this.GetType().Name);
            enumerator = null;
            OnEnd();
        }

        public void Cancel()
        {
            Status = CooperativeTaskStatus.Canceled;
        }

        protected CooperativeTaskStatus Finished()
        {
            return CooperativeTaskStatus.RanToCompletion;
        }

        protected CooperativeTaskStatus Wait()
        {
            return CooperativeTaskStatus.Waiting;
        }

        protected CooperativeTaskStatus Working()
        {
            return CooperativeTaskStatus.Running;
        }
    }
}
