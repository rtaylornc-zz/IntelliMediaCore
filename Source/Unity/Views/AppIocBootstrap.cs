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
	public class AppIocBootstrap : MonoInstaller
	{
		public AlertView alertPrefab;
		public ProgressIndicatorView progressIndicatorPrefab;
        public SignInView signInViewPrefab;
		public MainMenuView mainMenuViewPrefab;

        public override void InstallBindings()
		{
			Container.Bind<AppSettings>().ToSingle();
			Container.Bind<SessionState>().ToSingle();
			Container.Bind<ActivityLauncher>().ToSingle();
			Container.Bind<AuthenticationService>().ToSingle();
			Container.Bind<SessionService>().ToSingle();
			Container.Bind<CourseSettingsService>().ToSingle();
			Container.Bind<ActivityService>().ToSingle();

			Container.Bind<ViewFactory>().ToSingle();
			Container.Bind<ViewModel.Factory>().ToSingle();
			Container.Bind<StageManager>().ToSingle();

			Container.Bind<AlertViewModel>().ToTransient();
			Container.Bind<AlertView>().ToTransientPrefab(alertPrefab.gameObject);

			Container.Bind<ProgressIndicatorViewModel>().ToSingle();
			Container.Bind<ProgressIndicatorView>().ToTransientPrefab(progressIndicatorPrefab.gameObject);

            Container.Bind<SignInViewModel>().ToSingle();
			Container.Bind<SignInView>().ToTransientPrefab(signInViewPrefab.gameObject);

			Container.Bind<MainMenuViewModel>().ToSingle();
			Container.Bind<MainMenuView>().ToTransientPrefab(mainMenuViewPrefab.gameObject);
        }

		public override void Start()
		{
			base.Start();

			Container.Resolve<AppSettings>().ServerURI = "http://intellimedia-portal-dev.appspot.com/";

			Container.Resolve<ViewFactory>().Register<AlertViewModel, AlertView>();
			Container.Resolve<ViewFactory>().Register<ProgressIndicatorViewModel, ProgressIndicatorView>();
            Container.Resolve<ViewFactory>().Register<LoadingViewModel, LoadingView>();
            Container.Resolve<ViewFactory>().Register<SignInViewModel, SignInView>();
			Container.Resolve<ViewFactory>().Register<MainMenuViewModel, MainMenuView>();

			Container.Resolve<StageManager>().Reveal<SignInViewModel>();
		}
	}
}