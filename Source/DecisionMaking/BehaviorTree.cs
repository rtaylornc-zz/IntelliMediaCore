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
	public class BehaviorTree : DecisionMaker
	{
        public BehaviorTree()
        {
        }

        BehaviorTask root;
		public BehaviorTask Root 
        { 
            get
            {
                return root;
            }
            set
            {
                if (value != root)
                {
                    if (root != null)
                    {
                        root.Context = null;
                    }

                    root = value;

                    if (root != null)
                    {
                        root.Context = this;
                    }
                }
            }
        }

        public override void Execute(double timeout)
        {
            if (Root != null && Root.IsFinished)
            {
                DebugLog.Info("BehaviorTree.Execute");
                Root.Start();
            }
        }

        public IEnumerable<BehaviorTask> Nodes()
        {
            return DepthFirstPreOrder(Root);
        }

        public static IEnumerable<BehaviorTask> DepthFirstPreOrder(BehaviorTask node)
        {
            if (node != null)
            {
                yield return node;

                Composite composite = node as Composite;
                if (composite != null)
                {
                    foreach (BehaviorTask child in composite.Children)
                    {
                        foreach (BehaviorTask task in DepthFirstPreOrder(child))
                        {
                            yield return task;
                        }
                    }
                }
            }

            yield break;
        }
	}
}
