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
using System.Collections.Generic;

namespace IntelliMedia
{
    public class AspectUtility : UniqueMonoBehaviour<AspectUtility> {
       
        public static readonly Rect NativeScreenSize = new Rect(0, 0, 1920.0f, 1200.0f);
        public float wantedAspectRatio = 1.6f;
        public float guiNativeWidth = 1920.0f;
        public float guiNativeHeight = 1200.0f;
    	
    	public Texture2D blackSwab;

        private Rect aspectAdjustedRect;

        public Rect AspectAdjustedRect
        {
            get
            {
                return aspectAdjustedRect;
            }
        }

        public Rect ScreenPixelRect
        {
            get
            {
                return m_screenRect;
            }
        }

    	private Camera m_backgroundCam;
    	private Rect m_screenRect;
        private Rect m_originalCameraRect = new Rect(0, 0, 1, 1);
    	private int currentWidth = 0;
    	private int currentHeight = 0;

        /// <summary>
        /// Container of observers that wish to receive quest update information
        /// </summary>
        private List<GameObject> observers = new List<GameObject>();
        
        /// <summary>
        /// Matrix used to transform from screen space to constrained aspect sized screen
        /// </summary>
        private Matrix4x4 matrix;

        public Matrix4x4 Matrix
        {
            get
            {
                return matrix;
            }
        }

        public void RotateAroundPivot(float angle, Vector2 pivot)
        {
            Vector3 transformed = GUI.matrix.MultiplyPoint(new Vector3(pivot.x, pivot.y, 0));
            GUIUtility.RotateAroundPivot(angle, new Vector2(
                transformed.x, 
                transformed.y));
        }
       
        public void Start()
    	{
    		m_screenRect = new Rect(0.0f, 0.0f, Screen.width, Screen.height);
            
            // Make a new camera behind the normal camera which displays black; otherwise the unused space is undefined
            m_backgroundCam = new GameObject("BackgroundCam", typeof(Camera)).GetComponent<Camera>();
    		m_backgroundCam.clearFlags = CameraClearFlags.SolidColor;
    		m_backgroundCam.cullingMask = 0;
            m_backgroundCam.depth = -10;//m_Camera.depth-10;
            m_backgroundCam.backgroundColor = Color.black;
    		
    		matrix = Matrix4x4.TRS(
                		new Vector3(offsetX, offsetY, 0), 
                		Quaternion.identity, 
                		new Vector3(adjustedWidth / guiNativeWidth, adjustedHeight / guiNativeHeight, 1));
    	}

        public void OnLevelWasLoaded()
        {
    		currentWidth = 0;
    		currentHeight = 0;

            // Make a new camera behind the normal camera which displays black; otherwise the unused space is undefined
            m_backgroundCam = new GameObject("BackgroundCam", typeof(Camera)).GetComponent<Camera>();
            m_backgroundCam.clearFlags = CameraClearFlags.SolidColor;
            m_backgroundCam.cullingMask = 0;
            m_backgroundCam.depth = -10;// m_Camera.depth - 10;
            m_backgroundCam.backgroundColor = Color.black;
        }

    	public void OnResolutionChanged()
    	{
            int screenWidth = Screen.width;
            int screenHeight = Screen.height;

            float currentAspectRatio = (float)screenWidth / (float)screenHeight;
    		// TODO rtaylor 2011-04-04 Determine if there is a more efficient way to detect
            // resolution change so we don't have to call this every tick.
            //Debug.Log("AspectUtility.OnResolutionChanged: "+screenWidth+", "+screenHeight);
                // Pillarbox
                if (currentAspectRatio > wantedAspectRatio)
                {
                    float inset = 1.0f - wantedAspectRatio / currentAspectRatio;
                    aspectAdjustedRect = new Rect(inset / 2 + m_originalCameraRect.x * (1.0f - inset),
                        m_originalCameraRect.y,
                        m_originalCameraRect.width * (1.0f - inset),
                        m_originalCameraRect.height);

                    m_screenRect.x = Mathf.Floor(screenWidth * (inset / 2));
                    m_screenRect.y = 0.0f;
                    m_screenRect.width = Mathf.Ceil(screenWidth * (1.0f - inset));
                    m_screenRect.height = screenHeight;
                }
                // Letterbox
                else
                {
                    float inset = 1.0f - currentAspectRatio / wantedAspectRatio;
                    aspectAdjustedRect = new Rect(m_originalCameraRect.x,
                        inset / 2 + m_originalCameraRect.y * (1.0f - inset),
                        m_originalCameraRect.width,
                        m_originalCameraRect.height * (1.0f - inset));

                    m_screenRect.x = 0.0f;
                    m_screenRect.y = Mathf.Floor(screenHeight * (inset / 2));
                    m_screenRect.width = screenWidth;
                    m_screenRect.height = Mathf.Ceil(screenHeight * (1.0f - inset));
                }
    		
    		currentWidth = screenWidth;
    		currentHeight = screenHeight;

            matrix = Matrix4x4.TRS(
                new Vector3(offsetX, offsetY, 0),
                Quaternion.identity,
                new Vector3(adjustedWidth / guiNativeWidth, adjustedHeight / guiNativeHeight, 1));
        }
    	
