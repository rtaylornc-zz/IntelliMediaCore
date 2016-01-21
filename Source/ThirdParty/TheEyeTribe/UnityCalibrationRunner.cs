/*
 * Created by Robert Taylor on 2015-05-25 to be an instantiable GameObject for calibration only.
 * This class is based on the CalibCamera class supplied by The Eye Tribe in Unity 3D sample code.
 *
 * Copyright 2015
 * This source code is licensed under the BSD-style license found in the LICENSE file in the root directory of this source tree.  
 * 
 */

using UnityEngine;
using System.Collections;
using TETCSharpClient;
using TETCSharpClient.Data;
using TETControls.Calibration;
using System;
using System.Collections.Generic;

/// <summary>
/// Component attached to 'Main Camera' of '/Scenes/calib_scene.unity'.
/// This script handles the main menu GUI and the process of calibrating the 
/// EyeTribe Server.
/// </summary>
public class UnityCalibrationRunner : MonoBehaviour, ICalibrationProcessHandler, ITrackerStateListener
{
	private const int MaxCalibrationAttempts = 3;
	private const int MaxResamplePoints = 4;
	private const float PointPadding = 0.1f;
	private const float EyeSettleDuration = 0.250f;
	private const string CalibrationRunnerPrefabName = "EyeTribeCalibrationRunner";
	private static UnityCalibrationRunner instance;

	// Unity-Editor Accessible Member Variables
	public Texture2D calibrationPointTexture;
	public Vector2 calibrationPointSize = new Vector2(40, 40);
	public int depth;
	public GUIStyle backgroundStyle;
	
	public event EventHandler<CalibrationRunnerEventArgs> OnResult;
	private bool resultEventRaised;
	
	public int PointCount { get; private set; }
	public float CalibrationSampleDuration { get; private set; }

	private List<Point2D> calibrationPoints = new List<Point2D>();
	
	private delegate void Callback();
	private Queue<Callback> callbackQueue = new Queue<Callback>();

	private int resampleCount;

	public bool CalibrationPointVisible { get; set; }
	Rect calibrationPointRect;
	private Point2D CurrentCalibrationPoint
	{
		get
		{
			return new Point2D(calibrationPointRect.center.x, calibrationPointRect.center.y);
		}
	}

	public static UnityCalibrationRunner StartCalibration(float calibrationSampleDuration, int calibrationPoints, EventHandler<CalibrationRunnerEventArgs> resultHandler)
	{
		try
		{
			if (instance != null)
			{
				throw new Exception("Calibration already in progress. Abort to retry.");
			}

			CheckTrackerState(GazeManager.Instance.Trackerstate);

			GameObject gameObject = null;

			UnityEngine.Object prefab = Resources.Load(CalibrationRunnerPrefabName);
			if (prefab)
			{
				gameObject = Instantiate(prefab) as GameObject; 
			}
			else
			{
				throw new Exception(string.Format("Unable to load {0} prefab from Resources", CalibrationRunnerPrefabName));

			}

			instance = (gameObject != null ? gameObject.GetComponent<UnityCalibrationRunner>() : null);
			if (instance != null)
			{
				instance.OnResult += resultHandler;
				instance.CalibrationSampleDuration = calibrationSampleDuration;
				instance.PointCount = calibrationPoints;
			}
			else
			{
				throw new Exception(string.Format("Unable to find {0} component on {1} prefab", typeof(UnityCalibrationRunner).Name, CalibrationRunnerPrefabName));
			}
		}
		catch(Exception e)
		{
			resultHandler(null, new CalibrationRunnerEventArgs(
				CalibrationRunnerResult.Error,
				e.Message));
		}

		return instance;
	}

	public static bool IsCalibrating { get { return instance != null; }}

	public static void Abort()
	{
		if (IsCalibrating)
		{
			instance.RaiseResult(CalibrationRunnerResult.Abort, "Calibration attempt aborted.");
		}
	}

	private void Destroy()
	{
		if (instance != null)
		{
			Destroy(instance.gameObject);
		}
	}

	public void OnDestroy()
	{
		GazeManager.Instance.RemoveTrackerStateListener(this);
		GazeManager.Instance.CalibrationAbort();
		instance = null;
	}

	private void RaiseResult(CalibrationRunnerResult result, string message, string rating = null, CalibrationResult calibrationReport = null)
	{
		if (resultEventRaised)
		{
			Debug.LogError("Callback called more than once.");
		}
		resultEventRaised = true;

		if (OnResult != null)
		{
			if (calibrationReport == null)
			{
				calibrationReport = new CalibrationResult()
				{
					Result = false
				};
			}
			OnResult(this, new CalibrationRunnerEventArgs(result, message, rating, calibrationReport));
		}

		// After calling back, we are done, get rid of the singleton
		Destroy();
	}

	private void DispatchResult(CalibrationRunnerResult result, string message, string rating = null, CalibrationResult calibrationReport = null)
	{
		QueueCallback(new Callback(() => RaiseResult(result, message, rating, calibrationReport)));
	}

