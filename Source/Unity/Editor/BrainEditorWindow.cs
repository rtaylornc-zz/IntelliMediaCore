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
using UnityEditor;
using System.IO;
using IntelliMedia.DecisionMaking;

namespace IntelliMedia
{
    public class BrainEditorWindow : EditorWindow
    {        
        public BehaviorTree behaviorTree;
        public BehaviorTask selected;

        float panX = 0;
        float panY = 0;

        [MenuItem("Window/Illuminate/Brain Editor")]   
        public static void ShowEditor ()
        {     
            EditorWindow.GetWindow<BrainEditorWindow> ();         
        }
            
        void OnGUI ()
        {
            GUI.BeginGroup (new Rect (panX, panY, 100000, 100000));

            if (behaviorTree != null)
            {
                Rect nodeRect = new Rect(50, 30, 150, 50);
                GUI.Label(nodeRect, string.Format("{0} - {1} nodes", behaviorTree.GetType().Name, 99));
                foreach (BehaviorTask task in behaviorTree.Nodes())
                {
                    string nodeText = string.Format("{0}\n{1}", task.GetType().Name, GetStatus(task));
                    nodeRect.y += 55;
                    if (GUI.Toggle(nodeRect, (selected == task), nodeText, GUI.skin.button))
                    {
                        selected = task;
                    }
                    else if (selected == task)
                    {
                        selected = null;
                    }

                }
             }
    //        EditorGUILayout.EndVertical();

            GUI.EndGroup ();    
             
            if (GUI.RepeatButton (new Rect (15, 5, 20, 20), "^")) {
                panY -= 1;
                Repaint ();
            }
                
            if (GUI.RepeatButton (new Rect (5, 25, 20, 20), "<")) {
                panX -= 1;
                Repaint ();
            }
                
            if (GUI.RepeatButton (new Rect (25, 25, 20, 20), ">")) {
                panX += 1;
                Repaint ();
            }
                
            if (GUI.RepeatButton (new Rect (15, 45, 20, 20), "v")) {
                panY += 1;
                Repaint ();
            }
        }

        string GetStatus(ISchedulable task)
        {
            if (task.IsWaiting)
            {
                return "Waiting";
            }
            else if (task.IsRunning)
            {
                return "Running";
            }
            else
            {
                return "Finished";
            }
        }

        public void OnInspectorUpdate()
        {
            // This will only get called 10 times per second.
            Repaint();
        }

        void OnSelectionChange ()
        {
            Brain asset = Selection.activeObject as Brain;
            if (asset != null)
            {
                behaviorTree = asset.DecisionMaker as BehaviorTree;
                Repaint();
            }
        }
    }
}
