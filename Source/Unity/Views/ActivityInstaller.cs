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
using IntelliMedia;
using System;
using System.Linq;
using System.Reflection;

namespace IntelliMediaSample
{
	public class ActivityInstaller : MonoInstaller
	{
		[Serializable]
		public class ActivityToView
		{
			public string Urn;
			public string ViewModelClassName;
			public UnityGuiView View;
			public bool ViewIsPrefab;

			private Type viewModelType;
			public Type ViewModelType
			{
				get
				{
					if (viewModelType == null)
					{
						if (String.IsNullOrEmpty(ViewModelClassName))
						{
							throw new Exception("ViewModelClassName is null or empty");
						}

						viewModelType = Assembly.GetExecutingAssembly().GetTypes().First(t => t.Name == ViewModelClassName);
						if (viewModelType == null)
						{
							throw new Exception("Unable to find class named: " + ViewModelClassName);
						}
					}

					return viewModelType;
				}
			}

			private Type viewType;
			public Type ViewType
			{
				get
				{
					if (viewType == null)
					{
						if (View == null)
						{
							throw new Exception("ViewObject is null");
						}
						
						UnityGuiView view = View.GetComponent<UnityGuiView>();
						if (view == null)
						{
							throw new Exception("Unable to find View class attached to: " + View.name);
						}

						viewType = view.GetType();
					}
					
					return viewType;
				}
			}
		}
		public ActivityToView[] activityConfiguration;
			

		public override void InstallBindings()
		{
			foreach (ActivityToView activityToView in activityConfiguration)
			{
				Container.Bind(activityToView.ViewModelType).ToSingle();
				if (activityToView.ViewIsPrefab)
				{
					Container.Bind(activityToView.ViewType).ToTransientPrefab(activityToView.View.gameObject);
				}
				else
				{
					Container.Bind(activityToView.ViewType).ToInstance(activityToView.View);
				}
			}
		}

		public override void Start()
		{
			base.Start();

			foreach (ActivityToView activityToView in activityConfiguration)
			{
				Container.Resolve<ViewFactory>().Register(activityToView.ViewModelType, activityToView.ViewType);
				Container.Resolve<ActivityLauncher>().Register(activityToView.Urn, activityToView.ViewModelType);
			}
		}
	}
}