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
using System.Collections.Generic;
using System;
using IntelliMedia;
using UnityEngine.EventSystems;

namespace IntelliMedia
{
	public class UnitySceneView : MonoBehaviour, IView
    {
        public string sceneName;
        public bool destroyOnHide;

        public bool IsLoaded { get; private set; }

        public VisibilityEvent RevealedEvent = new VisibilityEvent();
		public VisibilityEvent HiddenEvent = new VisibilityEvent();

		public readonly BindableProperty<ViewModel> ViewModelProperty = new BindableProperty<ViewModel>();	
		public ViewModel BindingContext 
		{ 
			get { return ViewModelProperty.Value; }
			set { ViewModelProperty.Value = value; }
		}

		protected virtual void OnBindingContextChanged(ViewModel oldViewModel, ViewModel newViewModel)
		{
		}	

		public UnitySceneView()
		{
			this.ViewModelProperty.ValueChanged += OnBindingContextChanged;
		}

		public void Reveal(bool immediate = false, VisibilityEvent.OnceEventHandler handler = null)
		{
			if (handler != null)
			{
				RevealedEvent.EventTriggered += handler;
			}

			OnAppearing();
			if (immediate || BindingContext.IsRevealed)
			{
				OnVisible();
			}
			else
			{
                SceneService.Instance.LoadScene(sceneName, true, (bool success, string error) =>
                {
                    IsLoaded = true;
                    OnVisible();
                });
			}
		}

		public void Hide(bool immediate = false, VisibilityEvent.OnceEventHandler handler = null)
		{
			if (handler != null)
			{
				HiddenEvent.EventTriggered += handler;
			}

			OnDisappearing();
			if (immediate || !BindingContext.IsRevealed)
			{
				OnHidden();
			}
			else
			{
				//GetComponent<Animator>().SetTrigger("Hide");
			}
		}

		public virtual void OnAppearing()
		{
			//gameObject.SetActive(true);
			BindingContext.OnStartReveal();
		}

		public virtual void OnVisible()
		{
			BindingContext.OnFinishReveal();
			RevealedEvent.Trigger(this);
		}

		public virtual void OnDisappearing()
		{
			BindingContext.OnStartHide();
		}

		public virtual void OnHidden()
		{
			//gameObject.SetActive(false);
			BindingContext.OnFinishHide();
			HiddenEvent.Trigger(this);
			if (destroyOnHide)
			{
				//Destroy(this.gameObject);
			}
		}

		// Create
		// view.transform.SetParent(rootView.transform);
		// view.GetComponent<UnityEngine.RectTransform>().localPosition = new UnityEngine.Vector3();

		public virtual void OnDestroy()
		{
			if (BindingContext.IsRevealed)
			{
				Hide(true);
			}
			BindingContext = null;
		}
	}
}