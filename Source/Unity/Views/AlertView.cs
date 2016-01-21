//---------------------------------------------------------------------------------------
// Copyright 2015 North Carolina State University
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
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Zenject;
using IntelliMedia;
using UnityEngine.Events;

namespace IntelliMedia
{
	public class AlertView : UnityGuiView
	{
		public Text title;
		public Text message;
		public Transform buttonRow;

		public AlertViewModel ViewModel { get { return (AlertViewModel)BindingContext; }}

		protected override void OnBindingContextChanged(ViewModel oldViewModel, ViewModel newViewModel)
		{
			Contract.PropertyNotNull("title", title);
			Contract.PropertyNotNull("message", message);
			Contract.PropertyNotNull("buttonRow", buttonRow);

			base.OnBindingContextChanged(oldViewModel, newViewModel);

			UpdateControls();
		}

		private void UpdateControls()
		{
			if (ViewModel == null)
			{
				return;
			}

			title.text = (!string.IsNullOrEmpty (ViewModel.Title) ? ViewModel.Title : "");
			message.text = (!string.IsNullOrEmpty (ViewModel.Message) ? ViewModel.Message : "");

			// Use existing button as a prefab for creating new buttons
			Transform buttonPrefab = (buttonRow.childCount > 0 ? buttonRow.GetChild (0) : null);
			if (buttonPrefab == null) 
			{
				throw new System.Exception ("ButtonRow in AlertView prefab is missing button object");
			}

			// Remove any additional buttons in the row
			for (int index = 1; index < buttonRow.childCount; ++index) 
			{
				Destroy (buttonRow.GetChild (index).gameObject);
			}

			buttonRow.DetachChildren();
			for (int index = 0; index < ViewModel.ButtonLabels.Length; ++index) 
			{
				// Use local variable to ensure the correct value is passed lambda function below
				int buttonIndex = index;
				string buttonLabel = ViewModel.ButtonLabels [index];
				Transform newButton = GameObject.Instantiate (buttonPrefab);
				newButton.name = buttonLabel + "Button";
				newButton.GetComponent<Button>().onClick.AddListener (() => OnClicked (buttonIndex));
				newButton.GetComponentInChildren<Text> ().text = buttonLabel;
				newButton.transform.SetParent (buttonRow);
			}

			Destroy (buttonPrefab.gameObject);
		}

		public void OnClicked(int buttonIndex)
		{
			ViewModel.ButtonPressed(buttonIndex);	
		}
	}
}