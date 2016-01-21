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

namespace IntelliMedia.DecisionMaking
{
    public class DecisionMaker : ISchedulable
	{
		#if TOOL
		private static Blackboard globalKnowledge = new Blackboard();
		#else
		private static Blackboard globalKnowledge = IlluminateManager.Instance.GlobalKnowledge;
		#endif

        public DecisionMaker()
        {
            Scheduler.Enqueue(this);
        }

        private bool isEnabled;
        public bool IsEnabled 
        { 
            get
            {
                return isEnabled;
            }
            set
            {
                if (value != isEnabled) { isEnabled = value; OnEnabledChanged(); }
            }
        }

        protected virtual void OnEnabledChanged()
        {
            if (IsEnabled)
            {
                Scheduler.Global.Enqueue(scheduler);
            }
            else
            {
                Scheduler.Global.Dequeue(scheduler);
            }
        }

        public void Interrupt()
        {
            foreach (ISchedulable task in scheduler.GetAllTasks())
            {
                if (task != this)
                {
                    task.Cancel();
                }
            }
        }

        private readonly Scheduler scheduler = new Scheduler();
        public Scheduler Scheduler
        {
            get
            {
                return scheduler;
            }
        }

        public string Name
        {
            get
            {
                return LocalKnowledge.Get<string>("Name");
            }
            set
            {
                if (value != null)
                {
                    LocalKnowledge.Set("Name", value);
                }
                else
                {
                    LocalKnowledge.Unset("Name");
                }
            }
        }

        private readonly Blackboard localKnowledge = new Blackboard();
        public Blackboard LocalKnowledge
        {
            get
            {
                return localKnowledge;
            }
        }

        public Blackboard GlobalKnowledge
        {
            get
            {
				return globalKnowledge;
            }
        }

        #region ISchedulable implementation

        public virtual void Start ()
        {
        }

        public virtual void Execute(double timeout)
        {
        }

        public virtual void Cancel()
        {
        }

        public bool IsRunning {
            get {
                return true;
            }
        }

        public bool IsWaiting {
            get {
                return true;
            }
        }

		public bool IsFinished { get; protected set; }

        #endregion
	}
}
