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
using System;
using System.Collections.Generic;
using Zenject;

namespace IntelliMedia
{
    public class ViewFactory
    {
        DiContainer container;
        Dictionary<Type, Type> modelToView = new Dictionary<Type, Type>();

        public ViewFactory(DiContainer container)
        {
            this.container = container;
        }

        public IView Resolve(ViewModel viewModel)
        {
            Contract.ArgumentNotNull("viewModel", viewModel);

            if (!modelToView.ContainsKey(viewModel.GetType()))
            {
                throw new Exception(String.Format("View not registered for '{0}'", viewModel.GetType().Name));
            }

            IView view = (IView)container.Resolve(modelToView[viewModel.GetType()]);
            view.BindingContext = viewModel;

            return view;
        }

        public void Register(Type viewModelType, Type viewType)
        {
            Contract.ArgumentNotNull("viewModelType", viewModelType);
            Contract.ArgumentNotNull("viewType", viewType);

            modelToView[viewModelType] = viewType;
        }

        public void Register<TViewModel, TView>() where TViewModel : ViewModel where TView : IView
        {
            modelToView[typeof(TViewModel)] = typeof(TView);
        }
    }
}
