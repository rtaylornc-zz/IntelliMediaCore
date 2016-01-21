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
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using IntelliMedia.DecisionMaking;

namespace IntelliMedia
{
    [Serializable]
    public class Brain : ScriptableObject
	{
        // TODO rgtaylor 2014-09-10 Newtonsoft JSON is not compatible with WebPlayer, replace this
        // before deploying
        private ISerializer serialize = SerializerJson.Instance;

        [SerializeField]
        public TextAsset JsonAsset;

        [SerializeField]
        public string serializedDecisionMaker;

        public DecisionMaker DecisionMaker { get; set; }

        public void OnEnable ()
        {
            OnLoad();
        }

        public void OnLoad()
        {
            if (!string.IsNullOrEmpty(serializedDecisionMaker))
            {
                DecisionMaker = serialize.Deserialize<DecisionMaker>(serializedDecisionMaker);
            }
            else
            {
                DecisionMaker = null;
            }
        }

        public void OnSave()
        {
            if (DecisionMaker != null)
            {
                serializedDecisionMaker = serialize.Serialize<DecisionMaker>(DecisionMaker);
            }
            else
            {
                serializedDecisionMaker = null;
            }
        }

        // Example usage
        //        Illuminate.StateMachine baseDialog = new Illuminate.StateMachine();
        //        
        //        Illuminate.State intro = new Illuminate.State();
        //        Illuminate.State quest = new Illuminate.State();
        //        Illuminate.State bye = new Illuminate.State();
        //        
        //        
        //        intro.Action = new ConversationSay("Hello!");
        //        intro.AddTransition(new ConversationTransition("Nice to meet you.")
        //                                {
        //            TargetState = quest
        //        });
        //        
        //        quest.Action = new ConversationSay("Can you take this package to Bryce?");
        //        quest.AddTransition(new ConversationTransition("No.")
        //                                 {
        //            TargetState = bye
        //        });
        //        
        //        bye.Action = new ConversationSay("Bye! Bye!");
        //        bye.AddTransition(new ConversationTransition("Later.")
        //                             {
        //            TargetState = baseDialog.EndState
        //        });
        //        
        //        baseDialog.InitialState = intro;
        //
        //        BehaviorTree behaviorTree = new BehaviorTree();
        //        behaviorTree.Root = 
        //            new Selector(
        //                new Sequence(
        //                    new ConversationRequest(),
        //                    new ConversationStart(baseDialog)),
        //                new Sequence(
        //                    new SelectDestination("TestDest1", "TestDest2", "TestDest3"),
        //                    new MoveTo(),
        //                    new Wait(5)));
        //        
        //      IntelligentAgent kim = GameObject.Find("KimNew").GetComponent<IntelligentAgent>();
        //
        //        behaviorTree.Name = "Kim";
        //        kim.brain = ScriptableObject.CreateInstance<Brain>();
        //        kim.brain.name = "KimDecisionMaker";
        //        kim.brain.DecisionMaker = behaviorTree;
	}
}
