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
using System.Collections.Generic;
 
namespace IntelliMedia
{
    /// <summary>
    /// The purpose of this button is to wrap the Unity 3D button and properly handle mouse presses
    /// for buttons that overlap.
    /// http://forum.unity3d.com/threads/96563-corrected-GUI.Button-code-%28works-properly-with-layered-controls%29?p=629284#post629284
    /// </summary>
    public class UnityListBox<T>
    {
        public Texture2D background;
        public GUIStyle titleStyle;
        public GUIStyle buttonStyle;

        public string Title { get; set; }
        public IEnumerable<T> Items { get; set; }
        public T SelectedValue { get; set; }

        private Vector2 scrollPos = new Vector2(0, 0); 


        public void Draw(UnityEngine.Rect bounds)
        {
            if (background != null)
            {
                GUI.DrawTexture(bounds, background);
            }
            
            GUILayout.BeginArea(bounds);
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            GUILayout.BeginVertical();
               
            if (!string.IsNullOrEmpty(Title))
            {
                GUILayout.Label(Title, titleStyle);
                GUILayout.Space(20);
            }
            
            foreach (T item in Items)
            {
                if (item != null)
                {
                    if (GUILayout.Toggle((item.Equals(SelectedValue)), item.ToString(), buttonStyle, 
                                         GUILayout.Height(buttonStyle.fontSize), GUILayout.Width(bounds.width)))
                    {
                        SelectedValue = item;
                    }
                }
            }
            
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }         
    }
}