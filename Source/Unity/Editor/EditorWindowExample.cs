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
using System.Collections;
using System.Collections.Generic;

namespace IntelliMedia
{
    public class EditorWindowExample : EditorWindow
    {
        List<Node> nodes = new List<Node> ();
        
        
        [MenuItem ("Window/EditorWindow example")]
        static void Launch ()
        {
            GetWindow<EditorWindowExample> ().titleContent = new GUIContent("Example");
        }
        
        
        void OnGUI ()
        {
            GUILayout.Label ("This is an editor window - the base of any completely custom GUI work.", EditorStyles.wordWrappedMiniLabel);
            
            // Render all connections first //
            
            if (Event.current.type == EventType.repaint)
            {
                foreach (Node node in nodes)
                {
                    foreach (Node target in node.Targets)
                    {
                        Node.DrawConnection (node.Position, target.Position);
                    }
                }
            }
            
            GUI.changed = false;
            
            foreach (Node node in nodes)
                // Handle all nodes
            {
                node.OnGUI ();
            }
            
            wantsMouseMove = Node.Selection != null;
            // If we have a selection, we're doing an operation which requires an update each mouse move
            
            switch (Event.current.type)
            {
            case EventType.mouseUp:
                // If we had a mouse up event which was not handled by the nodes, clear our selection
                Node.Selection = null;
                Event.current.Use ();
                break;
            case EventType.mouseDown:
                if (Event.current.clickCount == 2)
                    // If we double-click and no node handles the event, create a new node there
                {
                    Node.Selection = new Node ("Node " + nodes.Count, Event.current.mousePosition);
                    nodes.Add (Node.Selection);
                    Event.current.Use ();
                }
                break;
            }
            
            if (GUI.changed)
                // Repaint if we changed anything
            {
                Repaint ();
            }
        }
    }
}