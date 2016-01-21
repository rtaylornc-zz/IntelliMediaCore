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
using System.Text;
using TETCSharpClient;
using TETCSharpClient.Data;

namespace IntelliMedia
{		
	public class EyeTribeEyeTracker : EyeTracker, IConnectionStateListener, IGazeListener
    {
		public int CalibrationPoints { get; set; }
		public float CalibrationPointSampleDuration { get; set; }
		public string ServerRecvAddress { get; set; }
		public int ServerRecvPort { get; set; }

		public EyeTribeEyeTracker()
		{
			ServerRecvPort = -1;
			IsCalibrationRequired = true;
		}

		private void ValidateSettings()
		{
			if (CalibrationPoints < 7) 
			{
				throw new Exception("The minimum number of calibration points allowed is 7, but it is recommended to use 9 or more. Update your eye tracker settings.");
			}

			if (CalibrationPointSampleDuration < 0.8f)
			{
				throw new Exception("The minimum sample duration is 0.8 seconds. Update your eye tracker settings.");
			}

			if (String.IsNullOrEmpty(ServerRecvAddress)) 
			{
				throw new Exception("ServerRecvAddress is null or empty");
			}

			if (ServerRecvPort < 0) 
			{
				throw new Exception("ServerRecvPort is not set");
			}
		}

		public override void Connect() 
		{
			ValidateSettings();

			DebugLog.Info("Eye Tracker: Connecting...");
			GazeManager.Instance.AddConnectionStateListener(this);

			bool success = GazeManager.Instance.Activate(
				GazeManager.ApiVersion.VERSION_1_0, 
				GazeManager.ClientMode.Push,
				ServerRecvAddress,
				ServerRecvPort);
			if (success)
			{
				DebugLog.Info("EyeTracker: Connnected to EyeTribe server: {0}:{1}", ServerRecvAddress, ServerRecvPort);
				IsConnected = true;
			}
			else
			{
				throw new Exception(string.Format("Unable to connect to EyeTribe server:  {0}:{1}", ServerRecvAddress, ServerRecvPort));
			}
		}

		public override void Disconnect() 
		{
			UnityCalibrationRunner.Abort();

			GazeManager.Instance.RemoveGazeListener(this);
			GazeManager.Instance.RemoveConnectionStateListener(this);

			DebugLog.Info("Eye Tracker: Disconnect...");
			GazeManager.Instance.Deactivate();
			DebugLog.Info("Disconnnected from EyeTracker: {0}:{1}", ServerRecvAddress, ServerRecvPort);
			IsConnected = false;
		}

		public override void Calibrate(CalibrationResultHandler callback)
		{
			IsCalibrated = false;

			DebugLog.Info("EyeTribe: Start calibration");
			UnityCalibrationRunner.StartCalibration(
				CalibrationPointSampleDuration,
				CalibrationPoints,
				(object sender, TETControls.Calibration.CalibrationRunnerEventArgs e) => 
				{
					IsCalibrated = e.CalibrationResult.Result;
					if (IsCalibrated)
					{
						GazeManager.Instance.AddGazeListener(this);
					}

					Dictionary<string, object> calibrationProperties = new Dictionary<string, object>();
					calibrationProperties["Result"] = e.Result.ToString();
					calibrationProperties["Rating"] = e.Rating;
					calibrationProperties["AverageErrorDegree"] = e.CalibrationResult.AverageErrorDegree;
					calibrationProperties["AverageErrorDegreeLeft"] = e.CalibrationResult.AverageErrorDegreeLeft;
					calibrationProperties["AverageErrorDegreeRight"] = e.CalibrationResult.AverageErrorDegreeRight;
	
					callback(IsCalibrated, e.Message, calibrationProperties);
				});
		}

		#region IGazeListener implementation

		public void OnGazeUpdate(GazeData gazeData)
		{
			OnGazeChanged(new GazeEventArgs() 
			{ 
				LeftX = (float)gazeData.LeftEye.SmoothedCoordinates.X,
				LeftY = UnityEngine.Screen.height - (float)gazeData.LeftEye.SmoothedCoordinates.Y,
				LeftPupilDiameter = (float)gazeData.LeftEye.PupilSize,
				RightX = (float)gazeData.RightEye.SmoothedCoordinates.X,
				RightY = UnityEngine.Screen.height - (float)gazeData.RightEye.SmoothedCoordinates.Y,
				RightPupilDiameter = (float)gazeData.RightEye.PupilSize
			});	
		}

		#endregion

		#region IConnectionStateListener implementation

		public void OnConnectionStateChanged(bool isConnected)
		{
			DebugLog.Info("EyeTribe server isConnected = {0}", isConnected);
		}

		#endregion
    }
}

