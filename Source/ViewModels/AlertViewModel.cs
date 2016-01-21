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
	public class AlertViewModel : ViewModel
	{
		private StageManager navigator;

		public string Title { get; set; }
		public string Message { get; set; }
		public Exception Error { get; set; }
		public string[] ButtonLabels { get; set; }

		public delegate void AlertDismissedHandler(int buttonIndex);
		public AlertDismissedHandler AlertDismissed;

		public AlertViewModel(StageManager navigator)
		{
			Contract.ArgumentNotNull("navigator", navigator);

			this.navigator = navigator;

			ButtonLabels = new string[] { "OK" };
		}

		public override void OnStartReveal ()
		{
			if (Error != null)
			{
				DebugLog.Error("Error Alert Displayed. {0}. {1}", Error.Message, Error.StackTrace);
			}
			base.OnStartReveal();
		}

		public void ButtonPressed(int index)
		{
			navigator.Hide(this);
			if (AlertDismissed != null)
			{
				AlertDismissed(index);
			}
		}
	}
}
