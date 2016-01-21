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

namespace IntelliMedia
{		
    public class SmiEyeTracker : EyeTracker
    {
        public enum DisplayDevice
        {
            Primary = 0,
            Secondary = 1
        }

        public int CalibrationPoints { get; set; }

		public string ServerSendAddress { get; set; }
		public int ServerSendPort { get; set; }

		public string ServerRecvAddress { get; set; }
		public int ServerRecvPort { get; set; }

		EyeTrackingController.EyeTrackingController ETDevice;
		
		private delegate void CalibrationCallback(EyeTrackingController.EyeTrackingController.CalibrationPointStruct calibrationPointData);	
        CalibrationCallback calibrationCallbackDelegate;

		private delegate void GetSampleCallback(EyeTrackingController.EyeTrackingController.SampleStruct sampleData);
        GetSampleCallback sampleCallbackDelegate;

        private Dictionary<int, string> errorDictionary = new Dictionary<int, string>();

		public SmiEyeTracker()
		{
			ServerSendPort = -1;
			ServerRecvPort = -1;
			IsCalibrationRequired = true;

            errorDictionary [1] = "RET_SUCCESS";

            errorDictionary [2] = "RET_NO_VALID_DATA";
            errorDictionary [3] = "RET_CALIBRATION_ABORTED";
            errorDictionary [4] = "RET_SERVER_IS_RUNNING";
            errorDictionary [5] = "RET_CALIBRATION_NOT_IN_PROGRESS";
            errorDictionary [11] = "RET_WINDOW_IS_OPEN";
            errorDictionary [12] = "RET_WINDOW_IS_CLOSED";
                
            errorDictionary [100] = "ERR_COULD_NOT_CONNECT";
            errorDictionary [101] = "ERR_NOT_CONNECTED";
            errorDictionary [102] = "ERR_NOT_CALIBRATED";
            errorDictionary [103] = "ERR_NOT_VALIDATED";
            errorDictionary [104] = "ERR_EYETRACKING_APPLICATION_NOT_RUNNING";
            errorDictionary [105] = "ERR_WRONG_COMMUNICATION_PARAMETER";
            errorDictionary [111] = "ERR_WRONG_DEVICE";
            errorDictionary [112] = "ERR_WRONG_PARAMETER";
            errorDictionary [113] = "ERR_WRONG_CALIBRATION_METHOD";
            errorDictionary [123] = "ERR_BIND_SOCKET";
            errorDictionary [124] = "ERR_DELETE_SOCKET";
            errorDictionary [131] = "ERR_NO_RESPONSE_FROM_IVIEWX";
            errorDictionary [133] = "ERR_WRONG_IVIEWX_VERSION";
            errorDictionary [171] = "ERR_ACCESS_TO_FILE";
            errorDictionary [191] = "ERR_EMPTY_DATA_BUFFER";
            errorDictionary [192] = "ERR_RECORDING_DATA_BUFFER";
            errorDictionary [193] = "ERR_FULL_DATA_BUFFER";
            errorDictionary [194] = "ERR_IVIEWX_IS_NOT_READY";
            errorDictionary [195] = "ERR_PAUSED_DATA_BUFFER";
            errorDictionary [201] = "ERR_IVIEWX_NOT_FOUND";
            errorDictionary [211] = "ERR_CAMERA_NOT_FOUND";
            errorDictionary [212] = "ERR_WRONG_CAMERA";
            errorDictionary [213] = "ERR_WRONG_CAMERA_PORT";
            errorDictionary [220] = "ERR_COULD_NOT_OPEN_PORT";
            errorDictionary [221] = "ERR_COULD_NOT_CLOSE_PORT";
            errorDictionary [222] = "ERR_AOI_ACCESS";
            errorDictionary [250] = "ERR_FEATURE_NOT_LICENSED";
            errorDictionary [400] = "ERR_INITIALIZATION";
		}

		private void ValidateSettings()
		{
			if (String.IsNullOrEmpty (ServerSendAddress)) 
			{
				throw new Exception("ServerSendAddress is null or empty");
			}

			if (ServerSendPort < 0) 
			{
				throw new Exception("ServerSendPort is not set");
			}

			if (String.IsNullOrEmpty (ServerRecvAddress)) 
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

			int ret = 0;
			try
			{
				ETDevice = new EyeTrackingController.EyeTrackingController();
				ETDevice.iV_SetLogger(0, new StringBuilder(this.GetType().Name + ".txt"));
				
                calibrationCallbackDelegate = new CalibrationCallback(CalibrationCallbackFunction);
                sampleCallbackDelegate = new GetSampleCallback(GetSampleCallbackFunction);

                ETDevice.iV_SetCalibrationCallback(calibrationCallbackDelegate);
                ETDevice.iV_SetSampleCallback(sampleCallbackDelegate);
								
				// connect to server 
				DebugLog.Info("SMI Eye Tracker: Connect");
				ret = ETDevice.iV_Connect(
					new StringBuilder(ServerSendAddress), ServerSendPort, 
					new StringBuilder(ServerRecvAddress), ServerRecvPort);
				if (ret == 1)
				{
					DebugLog.Info("EyeTracker connection established: {0}:{1}", ServerRecvAddress, ServerRecvPort);
					IsConnected = true;
				}
				else
				{
                    throw new Exception("Unable to connect. " + GetErrorMessage(ret));
				}
			}
			catch (Exception e)
			{
                throw new Exception(string.Format("Unable to connect SMI server ({0}:{1}). {2}", ServerRecvAddress, ServerRecvPort, e.Message));
			}
		}
		public override void Disconnect() 
		{
			int ret = 0;
			try
			{
				DebugLog.Info("SMI Eye Tracker: Disconnect");
				ret = ETDevice.iV_Disconnect();
				if (ret == 1)
				{					
					ETDevice = null;
					
					DebugLog.Info("Disconnnected from EyeTracker: {0}:{1}", ServerRecvAddress, ServerRecvPort);
					IsConnected = false;
				}
				else
				{
                    throw new Exception("Unable to setup disconnect. " + GetErrorMessage(ret));
				}
			}
			catch (Exception e)
			{
                throw new Exception(string.Format("Unable to disconnect from SMI server ({0}:{1}). {2}", ServerRecvAddress, ServerRecvPort, e.Message));			}
		}

