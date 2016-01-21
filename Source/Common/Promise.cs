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
using System.Collections.Generic;

namespace IntelliMedia
{
	public delegate object FulfilledHandler(object result);
	public delegate object FulfilledHandler<TParam>(TParam result);
	public delegate void RejectedHandler(Exception e);
	
	public interface IThenable
	{
		IThenable Then(FulfilledHandler fulfilled, RejectedHandler rejected = null);
		IThenable ThenAs<TParam>(FulfilledHandler<TParam> fulfilled, RejectedHandler rejected = null);
		IThenable Catch(RejectedHandler rejected);
		void Finally(Action action);
	}

	/// <summary>
	/// A Promise is an object that is used as a placeholder for the eventual results of a deferred (and possibly asynchronous) computation.
	/// </summary>
	/// <remarks>
	/// Loosely based on ECMA standard for Javascript 6 definition of Promise:
	/// https://tc39.github.io/ecma262/#sec-promise-objects
	/// </remarks>
	public class Promise : IThenable
	{
		public enum PromiseState
		{
			Pending,
			Fulfilled,
			Rejected	
		}
		public PromiseState State { get; private set; }

		public bool IsSettled { get { return State != PromiseState.Pending; }}
		public bool IsFulfilled { get { return State == PromiseState.Fulfilled; }}
		public bool IsRejected { get { return State == PromiseState.Rejected; }}

		private object Result { get; set; }
		private FulfilledHandler FulfillReaction { get; set; }
		private RejectedHandler RejectReaction { get; set; }
		private List<Promise> thenPromises = new List<Promise>();

		public Promise()
		{
			State = PromiseState.Pending;
		}

		public Promise(FulfilledHandler fulfilled, RejectedHandler rejected) : this()
		{
			FulfillReaction += fulfilled;
			RejectReaction += rejected;
		}

		public void Resolve(object result) 
		{
			if (IsSettled)
			{
				throw new Exception("Attempted to resolve a settled promise.");
			}

			try 
			{
				Result = (FulfillReaction != null ? FulfillReaction(result) : result);

				Promise promiseResult = Result as Promise;

				if (promiseResult != null)
				{
					switch(promiseResult.State)
					{
					case PromiseState.Pending:
						promiseResult.thenPromises.AddRange(this.thenPromises);
						this.thenPromises.Clear();
						State = PromiseState.Fulfilled;
						break;

					case PromiseState.Fulfilled:
						Resolve(promiseResult.Result);
						break;

					case PromiseState.Rejected:
						throw (Exception)(promiseResult.Result);
					}
				}
				else
				{
					foreach (Promise promise in thenPromises)
					{
						promise.Resolve(Result);
					}
					State = PromiseState.Fulfilled;
				}
			}
			catch(Exception e)
			{
				Reject(e);
			}
		}

		public void Reject(Exception error)
		{
			Contract.ArgumentNotNull("error", error);

			if (IsSettled)
			{
				throw new Exception("Attempted to rejected a settled promise.");
			}

			try 
			{
				if (RejectReaction != null)
				{
					RejectReaction(error);
				}
				Result = error;
				State = PromiseState.Rejected;
				foreach (Promise promise in thenPromises)
				{
					promise.Reject(error);
				}
			}
			catch(Exception e)
			{
				RejectReaction = null;
				Reject(e);
			}
		}

		#region IThenable implementation

		public IThenable ThenAs<TParam>(FulfilledHandler<TParam> fulfilled, RejectedHandler rejected = null)
		{
			return Then((object result) =>
			{
				if (result is TParam || object.Equals(result, default(TParam)))
				{
					return fulfilled((TParam)result);
				}
				else
				{
					throw new Exception(string.Format("Attempted fulfilled handler expected argument of type '{0}', but receieved '{1}'",
					                                  typeof(TParam).Name, (result != null ? result.GetType().Name : "null")));
				}
			},
			rejected);
		}

		public IThenable Then(FulfilledHandler fulfilled, RejectedHandler rejected = null)
		{
			if (IsSettled)
			{
				throw new Exception("Trying to add 'then' to a promise is already settled.");
			}

			Promise promise = new Promise(fulfilled, rejected);
			thenPromises.Add(promise);

			return promise;
		}

		public IThenable Catch(RejectedHandler rejected)
		{
			Contract.ArgumentNotNull("rejected", rejected);

			return Then(null, rejected);
		}
		
		public void Finally(Action action)
		{
			Contract.ArgumentNotNull("action", action);

			Then( (object result) => 
			{
				action();
				return null;
			},
			(Exception e) => 
			{
				action();
			});
		}
		
		#endregion
	}
}
