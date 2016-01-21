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
using System.Text.RegularExpressions;
using System;

namespace IntelliMedia
{
	public class WebActivityViewModel : ActivityViewModel
	{			
		private SessionState sessionState;

		public string Title { get; set; }
		public string Message { get; set; }
		public string Url { get; private set; }
						
		public WebActivityViewModel(SessionState sessionState, StageManager navigator, ActivityService activityService) : base(navigator, activityService)
		{
			Contract.ArgumentNotNull("sessionState", sessionState);

			this.sessionState = sessionState;
		}

		public override void OnStartReveal()
		{
			Contract.PropertyNotNull("Activity", Activity);
			Contract.PropertyNotNull("sessionState", sessionState);
			Contract.PropertyNotNull("sessionState.Student", sessionState.Student);

			Title = "Web Browser Launched";
			Message = String.Format("Use the web browser to complete the {0} activity.\nPress the <b>Done</b> button when you are finished.", Activity.Name);
			Url = SubstituteParameters(Activity.Uri);

			base.OnStartReveal ();
		}
		
		private string SubstituteParameters(string uri)
		{
			Student student = sessionState.Student;

			if (student == null)
			{
				throw new Exception("Student information has not been initialized. Student has not signed in.");
			}
			
			if (!string.IsNullOrEmpty(student.SubjectId))
			{
				uri = Regex.Replace(uri, "{subjectid}", student.SubjectId, RegexOptions.IgnoreCase);
			}
			
			if (!string.IsNullOrEmpty(student.Username))
			{
				uri = Regex.Replace(uri, "{username}", student.Username, RegexOptions.IgnoreCase);
			}
			
			if (!string.IsNullOrEmpty(student.CourseName))
			{
				uri = Regex.Replace(uri, "{course}", student.CourseName, RegexOptions.IgnoreCase);
			}
			
			if (!string.IsNullOrEmpty(student.GroupName))
			{
				uri = Regex.Replace(uri, "{group}", student.GroupName, RegexOptions.IgnoreCase);
			}
			
			if (!string.IsNullOrEmpty(student.InstructorName))
			{
				uri = Regex.Replace(uri, "{instructor}", student.InstructorName, RegexOptions.IgnoreCase);
			}
			
			if (!string.IsNullOrEmpty(student.InstitutionName))
			{
				uri = Regex.Replace(uri, "{institution}", student.InstitutionName, RegexOptions.IgnoreCase);
			}
			
			return Uri.EscapeUriString(uri);
		}

		
		public void DoneButtonPressed()
		{
			SaveActivityStateAndTransition<MainMenuViewModel>();
		}
	}
}
