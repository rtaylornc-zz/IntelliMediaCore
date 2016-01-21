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
using System.Collections;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using IntelliMedia.DecisionMaking;

namespace IntelliMedia
{
    public class IlluminateManager : MonoBehaviour 
    {
        public bool ShowStats;
        const double PerFrame = 33.3;  // 30 fps

        [Range(0,1)]
        public double MaxNormalizedFrameTime;
        double AiPerFrame
        {
            get
            {
                return Time.deltaTime * 1000 * MaxNormalizedFrameTime;
            }
        }

        Stopwatch stopwatch = new Stopwatch();

        private static IlluminateManager instance;
        public static IlluminateManager Instance
        {
            get
            {
                if (instance == null)
                {
                    IlluminateManager[] found = GameObject.FindObjectsOfType<IlluminateManager>();
                    if (found.Length == 1)
                    {
                        instance = found[0];
                    }
                    else if (found.Length > 1)
                    {
                        bool first = true;
                        StringBuilder gameObjectNames = new StringBuilder();
                        foreach (IlluminateManager manager in found)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                            {
                                gameObjectNames.Append(", ");
                            }
                            gameObjectNames.Append(manager.gameObject.name);
                        }
                        DebugLog.Error("Only a single GameObject with a IlluminateManager can exist in a scene. Found {0} GameObjects with IlluminateManager component: {1}",
                                         found.Length, gameObjectNames.ToString());

                        // Default to the first game object found
                        instance = found[0];
                    }
                    else
                    {
                        GameObject gameObject = new GameObject(typeof(IlluminateManager).Name,
                                                               typeof(IlluminateManager));
                        instance = gameObject.GetComponent<IlluminateManager>();
                        DontDestroyOnLoad(instance);
                    }
                }

                return instance;
            }
        }

        public static void DispatchCoroutine(IEnumerator routine)
        {
            Instance.StartCoroutine(routine);
        }

        private readonly Blackboard globalKnowledge = new Blackboard();
        public Blackboard GlobalKnowledge
        {
            get
            {
                return globalKnowledge;
            }
        }

        void Update()
        {
            stopwatch.Reset();
            stopwatch.Start();
            Scheduler.Global.Execute(AiPerFrame);
            stopwatch.Stop();
        }

        void OnGUI()
        {
            if (!ShowStats)
            {
                return;
            }

            int w = Screen.width, h = Screen.height;
            
            GUIStyle style = new GUIStyle();
            
            Rect rect = new Rect(AspectUtility.Current.offsetX, AspectUtility.Current.offsetY, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 100;
            if (stopwatch.ElapsedMilliseconds > AiPerFrame)
            {
                style.normal.textColor = Color.red;
            }
            else
            {
                style.normal.textColor = Color.green;
            }

            string text = string.Format("Illuminate: {0:0.0} ms; {1:0.0}% frame", 
                                        stopwatch.ElapsedMilliseconds,
                                        stopwatch.Elapsed.TotalSeconds/Time.deltaTime * 100);
            GUI.Label(rect, text, style);
        }
    }
}
