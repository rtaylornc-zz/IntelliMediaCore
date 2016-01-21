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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using IntelliMedia;

namespace IntelliMedia
{
	public class LoadingViewModel : ViewModel
	{
		private StageManager navigator;
		private List<ProgressInfo> indicators = new List<ProgressInfo>();

		public delegate void ProgressUpdatedHandler();
		public ProgressUpdatedHandler ProgressUpdated;

		private void OnProgressUpdated()
		{
			if (IsFinished)
			{
				if (IsRevealed || IsRevealInProgress)
				{
					navigator.Hide(this);
				}
			}
			else
			{
				if (!IsRevealed && !IsRevealInProgress)
				{
					navigator.Reveal(this);
				}
			}

			if (ProgressUpdated != null)
			{
				ProgressUpdated();
			}
		}

		private ProgressInfo currentProgress;
		public ProgressInfo CurrentProgress
		{
			get { return currentProgress; }
			private set 
			{ 
				if (currentProgress != value) 
				{ 
					ProgressInfo old = currentProgress;
					currentProgress = value;
					OnCurrentProgressChanged(old, currentProgress); 
				}
			}
		}

		private void OnCurrentProgressChanged(ProgressInfo oldInfo, ProgressInfo newInfo)
		{
			if (oldInfo != null)
			{
				oldInfo.ProgressUpdated -= OnProgressUpdated;
			}

			if (newInfo != null)
			{
				newInfo.ProgressUpdated += OnProgressUpdated;
			}

			OnProgressUpdated();
		}

		public bool IsFinished
		{
			get { return CurrentProgress == null; }
		}

		public void Cancel()
		{
			if (!IsFinished && !CurrentProgress.IsCancelled)
			{
				CurrentProgress.Cancel();
			}
		}

		public class ProgressInfo : IDisposable
		{
			private LoadingViewModel ViewModel { get; set; }

			public delegate void CancelledHandler();
			public CancelledHandler Cancelled;

			public ProgressUpdatedHandler ProgressUpdated;
			
			public DateTime StartTime { get; private set; }
			public bool IsBlocking { get; private set; }
			public bool IsCancelled { get; private set; }
					
			private string message;
			public string Message
			{
				get { return message; }
				private set { if (!String.Equals(message, value)) { message = value; OnProgressUpdated(); } }
			}

			private double percentComplete;
			public double PercentComplete
			{
				get { return percentComplete; }
				private set { if (percentComplete != value) { percentComplete = value; OnProgressUpdated(); } }
			}
			
			public ProgressInfo(LoadingViewModel busyViewModel, string message, bool isBlocking = true)
			{
				Contract.ArgumentNotNull("busyViewModel",busyViewModel);
				Contract.ArgumentNotNull("message", message );
				
				StartTime = DateTime.Now;
				
				ViewModel = busyViewModel;
				Message = message;
				IsBlocking = isBlocking;
				
				busyViewModel.Add(this);
			}
			
			public void Dispose()
			{
				ViewModel.Remove(this);
			}

			// If an activity is cancelled, it is still the caller's responsibility to
			// dispose of the ProgressInfo
			public void Cancel()
			{
				IsCancelled = true;
				if (Cancelled != null)
				{
					Cancelled();
				}

				OnProgressUpdated();
			}

			private void OnProgressUpdated()
			{
				if (ProgressUpdated != null)
				{
					ProgressUpdated();
				}
			}
		}

		public LoadingViewModel(StageManager navigator)
		{
			this.navigator = navigator;
		}

		public ProgressInfo Begin(string message, bool isBlocking = true)
		{           
			return new ProgressInfo(this, message, isBlocking);
		}

		public void Add(ProgressInfo indicator)
		{
			indicators.Insert(0, indicator);
			OnActivitiesChanged();
			DebugLog.Info(GetActivities());
		}
		
		public void Remove(ProgressInfo indicator)
		{
			indicators.Remove(indicator);
			OnActivitiesChanged();
			DebugLog.Info(GetActivities());
		}
		
		private void OnActivitiesChanged()
		{
			if (indicators.Count > 0)
			{
				List<ProgressInfo> activeIndicators = indicators.FindAll(a => !a.IsCancelled);
				if (activeIndicators != null && activeIndicators.Count > 0)
				{
					ProgressInfo indicator = indicators.FirstOrDefault(a => a.IsBlocking);
					if (indicator != null)
					{
						CurrentProgress = indicator;
					}
					else
					{
						CurrentProgress = indicators[0];
					}			
					return;
				}
			}
			
			CurrentProgress = null;
		}
		
		private string GetActivities()
		{
			StringBuilder indicatorInfo = new StringBuilder();
			indicatorInfo.AppendLine("BusyViewModel - Active Indicators");
			for (int index = 0;  index < indicators.Count; ++index)
			{
				indicatorInfo.AppendFormat("[{0}] {1}\n", index, indicators[index].Message);
			}
			
			return indicatorInfo.ToString();
		}
	}
}
