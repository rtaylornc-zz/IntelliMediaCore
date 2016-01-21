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
using Newtonsoft.Json;

namespace IntelliMedia.DecisionMaking
{
    public class State
	{
        public State()
        {
            Transitions = new LinkedList<Transition>();
        }

        [JsonProperty]
        internal LinkedList<Transition> Transitions;

        DecisionMaker context;
        public DecisionMaker Context
        {
            get
            {
                return context;
            }
            
            set
            {
                if (context != value)
                {
                    context = value;
                    OnContextChanged();
                }
            }
        }
        
        protected virtual void OnContextChanged()
        {
            if (Action != null)
            {
                Action.Context = Context;
            }
            if (EntryAction != null)
            {
                EntryAction.Context = Context;
            }
            if (ExitAction != null)
            {
                ExitAction.Context = Context;
            }
            if (Transitions != null)
            {
                foreach (Transition transition in Transitions)
                {
                    if (transition.Action != null)
                    {
                        transition.Action.Context = Context;
                    }
                }
            }
        }

        public BehaviorTask Action { get; set; }
        public BehaviorTask EntryAction { get; set; }
        public BehaviorTask ExitAction { get; set; }

        public void AddTransition(Transition transition)
        {
            transition.State = this;
            if (transition.Condition != null)
            {
                transition.Condition.State = this;
            }
            Transitions.AddLast(transition);
        }

        public void RemoveTransition(Transition transition)
        {
            transition.State = null;
            if (Transitions.Remove(transition))
            {
                transition.Condition.State = null;
            }
        }

        public IEnumerable<Transition> GetTransitions()
        {
            return Transitions;
        }
    }
}