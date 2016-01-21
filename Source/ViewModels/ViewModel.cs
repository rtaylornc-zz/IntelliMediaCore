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
using System.ComponentModel;
using System;
using System.Linq;
using System.Runtime;
using System.Linq.Expressions;
using Zenject;
using System.Reflection;

namespace IntelliMedia
{
	public class ViewModel
	{
		public string Name { get; set; }
		
		public void SetState<T>(Action<T> action) where T : ViewModel
		{
			action(this as T);
		}

		public bool IsRevealed { get; private set; }
		public bool IsRevealInProgress { get; private set; }
		public bool IsHideInProgress { get; private set; }

		public virtual void OnStartReveal()
		{
			IsRevealInProgress = true;
		}
		
		public virtual void OnFinishReveal()
		{
			IsRevealInProgress = false;
			IsRevealed = true;
		}
		
		public virtual void OnStartHide()
		{
			IsHideInProgress = true;
		}
		
		public virtual void OnFinishHide()
		{
			IsHideInProgress = false;
			IsRevealed = false;
		}

		public class Factory
		{
			DiContainer container;
			
			public Factory(DiContainer container)
			{
				this.container = container;
			}

			public ViewModel Resolve(Type viewModelType)
			{
				Contract.ArgumentNotNull("viewModelType", viewModelType);

				ViewModel vm = (ViewModel)container.Resolve(viewModelType);
				if (vm == null)
				{
					throw new Exception(String.Format("Unable to resolve '{0}' in IoC container", viewModelType.Name));
				}
				
				return vm;	
			}

			public ViewModel Resolve(string className)
			{
				Contract.ArgumentNotNull("className", className);
				
				if (string.IsNullOrEmpty(className))
				{
					throw new Exception("Class name cannot be empty or null");
				}			

				return Resolve(Type.GetType(className));
			}

			public TViewModel Resolve<TViewModel>(Type viewModelType, Action<TViewModel> setStateAction = null) where TViewModel : ViewModel
			{
				TViewModel vm = Resolve(viewModelType) as TViewModel;
				if (vm == null)
				{
					throw new Exception(String.Format("Unable to resolve '{0}' in IoC container", typeof(TViewModel).Name));
				}
				
				if (setStateAction != null)
				{
					vm.SetState(setStateAction);
				}
				
				return vm;	
			}

			public TViewModel Resolve<TViewModel>(Action<TViewModel> setStateAction = null) where TViewModel : ViewModel
			{
				TViewModel vm = (TViewModel)container.Resolve(typeof(TViewModel));
				if (vm == null)
				{
					throw new Exception(String.Format("Unable to resolve '{0}' in IoC container", typeof(TViewModel).Name));
				}

				if (setStateAction != null)
				{
					vm.SetState(setStateAction);
				}

				return vm;				
			}
		}

	}
}
