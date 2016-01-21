//---------------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using System.Runtime.Serialization;

namespace IntelliMedia
{   
    /// <summary>
    /// A timer that is executed on the main Unity thread.
    /// </summary>
	public class DispatcherTimer
	{
		// The following singleton is used to hook into Unity's coroutine processing that
		// is required by the WWW class.
		private class DispatcherTimerSingleton : MonoBehaviourSingleton<DispatcherTimerSingleton>
		{
		}
		private static readonly MonoBehaviour MonoBehavior = DispatcherTimerSingleton.Instance;

		private double LastTickTime { get; set; }

		public static void Invoke(double delay, Action action)
		{
			Contract.ArgumentNotNull("action", action);

			if (delay <= 0)
			{
				action();
			}
			else
			{
				new DispatcherTimer(TimeSpan.FromSeconds(delay), (object sender, EventArgs args) =>
				{
					DispatcherTimer dispatcher = sender as DispatcherTimer;
					if (dispatcher != null)
					{
						action();
						dispatcher.Stop();
					}
				}).Start();
			}
		}

		// Summary:
		//     Initializes a new instance of the System.Windows.Threading.DispatcherTimer
		//     class.
		public DispatcherTimer()
		{
		}

		//
		// Summary:
		//     Initializes a new instance of the System.Windows.Threading.DispatcherTimer
		//     class which uses the specified time interval, priority, event handler, and
		//     System.Windows.Threading.Dispatcher.
		//
		// Parameters:
		//   interval:
		//     The period of time between ticks.
		//
		//   callback:
		//     The event handler to call when the System.Windows.Threading.DispatcherTimer.Tick
		//     event occurs.
		public DispatcherTimer(TimeSpan interval, EventHandler callback)
		{
			Interval = interval;
			Tick += callback;
		}

		//
		// Summary:
		//     Gets or sets the period of time between timer ticks.
		//
		// Returns:
		//     The period of time between ticks. The default is 00:00:00.
		//
		// Exceptions:
		//   System.ArgumentOutOfRangeException:
		//     interval is less than 0 or greater than System.Int32.MaxValue milliseconds.
		public TimeSpan Interval { get; set; }
		//
		// Summary:
		//     Gets or sets a value that indicates whether the timer is running.
		//
		// Returns:
		//     true if the timer is enabled; otherwise, false. The default is false.
		private bool isEnabled;
		public bool IsEnabled 
		{ 
			get { return isEnabled; } 
			set { if (value != isEnabled) { isEnabled = value; OnEnabledChanged(); } }
		}

		private void OnEnabledChanged()
		{
			if (IsEnabled)
			{
				MonoBehavior.StartCoroutine(FireTickEvent());
			}
			else
			{
				MonoBehavior.StopCoroutine(FireTickEvent());
			}
		}

		private IEnumerator FireTickEvent()
		{
			LastTickTime = UnityEngine.Time.time;

			while (true)
			{
				float waitTime = (float)(Interval.TotalSeconds - (UnityEngine.Time.time - LastTickTime));
				DebugLog.Info("{0} - DispatcherTimer - Wait for {1} seconds", UnityEngine.Time.time, waitTime);
				yield return new WaitForSeconds(waitTime);

				if (!IsEnabled)
				{
					break;
				}

				if (Tick != null)
				{
					DebugLog.Info("{0} - DispatcherTimer - Tick", UnityEngine.Time.time);
					Tick(this, null);
				}
				LastTickTime = UnityEngine.Time.time;
			}
		}

		private double TimeSinceLastTick
		{
			get
			{
				return UnityEngine.Time.time - LastTickTime;
			}
		}
		
		// Summary:
		//     Occurs when the timer interval has elapsed.
		public event EventHandler Tick;
		
		// Summary:
		//     Starts the System.Windows.Threading.DispatcherTimer.
		public void Start()
		{
			IsEnabled = true;
		}

		//
		// Summary:
		//     Stops the System.Windows.Threading.DispatcherTimer.
		public void Stop()
		{
			IsEnabled = false;
		}
    }
}

