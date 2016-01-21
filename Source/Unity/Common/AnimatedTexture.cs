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
using UnityEngine;

namespace IntelliMedia
{
    [Serializable]
    public class AnimatedTexture
	{
        public int framesPerSecond = 5;
        public Texture2D textureSheet;
        public int columns;
        public int rows;

        public float Width
        {
            get
            {
                return textureSheet.width/columns;
            }
        }

        public float Height
        {
            get
            {
                return textureSheet.height/rows;
            }
        }

        private int totalFrames;
        private float currentFrame;
        private int prevFrame;
        private Rect texCoords = new Rect();

        public void GUILayoutDrawTexture(float width, float height, bool alphaBlend = true)
        {
            GUILayout.Label("", GUILayout.Width(width), GUILayout.Height(height));
            Rect textureRext = GUILayoutUtility.GetLastRect();
            DrawTexture(textureRext, alphaBlend);
        }

        public void DrawTexture(Rect position, bool alphaBlend = true)
        {
            if (texCoords.width == 0 ||  texCoords.height == 0)
            {
                Restart();
            }

            GUI.DrawTextureWithTexCoords(position, textureSheet, texCoords, alphaBlend);           
        }

        public void NextFrame()
        {
            // Maintain a consistent framerate regardless of game's update rate
            currentFrame += Time.deltaTime * framesPerSecond;
            if (currentFrame >= totalFrames)
            {
                currentFrame = 0;
            }

            // Convert partial frame to int
            int frame = (int)currentFrame;

            if (frame != prevFrame)
            {
                // Texcords are normalized across the texture AND the origin is 
                // the bottom left
                texCoords = new Rect(
                    (frame%columns) / (float)columns, 
                    (rows - ((frame/columns) + 1))/ (float)rows, 
                    Width / textureSheet.width,
                    Height / textureSheet.height);
            }
        }

        public void Restart()
        {
            totalFrames = columns * rows;

            currentFrame = 0;
            prevFrame = -1;
            NextFrame();
        }
	}
}