    	public void Update() 
    	{
    		if (Screen.width != currentWidth || Screen.height != currentHeight)
    		{
    			OnResolutionChanged();
                SendMessageToAllObservers("OnResolutionChanged", null);
    		}
    	}

        public int adjustedHeight {
            get {
                return (int)m_screenRect.height;
            }
        }
       
        public int adjustedWidth {
            get {
                return (int)m_screenRect.width;
            }
        }

        public int offsetX {
            get {
                return (int)m_screenRect.x;
            }
        }
       
        public int offsetY {
            get {
                return (int)m_screenRect.y;
            }
        }
        
        /// <summary>
        /// This method converts a rect in GUI pixel coordinates to a viewport pixel rect
        /// compatible with the Unity camera's pixelRect property.
        /// </summary>
        /// <param name="pixelRect">
        /// Position and size of camera as if it was a normal UI control <see cref="Rect"/>
        /// </param>
        /// <returns>
        /// Viewport in pixels scaled for the current screen size <see cref="Rect"/>
        /// </returns>
        public Rect GetPixelViewport(Rect pixelRect)
        {
            // The camera viewports (pixel rects) have their origin at the bottom left of the
            // screen. Thus, we need to flip the y.
            // The GUI is designed to be 1900x1200
            pixelRect.y = 1200 - (pixelRect.y + pixelRect.height);
            
            // Position the corner of the rect
            Vector3 position = matrix.MultiplyPoint(new Vector3(pixelRect.x, pixelRect.y, 0f));
            // Scale the size of the rect
            Vector3 size = new Vector3(pixelRect.width * matrix[0,0], pixelRect.height * matrix[1,1], 0f);
            
            return new Rect(position.x, position.y, size.x, size.y);
        }

        /// <summary>
        /// Register a GameObject that wishes to receive quest-related messages
        /// </summary>
        /// <param name="gameObject">
        /// Object to receive messages <see cref="GameObject"/>
        /// </param>
        public void AddObserver(GameObject gameObject)
        {
            observers.Add(gameObject);
        }

        /// <summary>
        /// Send a message to all registered listeners
        /// </summary>
        /// <param name="methodName">
        /// Name of method to call <see cref="System.String"/>
        /// </param>
        /// <param name="arg">
        /// Optional argument <see cref="System.Object"/>
        /// </param>
        private void SendMessageToAllObservers(string methodName, object arg)
        {
            GameObject obj;
            for (int i = 0; i < observers.Count; i++)
            {
                obj = observers[i];
                if (obj)
                {
                    obj.SendMessage(methodName, arg, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    RemoveObserver(obj);
                }
            }
        }

        /// <summary>
        /// Unregister an observer
        /// </summary>
        /// <param name="gameObject">
        /// Object that no longer wishes to receive messages<see cref="GameObject"/>
        /// </param>
        public void RemoveObserver(GameObject gameObject)
        {
            observers.Remove(gameObject);
        }
    	
    	// Draw black border to hide GUIs drawn outside of viewport but on top of background camera
    	void OnGUI()
    	{
    		GUI.depth = -100;
    		
    		// Draw Left Border
    		float borderWidth = offsetX;
    		GUI.DrawTexture(new Rect(0, 0, borderWidth, Screen.height), blackSwab);
    		
    		// Draw Right Border
    		GUI.DrawTexture(new Rect(offsetX + adjustedWidth, 0, 
    		                         borderWidth, Screen.height), blackSwab);
    		
    		// Draw Top Border
    		float borderHeight = (Screen.height - adjustedHeight) / 2;
    		GUI.DrawTexture(new Rect(0, 0, borderHeight, Screen.width), blackSwab);
    		
    		// Draw Bottom Border
    		GUI.DrawTexture(new Rect(0, borderHeight + adjustedHeight, Screen.width, borderHeight),
    		                blackSwab);
    	}
    }
}