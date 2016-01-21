//--------------------------------------------------------------------------------
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
using UnityEditor;
using System.IO;

namespace IntelliMedia
{
    [CustomEditor(typeof(Brain))]
    public class BrainPropertyEditor : Editor 
    {
        bool foo;

        public override void OnInspectorGUI()
        {
            Brain myTarget = (Brain)target;

            //BrainEditorWindow editor = null;//EditorWindow.GetWindow<BehaviorTreeEditorWindow> ();
            
            GUI.changed = false;

            EditorGUILayout.BeginVertical();

            //foo = EditorGUILayout.ToggleLeft("Foobar", foo);

            //myTarget.JsonAsset = EditorGUILayout.ObjectField(myTarget.JsonAsset, typeof(TextAsset), false) as TextAsset;

            /*
            if (!myTarget.loadFromJsonAsset)
            {
                GUI.enabled = (myTarget.DecisionMaker != null);

                if (myTarget.DecisionMaker is Illuminate.BehaviorTree)
                {
                    if (GUILayout.Button("Edit Behavior Tree"))
                    {
                        BrainEditorWindow.ShowEditor();
                    }
                }
                else if (myTarget.DecisionMaker is Illuminate.StateMachine)
                {
                    if (GUILayout.Button("Edit State Machine"))
                    {
                        BrainEditorWindow.ShowEditor();
                    }
                }

                if (GUILayout.Button("Export to JSON"))
                {
                    using (StreamWriter s = File.CreateText(myTarget.name + ".json"))
                    {
                        s.Write(myTarget.DecisionMakerJson);
                    }
                }

                GUI.enabled = true;

                if (GUILayout.Button("Import from JSON"))
                {
                    BrainEditorWindow.ShowEditor();
                }          
            }

            if (editor != null && editor.selected != null)
            {
                GUILayout.Label("Draw " + editor.selected.GetType().Name);
            }
    */
            EditorGUILayout.EndVertical();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(myTarget);
            }
            
        }
    }
}