	#region ITrackerStateListener implementation

	public void OnTrackerStateChanged(GazeManager.TrackerState trackerState)
	{
		try
		{
			CheckTrackerState(trackerState);
		}
		catch (Exception e)
		{
			DispatchResult(CalibrationRunnerResult.Error, e.Message);
		}
	}

	public void OnScreenStatesChanged (int screenIndex, int screenResolutionWidth, int screenResolutionHeight, float screenPhysicalWidth, float screenPhysicalHeight)
	{
	}

	#endregion
	
	void Start()
	{
		calibrationPointRect = new Rect(0, 0, calibrationPointSize.x, calibrationPointSize.y);			

		// TODO rgtaylor 2015-05-26 Temporarily disabled since this calls back 
		// resulting in the OnResult() callback being called twice
		//GazeManager.Instance.AddTrackerStateListener(this);

		// Show the calibration point in the center of the screen while we're waiting for the user
		// to press the space bar to start the calibration.
		UpdateCalibrationPointPosition(new Point2D(Screen.width/2, Screen.height/2));
	}

	private static void CheckTrackerState(GazeManager.TrackerState trackerState)
	{	
		string errorMessage = "";
		
		switch (GazeManager.Instance.Trackerstate)
		{
		case GazeManager.TrackerState.TRACKER_CONNECTED:
			//trackeStateOK = true;
			break;
		case GazeManager.TrackerState.TRACKER_CONNECTED_NOUSB3:
			errorMessage = "Device connected to a USB2.0 port";
			break;
		case GazeManager.TrackerState.TRACKER_CONNECTED_BADFW:
			// TODO rgtaylor 2015-05-26 Temporarily disabled since the eye tracker
			// is complaining about an older firmware.
			//errorMessage = "A firmware updated is required.";
			break;
		case GazeManager.TrackerState.TRACKER_NOT_CONNECTED:
			errorMessage = "Device not connected.";
			break;
		case GazeManager.TrackerState.TRACKER_CONNECTED_NOSTREAM:
			errorMessage = "No data coming out of the sensor.";
			break;
		}

		if (!string.IsNullOrEmpty(errorMessage))
		{
			throw new Exception(errorMessage);
		}
	}
	
	void Update()
	{	
		lock (callbackQueue)
		{
			//we handle queued callback in the update loop
			while (callbackQueue.Count > 0)
			{
				callbackQueue.Dequeue()();
			}
		}
		
		if (Input.GetKey(KeyCode.Space))
		{
			CreatePointList();
			GazeManager.Instance.CalibrationStart((short)calibrationPoints.Count, this);
		}

		if (Input.GetKey(KeyCode.Escape))
		{
			Abort();
		}
	}
	
	void OnGUI()
	{
		GUI.depth = depth;
		
		GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "", backgroundStyle);

