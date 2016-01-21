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
    public class StateMachine : DecisionMaker
    {
        Queue<BehaviorTask> ActionQueue = new Queue<BehaviorTask>();

        public readonly State StartState = new State();
        public readonly State EndState = new State();

        Transition StartTransition = new Transition() { Condition = new AlwaysTrueCondition() };

        public StateMachine()
        {
            StartState.AddTransition(StartTransition);
            Restart();
        }

        public override void Execute (double timeout)
        {
            UpdateState();
            while (ActionQueue.Count > 0)
            {
                BehaviorTask action = ActionQueue.Dequeue();
                action.Start();
            }
        }

        public bool IsInEndState
        {
            get
            {
                return CurrentState == EndState;
            }
        }

        public void Restart()
        {
            CurrentState = StartState;
        }

        State initialState;
        public State InitialState 
        { 
            get { return initialState; } 
            set { initialState = value; StartTransition.TargetState = initialState; }
        } 

        public State currentState;
        public State CurrentState 
        { 
            get
            {
                return currentState;
            }
            set
            {
                if (currentState != value)
                {
                    if (currentState != null)
                    {
                        currentState.Context = null;
                    }

                    currentState = value;
                    DebugLog.Info("--> Current State: {0}", (currentState != null ? currentState.ToString() : "null"));
                                                         
                    if (currentState != null)
                    {
                        currentState.Context = this;
                    }
                }
            }
        }

        public void UpdateState()
		{
            Transition triggeredTransition = null;

            foreach (Transition transition in CurrentState.GetTransitions())
            {
                if (transition.IsTriggered)
                {
                    triggeredTransition = transition;
                    break;
                }
            }

            if (triggeredTransition != null)
            {
                if (CurrentState.ExitAction != null)
                {
                    ActionQueue.Enqueue(CurrentState.ExitAction);
                }

                if (triggeredTransition.Action != null)
                {
                    ActionQueue.Enqueue(triggeredTransition.Action);
                }

                if (triggeredTransition.TargetState.EntryAction != null)
                {
                    ActionQueue.Enqueue(triggeredTransition.TargetState.EntryAction);
                }

                CurrentState = triggeredTransition.TargetState;

                if (CurrentState.Action != null)
                {
                    ActionQueue.Enqueue(CurrentState.Action);
                }            
            }
        }
	}
}