		public override void Calibrate(CalibrationResultHandler callback)
		{
			IsCalibrated = false;		
			string message = "Not calibrated";
			Dictionary<string, object> calibrationProperties = new Dictionary<string, object>();
			
			try {
				int ret = 0;
				
				int targetSize = 20;				
				
				EyeTrackingController.EyeTrackingController.CalibrationStruct m_CalibrationData = new EyeTrackingController.EyeTrackingController.CalibrationStruct ();
                m_CalibrationData.displayDevice = (int)DisplayDevice.Primary;
				m_CalibrationData.autoAccept = 1;
                m_CalibrationData.method = (int)CalibrationPoints;
				m_CalibrationData.visualization = 1;
				m_CalibrationData.speed = 0;
				m_CalibrationData.targetShape = 2;
				m_CalibrationData.backgroundColor = 230;
				m_CalibrationData.foregroundColor = 250;
				m_CalibrationData.targetSize = targetSize;
				m_CalibrationData.targetFilename = "";

				DebugLog.Info("SMI Eye Tracker: Setup calibration");
				ret = ETDevice.iV_SetupCalibration (ref m_CalibrationData);
				if (ret != 1) 
                {
					throw new Exception("Calibration setup returned an error: " + GetErrorMessage(ret));
				}

				DebugLog.Info("SMI Eye Tracker: Start calibration");
				ret = ETDevice.iV_Calibrate();
				if (ret != 1) 
                {
                    throw new Exception("Calibration returned an error: " + GetErrorMessage(ret));
				}

				DebugLog.Info("SMI Eye Tracker: Start validation");
				ret = ETDevice.iV_Validate();
				if (ret != 1) 
				{
					throw new Exception("Validation returned an error: " + GetErrorMessage(ret));
				}

				DebugLog.Info("SMI Eye Tracker: Get accuracy data");
				EyeTrackingController.EyeTrackingController.AccuracyStruct accuracyData = new EyeTrackingController.EyeTrackingController.AccuracyStruct();
				ret = ETDevice.iV_GetAccuracy(ref accuracyData, 1);
				if (ret != 1) 
				{
					throw new Exception("GetAccuracy returned an error: " + GetErrorMessage(ret));
				}

				DebugLog.Info("SMI Eye Tracker: Calibration and validation complete");
				IsCalibrated = true;

				message = string.Format("Successfully calibrated SMI eye tracker.\nLeft Eye Deviation = ({0}, {1})\nRight Eye Deviation = ({2}, {3})", 
				                        accuracyData.deviationXLeft, 
				                        accuracyData.deviationYLeft, 
				                        accuracyData.deviationXRight, 
				                        accuracyData.deviationYRight);

				calibrationProperties["deviationXLeft"] = accuracyData.deviationXLeft;
				calibrationProperties["deviationYLeft"] = accuracyData.deviationYLeft;
				calibrationProperties["deviationXRight"] = accuracyData.deviationXRight;
				calibrationProperties["deviationYRight"] = accuracyData.deviationYRight;
			}
            catch (Exception e) 
            {
				message = string.Format("Unable to calibrate SMI server ({0}:{1}). {2}", ServerRecvAddress, ServerRecvPort, e.Message);
			}

			if (callback != null)
			{			
				callback(IsCalibrated, message, calibrationProperties);
			}
		}

		private void CalibrationCallbackFunction(EyeTrackingController.EyeTrackingController.CalibrationPointStruct calibrationPoint)
		{
			DebugLog.Info("SMI Eye Tracker: CalibrationCallback - Number = {0} Position = ({1}, {2})", calibrationPoint.number, calibrationPoint.positionX, calibrationPoint.positionY);
		}
		
		private void GetSampleCallbackFunction(EyeTrackingController.EyeTrackingController.SampleStruct sampleData)
		{
            // TODO rgtaylor 2015-04-22 Invert the y since SMI *appears* to define 0,0 at
            // the top left of the screen. Need to confirm.
			OnGazeChanged(new GazeEventArgs() 
            { 
				LeftX = (float)sampleData.leftEye.gazeX,
				LeftY = UnityEngine.Screen.height - (float)sampleData.leftEye.gazeY,
				LeftPupilDiameter = (float)sampleData.leftEye.diam,
				RightX = (float)sampleData.rightEye.gazeX,
                RightY = UnityEngine.Screen.height - (float)sampleData.rightEye.gazeY,
				RightPupilDiameter = (float)sampleData.rightEye.diam
			});			
		}

		private string GetErrorMessage(int returnCode)
		{
            if (errorDictionary.ContainsKey(returnCode))
            {
                return errorDictionary[returnCode];
            } 
            else
            {
                return "Return Code " + returnCode.ToString();
            }
		}
    }
}