		if (CalibrationPointVisible)
		{
			Color originalColor = GUI.color;
			if (!GazeManager.Instance.IsCalibrating)
			{
				GUI.color = Color.gray;
			}
			GUI.DrawTexture(calibrationPointRect, calibrationPointTexture);
			GUI.color = originalColor;
		}
	}
	
	public void OnCalibrationStarted()
	{
		//Handle on main UI thread
		QueueCallback(new Callback(delegate
		{
			Invoke("MoveToNextCalibrationPoint", EyeSettleDuration);
		}));
	}
	
	public void OnCalibrationProgress(double progress)
	{
		//Called every time a new calibration point have been sampled
	}
	
	public void OnCalibrationProcessing()
	{
		//Called when the calculation of the calibration results begins
	}
	
	public void OnCalibrationResult(CalibrationResult calibResult)
	{
		//Should we resample?
		if (!calibResult.Result)
		{
			//Evaluate results
			foreach (CalibrationPoint calPoint in calibResult.Calibpoints)
			{
				if (calPoint.State == CalibrationPoint.STATE_RESAMPLE || calPoint.State == CalibrationPoint.STATE_NO_DATA)
				{
					calibrationPoints.Add(new Point2D(calPoint.Coordinates.X, calPoint.Coordinates.Y));
				}
			}

			if (resampleCount++ >= MaxCalibrationAttempts)
			{
				calibrationPoints.Clear();
				DispatchResult(CalibrationRunnerResult.Failure, "Exceeded max calibration attempts: " + MaxCalibrationAttempts);
				return;
			}

			if (calibrationPoints.Count >= MaxResamplePoints)
			{
				calibrationPoints.Clear();
				DispatchResult(CalibrationRunnerResult.Failure, "Exceeded max resample of calibration points: " + MaxResamplePoints);
				return;
			}
			
			//Handle on main UI thread
			QueueCallback(new Callback(delegate
			{
				Invoke("MoveToNextCalibrationPoint",  EyeSettleDuration);
			}));
		}
		else
		{
			int ratingValue;
			string ratingText;
			CalibrationRatingFunction(calibResult, out ratingValue, out ratingText);
			DispatchResult(CalibrationRunnerResult.Success, 
			               string.Format("Successfully calibrated TheEyeTribe eye tracker.\nCalibration rating: {0}\nAverageErrorDegree = {1}\nAverageErrorDegreeLeft = {2}\nAverageErrorDegreeRight = {3}\n", 
			              		ratingText, 
			              		calibResult.AverageErrorDegree,
			              		calibResult.AverageErrorDegreeLeft,
			              		calibResult.AverageErrorDegreeRight),
			               ratingText,
			               calibResult);
		}
	}

	private void MoveToNextCalibrationPoint()
	{
		if (calibrationPoints.Count > 0)
		{
			UpdateCalibrationPointPosition(calibrationPoints[0]);
			calibrationPoints.RemoveAt(0);
			
			// Short delay to allow the eye to settle before sampling
			Invoke("StartCalibrationPointSampling", EyeSettleDuration);
		}
	}
	
	private void StartCalibrationPointSampling()
	{
		GazeManager.Instance.CalibrationPointStart((int)Math.Round(CurrentCalibrationPoint.X), (int)Math.Round(CurrentCalibrationPoint.Y));

		Invoke("EndCalibrationPointSampling", CalibrationSampleDuration);
	}
	
	private void EndCalibrationPointSampling()
	{
		GazeManager.Instance.CalibrationPointEnd();
		
		UpdateCalibrationPointPosition(null);
		
		Invoke("MoveToNextCalibrationPoint", EyeSettleDuration);
	}

	private void UpdateCalibrationPointPosition(Point2D point)
	{
		CalibrationPointVisible = point != null;
		if (CalibrationPointVisible)
		{
			calibrationPointRect.center = Point2dToVector2(point);
		}
	}
			
	private void CreatePointList()
	{
		calibrationPoints.Clear();

		Vector2 size = new Vector2(Screen.width, Screen.height);
		float scaleW = 1;
		float scaleH = 1;
		float offsetX = 0;
		float offsetY = 0;
		
		// add some padding 
		float paddingHeight = PointPadding;
		float paddingWidth = (size.y * PointPadding) / size.x; // use the same distance for the width padding
		
		float columns = (float)Math.Sqrt(PointCount);
		float rows = columns;
		
		if (PointCount == 12)
		{
			columns = (float)Math.Round(columns + 1, 0);
			rows = (float)Math.Round(rows, 0);
		}
		
		ArrayList points = new ArrayList();
		for (int dirX = 0; dirX < columns; dirX++)
		{
			for (int dirY = 0; dirY < rows; dirY++)
			{
				double x = Mathf.Lerp(paddingWidth, 1 - paddingWidth, dirX / (columns - 1));
				double y = Mathf.Lerp(paddingHeight, 1 - paddingHeight, dirY / (rows - 1));
				points.Add(new Point2D(offsetX + x * scaleW, offsetY + y * scaleH));
			}
		}
		
		// Shuffle point order
		int[] order = new int[PointCount];
		
		for (var c = 0; c < PointCount; c++)
			order[c] = c;
		
		Shuffle(order);
		
		foreach (int number in order)
			calibrationPoints.Add((Point2D)points[number]);
		
		// De-normalize points to fit the current screen
		foreach (var point in calibrationPoints)
		{
			point.X *= Screen.width;
			point.Y *= Screen.height;
		}
	}

	private void Shuffle<T>(IList<T> list)
	{
		System.Random rng = new System.Random();
		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = rng.Next(n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}

	/// <summary>
	/// Simple rating of a given calibration.
	/// </summary>
	/// <param name="result">Any given CalibrationResult</param>
	/// <param name="rating">A number between 1 - 5 where 5 is the best othervise -1.</param>
	/// <param name="strRating">A string with a rating name othervise ERROR.</param>
	public void CalibrationRatingFunction(CalibrationResult result, out int rating, out string strRating)
	{
		if (result == null)
		{
			rating = -1;
			strRating = "ERROR";
			return;
		}
		if (result.AverageErrorDegree < 0.5)
		{
			rating = 5;
			strRating = "PERFECT";
			return;
		}
		if (result.AverageErrorDegree < 0.7)
		{
			rating = 4;
			strRating = "GOOD";
			return;
		}
		if (result.AverageErrorDegree < 1)
		{
			rating = 3;
			strRating = "MODERATE";
			return;
		}
		if (result.AverageErrorDegree < 1.5)
		{
			rating = 2;
			strRating = "POOR";
			return;
		}
		rating = 1;
		strRating = "REDO";
	}
	
	/// <summary>
	/// Utility method for adding callback tasks to a queue
	/// that will eventually be handle in the Unity game loop 
	/// method 'Update()'.
	/// </summary>
	private void QueueCallback(Callback newTask)
	{
		lock (callbackQueue)
		{
			callbackQueue.Enqueue(newTask);
		}
	}

	private static Vector2 Point2dToVector2(Point2D point)
	{
		return new Vector2((float)point.X, (float)point.Y);
	}
}
