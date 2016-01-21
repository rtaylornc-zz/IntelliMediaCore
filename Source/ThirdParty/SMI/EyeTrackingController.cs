// -----------------------------------------------------------------------
//
// (c) Copyright 1997-2014, SensoMotoric Instruments GmbH
// 
// Permission  is  hereby granted,  free  of  charge,  to any  person  or
// organization  obtaining  a  copy  of  the  software  and  accompanying
// documentation  covered  by  this  license  (the  "Software")  to  use,
// reproduce,  display, distribute, execute,  and transmit  the Software,
// and  to  prepare derivative  works  of  the  Software, and  to  permit
// third-parties to whom the Software  is furnished to do so, all subject
// to the following:
// 
// The  copyright notices  in  the Software  and  this entire  statement,
// including the above license  grant, this restriction and the following
// disclaimer, must be  included in all copies of  the Software, in whole
// or  in part, and  all derivative  works of  the Software,  unless such
// copies   or   derivative   works   are   solely   in   the   form   of
// machine-executable  object   code  generated  by   a  source  language
// processor.
// 
// THE  SOFTWARE IS  PROVIDED  "AS  IS", WITHOUT  WARRANTY  OF ANY  KIND,
// EXPRESS OR  IMPLIED, INCLUDING  BUT NOT LIMITED  TO THE  WARRANTIES OF
// MERCHANTABILITY,   FITNESS  FOR  A   PARTICULAR  PURPOSE,   TITLE  AND
// NON-INFRINGEMENT. IN  NO EVENT SHALL  THE COPYRIGHT HOLDERS  OR ANYONE
// DISTRIBUTING  THE  SOFTWARE  BE   LIABLE  FOR  ANY  DAMAGES  OR  OTHER
// LIABILITY, WHETHER  IN CONTRACT, TORT OR OTHERWISE,  ARISING FROM, OUT
// OF OR IN CONNECTION WITH THE  SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace EyeTrackingController
{

    public class EyeTrackingController
    {

#if (!UNITY_64) 
        // use 32 bit library 
        const string dllName = "iViewXAPI.dll";
#elif (UNITY_64)
        // use 64 bit library 
        const string dllName = "iViewXAPI64.dll";
#endif




        // API Struct definition. See the manual for further description. 

        public enum ETDevice 
		{
			NONE = 0, 
			RED = 1, 
			REDm = 2, 
			HiSpeed = 3, 
			MRI = 4, 
			HED = 5, 
			Custom = 7
		};

        public enum ETApplication 
		{
			iViewX = 0, 
			iViewXOEM = 1
		};

        public enum FilterType 
		{
			Average = 0 
		};

        public enum FilterAction 
		{
			Query = 0, 
			Set = 1
		};

        public enum CalibrationStatusEnum 
		{
			calibrationUnknown = 0, 
			calibrationInvalid = 1, 
			calibrationValid = 2, 
			calibrationInProgress = 3 
		};

		public enum REDGeometryEnum
		{
			monitorIntegrated = 0,
			standalone = 1
		};

        public struct SystemInfoStruct
        {
            public int samplerate;
            public int iV_MajorVersion;
            public int iV_MinorVersion;
            public int iV_Buildnumber;
            public int API_MajorVersion;
            public int API_MinorVersion;
            public int API_Buildnumber;
            public ETDevice iV_ETSystem;
        };

        public struct CalibrationPointStruct
        {
            public int number;
            public int positionX;
            public int positionY;
        };


        public struct EyeDataStruct
        {
            public double gazeX;
            public double gazeY;
            public double diam;
            public double eyePositionX;
            public double eyePositionY;
            public double eyePositionZ;
        };


        public struct SampleStruct
        {
            public Int64 timestamp;
            public EyeDataStruct leftEye;
            public EyeDataStruct rightEye;
            public int planeNumber;
        };
        

        public struct EventStruct
        {
            public char eventType;
            public char eye;
            public Int64 startTime;
            public Int64 endTime;
            public Int64 duration;
            public double positionX;
            public double positionY;
        };

		public struct EyePositionStruct
		{
			public int validity; 
			public double relativePositionX; 
			public double relativePositionY; 
			public double relativePositionZ; 
			public double positionRatingX;
			public double positionRatingY;
			public double positionRatingZ;
		};

		public struct TrackingStatusStruct 
		{
			public Int64 timestamp; 
			public EyePositionStruct leftEye; 
			public EyePositionStruct rightEye; 
			public EyePositionStruct total; 
		};
		
        public struct AccuracyStruct
        {
            public double deviationXLeft;
            public double deviationYLeft;
            public double deviationXRight;
            public double deviationYRight;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct CalibrationStruct
        {
            public int method;				        
            public int visualization;			    
            public int displayDevice;				
            public int speed;					    
            public int autoAccept;			        
            public int foregroundColor;	            
            public int backgroundColor;	            
            public int targetShape;		            
            public int targetSize;		            
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string targetFilename;
        };
        
        public struct REDGeometryStruct
        {
			public REDGeometryEnum redGeometry;
			public int	monitorSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string setupName;
            public int stimX;
            public int stimY;
            public int stimHeightOverFloor;
            public int redHeightOverFloor;
            public int redStimDist;
            public int redInclAngle;
            public int redStimDistHeight;
            public int redStimDistDepth;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct ImageStruct
        {
            public int imageHeight;
            public int imageWidth;
            public int imageSize;
            public IntPtr imageBuffer;
        };

        public struct DateStruct
        {
            public int day;
            public int month;
            public int year;
        };

        public struct AOIRectangleStruct
        {
            public int x1;
            public int x2;
            public int y1;
            public int y2;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct AOIStruct
        {
            public int enabled;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string aoiName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string aoiGroup;
            public AOIRectangleStruct position;
            public int fixationHit;
            public int outputValue;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string outputMessage;
            public char eye;
        };


        // Kernel Function definition 

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);


        // API Function definition. See the manual for further description. 

        [DllImport(dllName, EntryPoint = "iV_AbortCalibration")]
        private static extern int Unmanaged_AbortCalibration();

        [DllImport(dllName, EntryPoint = "iV_AcceptCalibrationPoint")]
        private static extern int Unmanaged_AcceptCalibrationPoint();

        [DllImport(dllName, EntryPoint = "iV_Calibrate")]
        private static extern int Unmanaged_Calibrate();
        
        [DllImport(dllName, EntryPoint = "iV_ChangeCalibrationPoint")]
        private static extern int Unmanaged_ChangeCalibrationPoint(int number, int positionX, int positionY);

        [DllImport(dllName, EntryPoint = "iV_ClearAOI")]
        private static extern int Unmanaged_ClearAOI();

        [DllImport(dllName, EntryPoint = "iV_ClearRecordingBuffer")]
        private static extern int Unmanaged_ClearRecordingBuffer();

        [DllImport(dllName, EntryPoint = "iV_ConfigureFilter")]
		private static extern int Unmanaged_ConfigureFilter(FilterType filter, FilterAction action, IntPtr data);

        [DllImport(dllName, EntryPoint = "iV_Connect")]
        private static extern int Unmanaged_Connect(StringBuilder sendIPAddress, int sendPort, StringBuilder recvIPAddress, int receivePort);

        [DllImport(dllName, EntryPoint = "iV_ConnectLocal")]
        private static extern int Unmanaged_ConnectLocal();

        [DllImport(dllName, EntryPoint = "iV_ContinueEyetracking")]
        private static extern int Unmanaged_ContinueEyetracking();

        [DllImport(dllName, EntryPoint = "iV_ContinueRecording")]
        private static extern int Unmanaged_ContinueRecording(StringBuilder etMessage);

        [DllImport(dllName, EntryPoint = "iV_DefineAOI")]
        private static extern int Unmanaged_DefineAOI(ref AOIStruct aoiData);

        [DllImport(dllName, EntryPoint = "iV_DefineAOIPort")]
        private static extern int Unmanaged_DefineAOIPort(int port);
               
        [DllImport(dllName, EntryPoint = "iV_DeleteREDGeometry")]
        private static extern int Unmanaged_DeleteREDGeometry(StringBuilder name);

        [DllImport(dllName, EntryPoint = "iV_DisableAOI")]
        private static extern int Unmanaged_DisableAOI(StringBuilder name);

        [DllImport(dllName, EntryPoint = "iV_DisableAOIGroup")]
        private static extern int Unmanaged_DisableAOIGroup(StringBuilder group);

        [DllImport(dllName, EntryPoint = "iV_DisableGazeDataFilter")]
        private static extern int Unmanaged_DisableGazeDataFilter();

        [DllImport(dllName, EntryPoint = "iV_DisableProcessorHighPerformanceMode")]
        private static extern int Unmanaged_DisableProcessorHighPerformanceMode();

        [DllImport(dllName, EntryPoint = "iV_Disconnect")]
        private static extern int Unmanaged_Disconnect();

        [DllImport(dllName, EntryPoint = "iV_EnableAOI")]
        private static extern int Unmanaged_EnableAOI(StringBuilder name);

        [DllImport(dllName, EntryPoint = "iV_EnableAOIGroup")]
        private static extern int Unmanaged_EnableAOIGroup(StringBuilder group);

        [DllImport(dllName, EntryPoint = "iV_EnableGazeDataFilter")]
        private static extern int Unmanaged_EnableGazeDataFilter();

        [DllImport(dllName, EntryPoint = "iV_EnableProcessorHighPerformanceMode")]
        private static extern int Unmanaged_EnableProcessorHighPerformanceMode();

        [DllImport(dllName, EntryPoint = "iV_GetAccuracy")]
        private static extern int Unmanaged_GetAccuracy(ref AccuracyStruct accuracyData, int visualization);

        [DllImport(dllName, EntryPoint = "iV_GetAccuracyImage")]
        private static extern int Unmanaged_GetAccuracyImage(ref ImageStruct imageData);

        [DllImport(dllName, EntryPoint = "iV_GetAOIOutputValue")]
        private static extern int Unmanaged_GetAOIOutputValue(ref int aoiOutputValue);

        [DllImport(dllName, EntryPoint = "iV_GetCalibrationParameter")]
        private static extern int Unmanaged_GetCalibrationParameter(ref CalibrationStruct calibrationData);

        [DllImport(dllName, EntryPoint = "iV_GetCalibrationPoint")]
        private static extern int Unmanaged_GetCalibrationPoint(int calibrationPointNumber, ref CalibrationPointStruct calibrationPoint);

        [DllImport(dllName, EntryPoint = "iV_GetCalibrationStatus")]
        private static extern int Unmanaged_GetCalibrationStatus(ref CalibrationStatusEnum status);

        [DllImport(dllName, EntryPoint = "iV_GetCurrentCalibrationPoint")]
        private static extern int Unmanaged_GetCurrentCalibrationPoint(ref CalibrationPointStruct actualCalibrationPoint);

        [DllImport(dllName, EntryPoint = "iV_GetCurrentREDGeometry")]
        private static extern int Unmanaged_GetCurrentREDGeometry(ref REDGeometryStruct geometry);

        [DllImport(dllName, EntryPoint = "iV_GetCurrentTimestamp")]
        private static extern int Unmanaged_GetCurrentTimestamp(ref Int64 currentTimestamp);

		[DllImport(dllName, EntryPoint = "iV_GetDeviceName")]
		private static extern int Unmanaged_GetDeviceName([In] [Out] char[] snArray);

        [DllImport(dllName, EntryPoint = "iV_GetEvent")]
        private static extern int Unmanaged_GetEvent(ref EventStruct eventDataSample);

        [DllImport(dllName, EntryPoint = "iV_GetEyeImage")]
        private static extern int Unmanaged_GetEyeImage(ref ImageStruct imageData);

        [DllImport(dllName, EntryPoint = "iV_GetFeatureKey")]
        private static extern int Unmanaged_GetFeatureKey(ref Int64 featureKey);

        [DllImport(dllName, EntryPoint = "iV_GetGeometryProfiles")]
        private static extern int Unmanaged_GetGeometryProfiles(int maxSize, ref StringBuilder profileNames);

        [DllImport(dllName, EntryPoint = "iV_GetLicenseDueDate")]
        private static extern int Unmanaged_GetLicenseDueDate(ref DateStruct licenseDueDate);

        [DllImport(dllName, EntryPoint = "iV_GetREDGeometry")]
        private static extern int Unmanaged_GetREDGeometry(StringBuilder profileName, ref REDGeometryStruct geometry);

        [DllImport(dllName, EntryPoint = "iV_GetSample")]
        private static extern int Unmanaged_GetSample(ref SampleStruct rawDataSample);

        [DllImport(dllName, EntryPoint = "iV_GetSceneVideo")]
        private static extern int Unmanaged_GetSceneVideo(ref ImageStruct imageData);

        [DllImport(dllName, EntryPoint = "iV_GetSerialNumber")]
        private static extern int Unmanaged_GetSerialNumber([In] [Out] char[] snArray);

        [DllImport(dllName, EntryPoint = "iV_GetSystemInfo")]
        private static extern int Unmanaged_GetSystemInfo(ref SystemInfoStruct systemInfoData);

        [DllImport(dllName, EntryPoint = "iV_GetTrackingMonitor")]
        private static extern int Unmanaged_GetTrackingMonitor(ref ImageStruct imageData);

        [DllImport(dllName, EntryPoint = "iV_GetTrackingStatus")]
        private static extern int Unmanaged_GetTrackingStatus(ref TrackingStatusStruct trackingStatus);
        
		[DllImport(dllName, EntryPoint = "iV_HideAccuracyMonitor")]
        private static extern int Unmanaged_HideAccuracyMonitor();

        [DllImport(dllName, EntryPoint = "iV_HideEyeImageMonitor")]
        private static extern int Unmanaged_HideEyeImageMonitor();

        [DllImport(dllName, EntryPoint = "iV_HideSceneVideoMonitor")]
        private static extern int Unmanaged_HideSceneVideoMonitor();

        [DllImport(dllName, EntryPoint = "iV_HideTrackingMonitor")]
        private static extern int Unmanaged_HideTrackingMonitor();

        [DllImport(dllName, EntryPoint = "iV_IsConnected")]
        private static extern int Unmanaged_IsConnected();

        [DllImport(dllName, EntryPoint = "iV_LoadCalibration")]
        private static extern int Unmanaged_LoadCalibration(StringBuilder name);

        [DllImport(dllName, EntryPoint = "iV_Log")]
        private static extern int Unmanaged_Log(StringBuilder logMessage);

        [DllImport(dllName, EntryPoint = "iV_PauseEyetracking")]
        private static extern int Unmanaged_PauseEyetracking();

        [DllImport(dllName, EntryPoint = "iV_PauseRecording")]
        private static extern int Unmanaged_PauseRecording();

        [DllImport(dllName, EntryPoint = "iV_Quit")]
        private static extern int Unmanaged_Quit();

        [DllImport(dllName, EntryPoint = "iV_ReleaseAOIPort")]
        private static extern int Unmanaged_ReleaseAOIPort();

        [DllImport(dllName, EntryPoint = "iV_RemoveAOI")]
        private static extern int Unmanaged_RemoveAOI(StringBuilder  aoiName);

        [DllImport(dllName, EntryPoint = "iV_ResetCalibrationPoints")]
        private static extern int Unmanaged_ResetCalibrationPoints();

        [DllImport(dllName, EntryPoint = "iV_SaveCalibration")]
        private static extern int Unmanaged_SaveCalibration(StringBuilder  aoiName);

        [DllImport(dllName, EntryPoint = "iV_SaveData")]
        private static extern int Unmanaged_SaveData(StringBuilder  filename, StringBuilder description, StringBuilder user, int overwrite);

        [DllImport(dllName, EntryPoint = "iV_SendCommand")]
        private static extern int Unmanaged_SendCommand(StringBuilder etMessage);

        [DllImport(dllName, EntryPoint = "iV_SendImageMessage")]
        private static extern int Unmanaged_SendImageMessage(StringBuilder etMessage);

        [DllImport(dllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "iV_SetAOIHitCallback")]
        private static extern void Unmanaged_SetAOIHitCallback(MulticastDelegate aoiHitCallbackFunction);

        [DllImport(dllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "iV_SetCalibrationCallback")]
        private static extern void Unmanaged_SetCalibrationCallback(MulticastDelegate calibrationCallbackFunction);

        [DllImport(dllName, EntryPoint = "iV_SetConnectionTimeout")]
        private static extern int Unmanaged_SetConnectionTimeout(int time);

        [DllImport(dllName, EntryPoint = "iV_SelectREDGeometry")]
        private static extern int Unmanaged_SelectREDGeometry(StringBuilder profileName);

        [DllImport(dllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "iV_SetEventCallback")]
        private static extern void Unmanaged_SetEventCallback(MulticastDelegate eventCallbackFunction);

        [DllImport(dllName, EntryPoint = "iV_SetEventDetectionParameter")]
        private static extern int Unmanaged_SetEventDetectionParameter(int minDuration, int maxDispersion);

        [DllImport(dllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "iV_SetEyeImageCallback")]
        private static extern void Unmanaged_SetEyeImageCallback(MulticastDelegate eyeImageCallbackFunction);

        [DllImport(dllName, EntryPoint = "iV_SetLicense")]
        private static extern int Unmanaged_SetLicense(StringBuilder licenseKey);

        [DllImport(dllName, EntryPoint = "iV_SetLogger")]
        private static extern int Unmanaged_SetLogger(int logLevel, StringBuilder filename);

        [DllImport(dllName, EntryPoint = "iV_SetResolution")]
        private static extern int Unmanaged_SetResolution(int stimulusWidth, int stimulusHeight);

        [DllImport(dllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "iV_SetSampleCallback")]
        private static extern void Unmanaged_SetSampleCallback(MulticastDelegate sampleCallbackFunction);

        [DllImport(dllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "iV_SetSceneVideoCallback")]
        private static extern void Unmanaged_SetSceneVideoCallback(MulticastDelegate sceneVideoCallbackFunction);

        [DllImport(dllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "iV_SetTrackingMonitorCallback")]
        private static extern void Unmanaged_SetTrackingMonitorCallback(MulticastDelegate trackingMonitorCallbackFunction);

        [DllImport(dllName, EntryPoint = "iV_SetTrackingParameter")]
        private static extern int Unmanaged_SetTrackingParameter(int ET_PARAM_EYE, int ET_PARAM, int value);

        [DllImport(dllName, EntryPoint = "iV_SetupCalibration")]
        private static extern int Unmanaged_SetupCalibration(ref CalibrationStruct calibrationData);

        [DllImport(dllName, EntryPoint = "iV_SetREDGeometry")]
        private static extern int Unmanaged_SetREDGeometry(ref REDGeometryStruct redGeometry);

        [DllImport(dllName, EntryPoint = "iV_ShowAccuracyMonitor")]
        private static extern int Unmanaged_ShowAccuracyMonitor();

        [DllImport(dllName, EntryPoint = "iV_ShowEyeImageMonitor")]
        private static extern int Unmanaged_ShowEyeImageMonitor();

        [DllImport(dllName, EntryPoint = "iV_ShowSceneVideoMonitor")]
        private static extern int Unmanaged_ShowSceneVideoMonitor();

        [DllImport(dllName, EntryPoint = "iV_ShowTrackingMonitor")]
        private static extern int Unmanaged_ShowTrackingMonitor();

        [DllImport(dllName, EntryPoint = "iV_Start")]
        private static extern int Unmanaged_Start(ETApplication etApplication);

        [DllImport(dllName, EntryPoint = "iV_StartRecording")]
        private static extern int Unmanaged_StartRecording();

        [DllImport(dllName, EntryPoint = "iV_StopRecording")]
        private static extern int Unmanaged_StopRecording();

        [DllImport(dllName, EntryPoint = "iV_TestTTL")]
        private static extern int Unmanaged_TestTTL(long value);

        [DllImport(dllName, EntryPoint = "iV_Validate")]
        private static extern int Unmanaged_Validate();






        public int iV_AbortCalibration()
        {
            return Unmanaged_AbortCalibration();
        }

        public int iV_AcceptCalibrationPoint()
        {
            return Unmanaged_AcceptCalibrationPoint();
        }

        public int iV_Calibrate()
        {
            return Unmanaged_Calibrate();
        }

        public int iV_ChangeCalibrationPoint(int number, int positionX, int positionY)
        {
            return Unmanaged_ChangeCalibrationPoint(number, positionX, positionY);
        }

        public int iV_ClearAOI()
        {
            return Unmanaged_ClearAOI();
        }

        public int iV_ClearRecordingBuffer()
        {
            return Unmanaged_ClearRecordingBuffer();
        }

        public int iV_ConfigureFilter(FilterType filter, FilterAction action, IntPtr data)
        {
            return Unmanaged_ConfigureFilter(filter, action, data);
        }

        public int iV_Connect(StringBuilder sendIP, int sendPort, StringBuilder receiveIP, int receivePort)
        {
            return Unmanaged_Connect(sendIP, sendPort, receiveIP, receivePort);
        }

        public int iV_ConnectLocal()
        {
            return Unmanaged_ConnectLocal();
        }

        public int iV_ContinueEyetracking()
        {
            return Unmanaged_ContinueEyetracking();
        }

        public int iV_ContinueRecording(StringBuilder trialname)
        {
            return Unmanaged_ContinueRecording(trialname);
        }

        public int iV_DefineAOI(ref AOIStruct aoi)
        {
            return Unmanaged_DefineAOI(ref aoi);
        }

        public int iV_DefineAOIPort(int port)
        {
            return Unmanaged_DefineAOIPort(port);
        }

        public int iV_DeleteREDGeometry(StringBuilder name)
        {
            return Unmanaged_DeleteREDGeometry(name);
        }

        public int iV_DisableAOI(StringBuilder aoiName)
        {
            return Unmanaged_DisableAOI(aoiName);
        }

        public int iV_DisableAOIGroup(StringBuilder aoiGroup)
        {
            return Unmanaged_DisableAOIGroup(aoiGroup);
        }

        public int iV_DisableGazeDataFilter()
        {
            return Unmanaged_DisableGazeDataFilter();
        }

        public int iV_DisableProcessorHighPerformanceMode()
        {
            return Unmanaged_DisableProcessorHighPerformanceMode();
        }

        public int iV_Disconnect()
        {
            return Unmanaged_Disconnect();
        }

        public int iV_EnableAOI(StringBuilder aoiName)
        {
            return Unmanaged_EnableAOI(aoiName);
        }

        public int iV_EnableAOIGroup(StringBuilder aoiGroup)
        {
            return Unmanaged_EnableAOIGroup(aoiGroup);
        }

        public int iV_EnableGazeDataFilter()
        {
            return Unmanaged_EnableGazeDataFilter();
        }

        public int iV_EnableProcessorHighPerformanceMode()
        {
            return Unmanaged_EnableProcessorHighPerformanceMode();
        }

        public int iV_GetAccuracy(ref AccuracyStruct accuracyData, int visualization)
        {
            return Unmanaged_GetAccuracy(ref accuracyData, visualization);
        }

        public int iV_GetAccuracyImage(ref ImageStruct image)
        {
            return Unmanaged_GetAccuracyImage(ref image);
        }

        public int iV_GetAOIOutputValue(ref int aoiOutputValue)
        {
            return Unmanaged_GetAOIOutputValue(ref aoiOutputValue);
        }

        public int iV_GetCalibrationParameter(ref CalibrationStruct calibrationData)
        {
            return Unmanaged_GetCalibrationParameter(ref calibrationData);
        }

        public int iV_GetCalibrationPoint(int calibrationPointNumber, ref CalibrationPointStruct calibrationPoint)
        {
            return Unmanaged_GetCalibrationPoint(calibrationPointNumber, ref calibrationPoint);
        }

        public int iV_GetCalibrationStatus(ref CalibrationStatusEnum calibrationStatus)
        {
            return Unmanaged_GetCalibrationStatus(ref calibrationStatus);
        }

        public int iV_GetCurrentCalibrationPoint(ref CalibrationPointStruct currentCalibrationPoint)
        {
            return Unmanaged_GetCurrentCalibrationPoint(ref currentCalibrationPoint);
        }

        public int iV_GetCurrentREDGeometry(ref REDGeometryStruct geometry)
        {
            return Unmanaged_GetCurrentREDGeometry(ref geometry);
        }
		
        public int iV_GetCurrentTimestamp(ref Int64 currentTimestamp)
        {
            return Unmanaged_GetCurrentTimestamp(ref currentTimestamp);
        }

		public int iV_GetDeviceName(out char[] deviceName)
		{
			deviceName = new char[64];
			return (int)Unmanaged_GetDeviceName(deviceName);
		}
	  
        public int iV_GetEvent(ref EventStruct eventDataSample)
        {
            return Unmanaged_GetEvent(ref eventDataSample);
        }

        public int iV_GetEyeImage(ref ImageStruct image)
        {
            return Unmanaged_GetEyeImage(ref image);
        }

        public int iV_GetFeatureKey(ref Int64 featureKey)
        {
            return Unmanaged_GetFeatureKey(ref featureKey);
        }

        public int iV_GetGeometryProfiles(int maxSize, ref StringBuilder profileNames)
        {
            return Unmanaged_GetGeometryProfiles(maxSize, ref profileNames);
        }

        public int iV_GetLicenseDueDate(ref DateStruct licenseDueDate)
        {
            return Unmanaged_GetLicenseDueDate(ref licenseDueDate);
        }

		public int iV_GetREDGeometry(StringBuilder profileName, ref REDGeometryStruct geometry)
        {
            return Unmanaged_GetREDGeometry(profileName, ref geometry);
        }

        public int iV_GetSample(ref SampleStruct rawDataSample)
        {
            return Unmanaged_GetSample(ref rawDataSample);
        }

        public int iV_GetSceneVideo(ref ImageStruct image)
        {
            return Unmanaged_GetSceneVideo(ref image);
        }

		public int iV_GetSerialNumber(out char[] serialNumber)
		{
			serialNumber = new char[64];
			return (int)Unmanaged_GetSerialNumber(serialNumber);
        }

        public int iV_GetSystemInfo(ref SystemInfoStruct systemInfo)
        {
            return Unmanaged_GetSystemInfo(ref systemInfo);
        }

        public int iV_GetTrackingMonitor(ref ImageStruct image)
        {
            return Unmanaged_GetTrackingMonitor(ref image);
        }

        public int iV_GetTrackingStatus(ref TrackingStatusStruct trackingstatus)
        {
            return Unmanaged_GetTrackingStatus(ref trackingstatus);
        }

        public int iV_HideAccuracyMonitor()
        {
            return Unmanaged_HideAccuracyMonitor();
        }

        public int iV_HideEyeImageMonitor()
        {
            return Unmanaged_HideEyeImageMonitor();
        }

        public int iV_HideSceneVideoMonitor()
        {
            return Unmanaged_HideSceneVideoMonitor();
        }

        public int iV_HideTrackingMonitor()
        {
            return Unmanaged_HideTrackingMonitor();
        }

        public int iV_IsConnected()
        {
            return Unmanaged_IsConnected();
        }

        public int iV_LoadCalibration(StringBuilder name)
        {
            return Unmanaged_LoadCalibration(name);
        }

        public int iV_Log(StringBuilder message)
        {
            return Unmanaged_Log(message);
        }

        public int iV_PauseEyetracking()
        {
            return Unmanaged_PauseEyetracking();
        }

        public int iV_PauseRecording()
        {
            return Unmanaged_PauseRecording();
        }

        public int iV_Quit()
        {
            return Unmanaged_Quit();
        }

        public int iV_ReleaseAOIPort()
        {
            return Unmanaged_ReleaseAOIPort();
        }
        public int iV_RemoveAOI(StringBuilder aoiName)
        {
            return Unmanaged_RemoveAOI(aoiName);
        }
                       
        public int iV_ResetCalibrationPoints()
        {
            return Unmanaged_ResetCalibrationPoints();
        }

        public int iV_SaveCalibration(StringBuilder name)
        {
            return Unmanaged_SaveCalibration(name);
        }

        public int iV_SaveData(StringBuilder filename, StringBuilder description, StringBuilder user, int overwrite)
        {
            return Unmanaged_SaveData(filename, description, user, overwrite);
        }

        public int iV_SendCommand(StringBuilder etMessage)
        {
            return Unmanaged_SendCommand(etMessage);
        }

        public int iV_SendImageMessage(StringBuilder message)
        {
            return Unmanaged_SendImageMessage(message);
        }

        public void iV_SetCalibrationCallback(MulticastDelegate calibrationCallback)
        {
            Unmanaged_SetCalibrationCallback(calibrationCallback);
        }

        public void iV_SetConnectionTimeout(int time)
        {
            Unmanaged_SetConnectionTimeout(time);
        }

        public int iV_SelectREDGeometry(StringBuilder profileName)
        {
            return Unmanaged_SelectREDGeometry(profileName);
        }

        public void iV_SetResolution(int stimulusWidth, int stimulusHeight)
        {
            Unmanaged_SetResolution(stimulusWidth, stimulusHeight);
        }

        public void iV_SetEventCallback(MulticastDelegate eventCallback)
        {
            Unmanaged_SetEventCallback(eventCallback);
        }

        public int iV_SetEventDetectionParameter(int minDuration, int maxDispersion)
        {
            return Unmanaged_SetEventDetectionParameter(minDuration, maxDispersion);
        }

        public void iV_SetEyeImageCallback(MulticastDelegate eyeImageCallback)
        {
            Unmanaged_SetEyeImageCallback(eyeImageCallback);
        }

        public int iV_SetLicense(StringBuilder key)
        {
            return Unmanaged_SetLicense(key);
        }

        public int iV_SetLogger(int logLevel, StringBuilder filename)
        {
            return Unmanaged_SetLogger(logLevel, filename);
        }

        public void iV_SetSampleCallback(MulticastDelegate sampleCallback)
        {
            Unmanaged_SetSampleCallback(sampleCallback);
        }

        public void iV_SetSceneVideoCallback(MulticastDelegate sceneVideoCallback)
        {
            Unmanaged_SetSceneVideoCallback(sceneVideoCallback);
        }

        public void iV_SetTrackingMonitorCallback(MulticastDelegate trackingMonitorCallback)
        {
            Unmanaged_SetTrackingMonitorCallback(trackingMonitorCallback);
        }

        public void iV_SetTrackingParameter(int ET_PARAM_EYE, int ET_PARAM, int value)
        {
            Unmanaged_SetTrackingParameter(ET_PARAM_EYE, ET_PARAM, value);
        }

        public int iV_SetupCalibration(ref CalibrationStruct calibrationData)
        {
            return Unmanaged_SetupCalibration(ref calibrationData);
        }

        public int iV_SetREDGeometry(ref REDGeometryStruct redGeometry)
        {
            return Unmanaged_SetREDGeometry(ref redGeometry);
        }

        public int iV_ShowAccuracyMonitor()
        {
            return Unmanaged_ShowAccuracyMonitor();
        }

        public int iV_ShowEyeImageMonitor()
        {
            return Unmanaged_ShowEyeImageMonitor();
        }

        public int iV_ShowSceneVideoMonitor()
        {
            return Unmanaged_ShowSceneVideoMonitor();
        }

        public int iV_ShowTrackingMonitor()
        {
            return Unmanaged_ShowTrackingMonitor();
        }

        public int iV_Start(int etApplication)
        {
            return Unmanaged_Start((ETApplication)etApplication);
        }

        public int iV_StartRecording()
        {
            return Unmanaged_StartRecording();
        }

        public int iV_StopRecording()
        {
            return Unmanaged_StopRecording();
        }

        public int iV_TestTTL(int value)
        {
            return Unmanaged_TestTTL(value);
        }

        public int iV_Validate()
        {
            return Unmanaged_Validate();
        }

    }
}
