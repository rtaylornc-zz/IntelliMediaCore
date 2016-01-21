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
using System.Collections.Generic;
using UnityEngine;

namespace IntelliMedia
{
    public class SimulatedEyeTracker : EyeTracker
    {
		private static readonly System.Random RandomGenerator = new System.Random();

        public float SimulatedEyeTrackingFrequency { get; set; }

        private GazeEventArgs gazeLocation;
		private DateTime timeOfLastUpdate;

        public SimulatedEyeTracker()
        {
            IsCalibrationRequired = true;
            SimulatedEyeTrackingFrequency = -1;
        }

        private void ValidateSettings()
        {                        
            if (SimulatedEyeTrackingFrequency < 0) 
            {
                throw new Exception("SimulatedEyeTrackingFrequency is not set");
            }
        }

        public override void Connect() 
        {
            ValidateSettings();

            IsConnected = true;
            IsCalibrated = true;
        }

		public override void Calibrate(CalibrationResultHandler callback)
		{
			Dictionary<string, object> calibrationProperties = new Dictionary<string, object>();
			calibrationProperties["FakeDate"] = true;

			callback(true, "Simulated eye tracker successfully initialized.", calibrationProperties);
		}

        public override void Disconnect() 
        {
            IsConnected = false;
        }

        public void SetGazeLocation(float x, float y)
        {
            gazeLocation = new GazeEventArgs() 
            {
                LeftX = x,
                LeftY = y,
				LeftPupilDiameter = SimulatedPupilSize,
                RightX = x,
                RightY = y,
				RightPupilDiameter = SimulatedPupilSize
            };
        }

		public override void Update()
		{				
            if (IsConnected)
            {
                if ((DateTime.Now - timeOfLastUpdate).TotalSeconds > 1f / SimulatedEyeTrackingFrequency)
                {
                    if (gazeLocation != null)
                    {
                        OnGazeChanged(gazeLocation);
                    }
                    else
                    {
                        // If a gaze Location hasn't been specified use 
                        // the current mouse position.
                        // mouse position in pixel coordinates
                        // The bottom-left of the screen or window is at (0, 0)
                        Vector3 mousePosition = Input.mousePosition;
                        OnGazeChanged(new GazeEventArgs()
                                      {
                                        LeftX = mousePosition.x,
                                        LeftY = mousePosition.y,
										LeftPupilDiameter = SimulatedPupilSize,
                                        RightX = mousePosition.x,
                                        RightY = mousePosition.y,
										RightPupilDiameter = SimulatedPupilSize
                                      });
                    }
                    timeOfLastUpdate = DateTime.Now;
                }
            }
		}

		private float SimulatedPupilSize
		{
			get
			{
				return (float)(RandomGenerator.NextDouble() * 3);
			}
		}
    }
}

