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
using System.Collections;
using System.Collections.Generic;

namespace IntelliMedia
{
	public class ActivityLauncher
	{
		private ActivityService activityService;
		private ViewModel.Factory viewModelFactory;
		private Dictionary<string, Type> urnToViewModelType = new Dictionary<string, Type>();
		private LogEntry startedEntry;

		public ActivityLauncher(
			ActivityService activityService, 
			ViewModel.Factory viewModelFactory)
		{
			this.activityService = activityService;
			this.viewModelFactory = viewModelFactory;
		}
		
		public Promise Start(Student student, Activity activity, bool resetActivityState = false)
		{
			Promise promise = new Promise();

			try
			{
				ActivityState activityState = null;

				activityService.LoadActivityState(student.Id, activity.Id, true).ThenAs<ActivityState>((ActivityState state) =>
				{
					activityState = state;
					if (resetActivityState || !activityState.CanResume)
					{
						// Generate a new trace ID for restarts or new games that don't have saved state
						activityState.TraceId = Guid.NewGuid().ToString();
					}

					DebugLog.Info("Start activity!");
//					startedEntry = TraceLog.Player(TraceLog.Action.Started, "Activity",
//					                               "Name", activity.Name,
//					                               "ActivityUri", activity.Uri,
//					                               "IsComplete", activityState.IsComplete,
//					                               "CanResume", activityState.CanResume,
//					                               "Restart", resetActivityState,
//					                               "StartDate", activityState.ModifiedDate.ToIso8601(),
//					                               "Username", student.Username,
//					                               "SubjectId", student.SubjectId,
//					                               "TraceId", activityState.TraceId);
//					
//					if (activity.Uri.Contains("episode"))
//					{
//						LaunchEpisode(activityState, activity.Name, activity.Uri, resetActivityState);                
//					}
//					else if (activity.Uri.Contains("assessment"))
//					{
//						LaunchAssessment(activityState, activity.Name, activity.Uri);                
//					}
//					else if (activity.Uri.Contains("http"))
//					{
//						LaunchUrl(activityState, activity.Name, activity.Uri);
//					} 
//					else
//					{
//						throw new Exception(String.Format("{0} has an unknown URI type: ", activity.Name, activity.Uri));
//					}	

					promise.Resolve(viewModelFactory.Resolve<ActivityViewModel>(Resolve(activity.Uri), vm =>
					{
						vm.Activity = activity;
						vm.ActivityState = activityState;
					}));

//					promise.Resolve(viewModelFactory.Resolve<WebActivityViewModel>(web => 
//					{
//						web.Title = "Web Browser Launched";
//						web.Message = "Complete web viewer action for " + activity.Name;
//						web.URL = "http://www.google.com";
//					}));

					return true;
				})
				.Catch((Exception e) => 
				{
					promise.Reject(e);
				});                 
			}
			catch (Exception e)
			{
				promise.Reject(e);
			}

			return promise;
		}

		private void LaunchEpisode(ActivityState activityState, string activityName, string episodeUrn, bool restart = false)
		{
//			Episode episode = Episode.All.FirstOrDefault(e => String.Compare(e.Urn, episodeUrn, StringComparison.CurrentCultureIgnoreCase) == 0);
//			if (episode != null)
//			{
//				Action startGameAction = () =>
//				{
//					if (restart)
//					{
//						// Clear previous save data
//						activityState.Restart();
//					}
//					
//					activityState.RecordLaunch();
//					
//					SaveActivityState("Launching", false, activityState,
//					                  (bool success, ActivityState savedActivityState, string error) =>
//					                  {
//						GameViewModel.Instance.StartGame(episode, activityName, savedActivityState, ActivityEnded);
//					});
//				};
//
//				// TODO rgtaylor 2015-12-16 Refactor
////				if (EyeTrackingService.Instance.IsEnabled)
////				{
////					EyeTrackingService.Instance.Initialize();
////					EyeTrackingService.Instance.Calibrate(startGameAction);
////				}
////				else
//				{
//					startGameAction();
//				}
//			}
//			else
//			{
//				throw new Exception(String.Format("The '{0}' activity is not supported.", activityName));
//			}
		}

		public Type Resolve(string activityUrn)
		{
			Contract.ArgumentNotNull("activityUrn", activityUrn);
			
			if (!urnToViewModelType.ContainsKey(activityUrn))
			{
				throw new Exception(String.Format("Activity URN not registered for '{0}'", activityUrn));
			}
			
			return urnToViewModelType[activityUrn];
		}
		
		public void Register(string urn, Type viewModel)
		{
			urnToViewModelType[urn] = viewModel;
		}
	}
}
