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
    /// <summary>
    /// The purpose of this button is to wrap the Unity 3D button and properly handle mouse presses
    /// for buttons that overlap.
    /// http://forum.unity3d.com/threads/96563-corrected-GUI.Button-code-%28works-properly-with-layered-controls%29?p=629284#post629284
    /// </summary>
    public class UnityButton
    {
        private static int highestDepthID = 0;
#if (UNITY_IPHONE || UNITY_ANDROID)
        private static Vector2 touchBeganPosition = Vector2.zero;
#endif
        private static EventType lastEventType = EventType.Layout;
        private static bool wasDragging = false;
        private static int frame = 0;
        private static int lastEventFrame = 0;

        static public bool Button(UnityEngine.Rect bounds, GUIContent content, GUIStyle style = null, bool forceOnTop = false)
        {
            return ButtonBase(bounds, content, false, style, forceOnTop);
        }

        static public bool Button(UnityEngine.Rect bounds, string caption, GUIStyle style = null, bool forceOnTop = false)
        {
            return Button(bounds, new GUIContent(caption), style, forceOnTop);
        }

        static public bool Toggle(UnityEngine.Rect bounds, bool on, GUIContent content, GUIStyle style = null, bool forceOnTop = false)
        {
            return (ButtonBase(bounds, content, on, style, forceOnTop) ? !on : on);
        }

        static public bool Toggle(UnityEngine.Rect bounds, bool on, string caption, GUIStyle style = null, bool forceOnTop = false)
        {
            return Toggle(bounds, on, new GUIContent(caption), style, forceOnTop);
        }

        private static bool ButtonBase(UnityEngine.Rect bounds, GUIContent content, bool on, GUIStyle style, bool forceOnTop)
        {
            Contract.ArgumentNotNull("style", style);

            int controlID = GUIUtility.GetControlID(content, FocusType.Passive);
            bool isMouseOver = bounds.Contains(Event.current.mousePosition);
            int depth = (1000 - GUI.depth) * 1000 + (forceOnTop ? 10000 : controlID);
            if (isMouseOver && depth > highestDepthID)
            {
                highestDepthID = depth;
            }
    
            bool isTopmostMouseOver = (highestDepthID == depth);
    
    #if (UNITY_IPHONE || UNITY_ANDROID)  && !UNITY_EDITOR
            bool paintMouseOver = isTopmostMouseOver && (Input.touchCount > 0);
    #else
            bool paintMouseOver = isTopmostMouseOver;
    
    #endif
    
            if ( Event.current.type == EventType.Layout && lastEventType != EventType.Layout )
            {
                highestDepthID = 0;
                frame++;
            }
    
            lastEventType = Event.current.type;
            if (Event.current.type == EventType.Repaint)
            {
                bool isDown = (GUIUtility.hotControl == controlID);
                style.Draw(bounds, content, paintMouseOver, isDown, on, false);
            }

    #if (UNITY_IPHONE || UNITY_ANDROID)  && !UNITY_EDITOR
            if ( Input.touchCount > 0 )
            {
                Touch touch = Input.GetTouch(0);
                if ( touch.phase == TouchPhase.Began )
                {
                    touchBeganPosition = touch.position;
                    wasDragging = true;
                }
                else if ( touch.phase == TouchPhase.Ended
                        && ((Mathf.Abs(touch.position.x - touchBeganPosition.x) > 15)
                            || (Mathf.Abs(touch.position.y - touchBeganPosition.y) > 15)))
                {
                    wasDragging = true;
                }
                else
                {
                    wasDragging = false;
                }
            }
            else if ( Event.current.type == EventType.Repaint )
            {
                wasDragging = false;
            }
    #endif
    
            // Workaround:
            // ignore duplicate mouseUp events. These can occur when running
            // unity editor with unity remote on iOS ... (anybody knows WHY?)
            if ( frame <= (1+lastEventFrame) )
            {
                return false;
            }

            if (isMouseOver)
            {
                switch (Event.current.GetTypeForControl(controlID))
                {
                    case EventType.mouseDown:
                    {
                        //DebugLog.Info("UnityButton: Mouse DOWN over controlID={0} isTopmostMouseOver={1} depth={2} highest depth={3} wasDragging={4} content={5}",
                        //controlID, isTopmostMouseOver, depth, highestDepthID, wasDragging, content.text);
                        if (isTopmostMouseOver && !wasDragging)
                        {
                            GUIUtility.hotControl = controlID;
                            Event.current.Use();
                        }
                        break;
                    }
    
                    case EventType.mouseUp:
                    {
                        //DebugLog.Info("UnityButton: Mouse UP over controlID={0} isTopmostMouseOver={1} depth={2} highest depth={3} wasDragging={4} content={5}",
                        //controlID, isTopmostMouseOver, depth, highestDepthID, wasDragging, content.text);
                        if (isTopmostMouseOver && !wasDragging)
                        {
                            GUIUtility.hotControl = 0;
                            lastEventFrame = frame;
                            Event.current.Use();
                            return true;
                        }
                        break;
                    }
                }
            }

            return false;
        }
    }
}