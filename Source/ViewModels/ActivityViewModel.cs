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
using Zenject;
using System.Collections.Generic;
using System;

namespace IntelliMedia
{
	public class ActivityViewModel : ViewModel
	{
		private StageManager navigator;
		private ActivityService activityService;

		public Activity Activity { get; set; }
		public ActivityState ActivityState { get; set; }
		
		public ActivityViewModel(StageManager navigator, ActivityService activityService)
		{
			Contract.ArgumentNotNull("navigator", navigator);
			Contract.ArgumentNotNull("activityService", activityService);
			
			this.navigator = navigator;
			this.activityService = activityService;
		}
		
		protected TDataModel DeserializeActivityData<TDataModel>() where TDataModel : class, new()
		{
			if (ActivityState.GameData != null)
			{
				return SerializerXml.Instance.Deserialize<TDataModel>(ActivityState.GameData);
			}
			else
			{
				return new TDataModel();
			}
		}

		protected void SerializeActivityData<TDataModel>(TDataModel data) where TDataModel : class, new()
		{
			if (data != null)
			{
				ActivityState.GameData = SerializerXml.Instance.Serialize<TDataModel>(data);
			}
			else
			{
				ActivityState.GameData = null;
			}
		}
		
		protected void SaveActivityStateAndTransition<ToViewModel>()
		{												
			DebugLog.Info("Save state");
            navigator.Reveal<ProgressIndicatorViewModel>().ThenAs<ProgressIndicatorViewModel>((ProgressIndicatorViewModel progressIndicatorViewModel) =>
            {
                ProgressIndicatorViewModel.ProgressInfo busyIndicator = progressIndicatorViewModel.Begin("Saving...");
                activityService.SaveActivityState(ActivityState)
                    .ThenAs<ActivityState>((ActivityState activityState) =>
                    {
                        navigator.Transition(this, typeof(ToViewModel));
                        return true;
                    })
                    .Catch((Exception e) =>
                           {
                               navigator.Reveal<AlertViewModel>(alert =>
                                                            {
                                                                alert.Title = "Unable to save";
                                                                alert.Message = e.Message;
                                                                alert.Error = e;
                                                                alert.AlertDismissed += ((int index) => DebugLog.Info("Button {0} pressed", index));
                                                            });

                           }).Finally(() =>
                                  {
                                      busyIndicator.Dispose();
                                  });

                return true;
            });
		}
	}
}
