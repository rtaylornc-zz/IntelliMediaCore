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

namespace IntelliMedia
{
    /// <summary>
    /// This class dynamically adjusts the quality setting for the game based on the frame rate.
    /// In addition, it provides an onscreen printout of the frame rate as well as build information
    /// for the game. The printout can be toggled by pressing RIGHT ALT + BACK QUOTE.
    /// </summary>
    public class QualityAdjuster : MonoBehaviour
    {
        private static readonly string AutomaticallyAdjustQualityKey = "AutomaticallyAdjustQuality";
        private static readonly string ManualQualityLevelKey = "ManualQualityLevel";

        public float calculateFpsInterval = 0.5f;   // Each frame rate sample is calculated over this interval
        public float averageFpsInterval = 5.0f;     // Frame rate is the average of all samples over this interval
        
        public bool automaticallyAdjustQuality = true;
        public bool automaticallyAdjustQualityInEditor = false;
        public int manualQualityLevel;

        public float decreaseQualityFps = 21.0f;    // Quality will be decreased if this FPS is reached
        public float increaseQualityFps = 54.0f;    // Quality will be increased if this FPS is reached
        public float qualityUpdateDelay = 20.0f;    // Quality will not update until this much time passed

    	public bool KeepOnLevelLoad = true;

        public Rect displayRect = new Rect(10, 10, 100, 80);
        public GUIStyle displayStyle;
        public float fps { get; private set; }
        private string displayText = "";
        private bool showDisplay;

        private float accumulatedDuringInterval = 0;
        private int framesDuringInterval = 0;
        private float timeRemainingInInterval;

        private float[] frameRateSamples;
        private int currentSampleIndex = -1;

        private float lastQualityAdjustmentTime;

        void Start()
        {
    		if (KeepOnLevelLoad)
    		{
    			DontDestroyOnLoad(this);
    		}

            // Clamp averageOverInterval and updateInterval to reasonable values
            this.calculateFpsInterval = Mathf.Clamp(this.calculateFpsInterval, 0.02f, 1.0f);
            this.averageFpsInterval = Mathf.Clamp(this.averageFpsInterval, 1.0f, 120.0f);

            int numberOfSamples = (int)(this.averageFpsInterval / this.calculateFpsInterval);
            this.frameRateSamples = new float[numberOfSamples];

            this.framesDuringInterval = 0;
            this.accumulatedDuringInterval = 0.0f;
            this.timeRemainingInInterval = this.calculateFpsInterval;
            this.lastQualityAdjustmentTime = Time.unscaledTime;

            showDisplay = false;

            // Restore overrride
            automaticallyAdjustQuality = (PlayerPrefs.GetInt(AutomaticallyAdjustQualityKey) != 0);
            manualQualityLevel = PlayerPrefs.GetInt(ManualQualityLevelKey);

            if (Application.isEditor)
            {
                Debug.Log("QualityAdjuster is active, use RIGHT ALT + BACK QUOTE to view FPS.");
            }
        }

        void OnDestroy()
        {
            // Save override
            PlayerPrefs.SetInt(AutomaticallyAdjustQualityKey, (automaticallyAdjustQuality ? 1 : 0));
            PlayerPrefs.SetInt(ManualQualityLevelKey, manualQualityLevel);
        }

        public void OnLevelWasLoaded(int level)
        {
            // Let's reset counters each time a level is loaded
            this.framesDuringInterval = 0;
            this.accumulatedDuringInterval = 0.0f;
            this.timeRemainingInInterval = this.calculateFpsInterval;
            this.lastQualityAdjustmentTime = Time.unscaledTime;
        }

        public void OnGUI()
        {
            if (showDisplay)
            {
                GUI.depth = -2000;
                GUI.Label(displayRect, displayText, displayStyle);
            }
        }

        public void Update()
        {
            if (Input.GetKey(KeyCode.RightAlt) && Input.GetKeyDown(KeyCode.BackQuote))
            {
                showDisplay = !showDisplay;
            }

            this.timeRemainingInInterval -= Time.unscaledDeltaTime;
            this.accumulatedDuringInterval += Time.unscaledDeltaTime;
            this.framesDuringInterval += 1;

            // Calculate a new sample at the end of each interval
            if (this.timeRemainingInInterval <= 0.0)
            {
                float fpsSample =  this.framesDuringInterval / this.accumulatedDuringInterval;

                if (this.currentSampleIndex < 0)
                {
                    for (int i = 0; i < this.frameRateSamples.Length; ++i)
                    {
                        this.frameRateSamples[i] = fpsSample;
                    }
                    this.currentSampleIndex = 0;
                }

                this.frameRateSamples[this.currentSampleIndex] = fpsSample;
                this.currentSampleIndex = (this.currentSampleIndex + 1) % this.frameRateSamples.Length;

                fps = 0.0f;
                for (int i = 0; i < this.frameRateSamples.Length; ++i)
                {
                    fps += this.frameRateSamples[i];
                }
                fps /= this.frameRateSamples.Length;

                string format = System.String.Format("{0:F2} FPS {1}",
                    fps, QualitySettings.names[QualitySettings.GetQualityLevel()]);

                displayText = format;

                if (this.automaticallyAdjustQuality && (!Application.isEditor || this.automaticallyAdjustQualityInEditor) &&
                    ((Time.unscaledTime - this.lastQualityAdjustmentTime) > this.qualityUpdateDelay))
                {
                    if (fps > this.increaseQualityFps)
                    {
                        if (QualitySettings.GetQualityLevel() < (QualitySettings.names.Length - 1))
                        {
                            QualitySettings.IncreaseLevel(true);
                            this.lastQualityAdjustmentTime = Time.unscaledTime;
                        }
                    }
                    else if (fps < this.decreaseQualityFps)
                    {
                        if (QualitySettings.GetQualityLevel() > 0)
                        {
                            QualitySettings.DecreaseLevel(true);
                            this.lastQualityAdjustmentTime = Time.unscaledTime;
                        }
                    }
                }
                else if (!this.automaticallyAdjustQuality)
                {
                    if (QualitySettings.GetQualityLevel() != manualQualityLevel)
                    {
                        QualitySettings.SetQualityLevel(manualQualityLevel, true);
                    }
                }

                this.framesDuringInterval = 0;
                this.accumulatedDuringInterval = 0.0f;
                this.timeRemainingInInterval = this.calculateFpsInterval;
            }
        }
    }
}
