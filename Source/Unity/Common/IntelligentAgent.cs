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
using UnityEngine;
using System.Collections;

namespace IntelliMedia
{
	public class IntelligentAgent : MonoBehaviour 
	{
        private Brain previousBrain;
        public Brain brain;

        void OnEnable()
        {
            if (brain != null)
            {
                brain.DecisionMaker.IsEnabled = true;
            }
        }

        void OnDisable() 
        {
            if (brain != null)
            {
                brain.DecisionMaker.IsEnabled = false;
            }
        }

        void Start()
        {
        }

        public void Interrupt()
        {
            if (brain != null && brain.DecisionMaker != null)
            {
                brain.DecisionMaker.Interrupt();
            }
        }

        void Update()
        {
            // Detect field change
            if (brain != previousBrain)
            {
                if (brain != null)
                {
                    if (brain.DecisionMaker != null)
                    {
                        DebugLog.Info("IntelligentAgent set NavigationController");
                        NavigationController navController = GetComponent<NavigationController>();
                        if (navController != null)
                        {
                            brain.DecisionMaker.LocalKnowledge.Set("NavigationController", navController);
                        }
                        brain.DecisionMaker.IsEnabled = enabled;
                    }
                    else
                    {
                        DebugLog.Error("DecisionMaker not set in {0}", brain.name);
                    }
                }
                previousBrain = brain;
            }
        }
	}
}
