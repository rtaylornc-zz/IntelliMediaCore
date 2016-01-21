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

namespace IntelliMedia
{
	public class SignInViewModel : ViewModel
	{
		private StageManager navigator;
		private SessionState sessionState;
		private AuthenticationService authenticator;
		private SessionService sessionService;
		private CourseSettingsService courseSettingsService;

		public SignInViewModel(
			StageManager navigator, 
			SessionState sessionState,
			AuthenticationService authenticator,
			SessionService sessionService,
			CourseSettingsService courseSettingsService)
		{
			this.navigator = navigator;
			this.sessionState = sessionState;
			this.authenticator = authenticator;
			this.sessionService = sessionService;
			this.courseSettingsService = courseSettingsService;
		}

		public void SignIn(string username, string password)
		{
			try
			{
				if (string.IsNullOrEmpty(username))
				{
					throw new Exception("Username is blank");
				}

				if (string.IsNullOrEmpty(password))
				{
					throw new Exception("Password is blank");
				}

				sessionState.Student = null;
				sessionState.Session = null;

				DebugLog.Info("SignIn {0}", username);
                navigator.Reveal<ProgressIndicatorViewModel>().ThenAs<ProgressIndicatorViewModel>((ProgressIndicatorViewModel progressIndicatorViewModel) =>
                {
                    ProgressIndicatorViewModel.ProgressInfo busyIndicator = progressIndicatorViewModel.Begin("Signing in...");
                    // TODO rgtaylor 2015-12-10 Replace hardcoded 'domain'
                    authenticator.SignIn("domain", username, password)
                    .ThenAs<Student>((Student student) =>
                    {
                        sessionState.Student = student;
                        return sessionService.Start(sessionState.Student.SessionGuid);
                    })
                    .ThenAs<Session>((Session session) =>
                    {
                        sessionState.Session = session;
                        return courseSettingsService.LoadSettings(sessionState.Student.Id);
                    })
                    .ThenAs<CourseSettings>((CourseSettings settings) =>
                    {
                        sessionState.CourseSettings = settings;
                        navigator.Transition(this, typeof(MainMenuViewModel));
                        return true;

                    }).Catch((Exception e) =>
                    {
                        navigator.Reveal<AlertViewModel>(alert =>
                        {
                            alert.Title = "Unable to sign in";
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
			catch (Exception e)
			{
				navigator.Reveal<AlertViewModel>(alert => 
				{
					alert.Title = "Unable to sign in";
					alert.Message = e.Message;
					alert.Error = e;
				});
			}
		}
	}
}
