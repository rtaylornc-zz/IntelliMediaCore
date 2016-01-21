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
using Zenject;

namespace IntelliMedia
{
	public class StageManager
	{
		private ViewModel.Factory viewModelFactory;
		private ViewFactory viewFactory;

		private HashSet<IView> revealedViews = new HashSet<IView>();

		public StageManager(ViewModel.Factory viewModelFactory, ViewFactory viewFactory)
		{
			this.viewModelFactory = viewModelFactory;
			this.viewFactory = viewFactory;
		}

		public void Transition(ViewModel from, ViewModel to)
		{
			Hide(from, (IView view) =>
			{
				Reveal(to);
			});
		}

		public void Transition(ViewModel from, Type toViewModelType)
		{
			Transition(from, viewModelFactory.Resolve(toViewModelType));
		}

		public Promise Reveal(ViewModel vm, VisibilityEvent.OnceEventHandler handler = null)
		{
			Contract.ArgumentNotNull("vm", vm);

            Promise promise = new Promise();

            DebugLog.Info("StageManager.Reveal: {0}", vm.GetType().Name);

			IView view = revealedViews.FirstOrDefault(v => v.BindingContext == vm);
			if (view == null)
			{
				view = viewFactory.Resolve(vm);
				view.BindingContext = vm;
				revealedViews.Add(view);
				view.Reveal(false, (IView revealedView) =>
                {
                    promise.Resolve(revealedView.BindingContext);
                });
            }
			
			return promise;
		}

		public Promise Reveal<TViewModel>(Action<TViewModel> setStateAction = null) where TViewModel : ViewModel
		{
			TViewModel vm = viewModelFactory.Resolve<TViewModel>(setStateAction);

			return Reveal(vm);
		}

		public ViewModel Hide(ViewModel vm, VisibilityEvent.OnceEventHandler handler = null)
		{
			Contract.ArgumentNotNull("vm", vm);

			DebugLog.Info("StageManager.Hide: {0}", vm.GetType().Name);

			IView view = revealedViews.FirstOrDefault(v => v.BindingContext == vm);
			
			if (view != null)
			{			
				view.Hide(false, handler);
				revealedViews.Remove(view);
			}
			
			return vm;
		}

		public TViewModel Hide<TViewModel>(Action<TViewModel> setStateAction = null) where TViewModel : ViewModel
		{
			return (TViewModel)Hide(viewModelFactory.Resolve<TViewModel>(setStateAction));
		}

		public bool IsRevealed<TViewModel>() where TViewModel : ViewModel
		{
			TViewModel vm = viewModelFactory.Resolve<TViewModel>();
			IView view = revealedViews.FirstOrDefault(v => v.BindingContext == vm);
			if (view != null && !vm.IsRevealed)
			{
				/*
				throw new Exception(String.Format("Stage mismatch: '{0}' is in revealed list, but {1}.IsRevealed=false",
				                                  view.GetType().Name,
				                                  typeof(TViewModel).Name));
				                                  */
			} 

			return view != null;
		}
	}
}
