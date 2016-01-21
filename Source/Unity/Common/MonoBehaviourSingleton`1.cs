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
using UnityEngine;

namespace IntelliMedia
{   
    /// <summary>
    /// Inherit from this class to create a Unity MonoBehaviour-derived singleton.
    /// </summary>
    public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
		public static bool IsBeingDestroyed { get; private set; }

        // Immediately initialize the _instance instead of checking for null every time 
        // get() is called on the Instance member. Adding 'readonly' to _instance makes it
        // threadsafe, but we can't have a Reset() method. Since this Singleton is intended
        // for accessing Unity APIs, which are singlethreaded, threadsafety isn't a requirement.
        // This implementation is based the following: http://www.dotnetperls.com/singleton
		static T _instance;
        public static T Instance
        {
            get
            {
				if (_instance == null && !IsBeingDestroyed)
				{
					_instance = Create();
				}
                return _instance;
            }
        }

        /// <summary>
        /// Factory method that creates a GameObject to host the singleton
        /// </summary>
        private static T Create()
        {
			DebugLog.Info("Creating {0} singleton", typeof(T).Name);

            GameObject gameObject = new GameObject(typeof(T).Name,
                                                   typeof(T));
            // Do not automatically destory the game object when loading a new scene
            DontDestroyOnLoad(gameObject);

            return (T)(gameObject.GetComponent(typeof(T)));
        }

        /// <summary>
        /// Destroy and recreate the singleton object
        /// </summary>
        public static void Reset()
        {
			DebugLog.Info("Reset {0} singleton", typeof(T).Name);

            if (_instance != null)
            {
                GameObject previousObject = _instance.gameObject;
                _instance = Create();
                Destroy(previousObject);
            }
        }

        // Don't allow this class to be instantiated outside of the singleton pattern
        protected MonoBehaviourSingleton()
        {
        }

		protected virtual void OnDestroy()
		{
			DebugLog.Info("OnDestroy {0} singleton", typeof(T).Name);
			IsBeingDestroyed = true;
		}

		protected virtual void OnApplicationQuit()
		{
			DebugLog.Info("Destroying {0} singleton on application exit", typeof(T).Name);

			if (_instance != null)
			{
				Destroy(_instance.gameObject);
				_instance = null;
			}
		}
    }
}

