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
using System.Collections.Generic;

namespace IntelliMedia
{   
    /// <summary>
    /// Inherit from this class to ensure that only one GameObject with this 
    /// MonoBehaviour exists at a time. Also, the Instance property provides
    /// convenient access to the MonoBehavior. This is not the same as a singleton.
    /// A singleton is created by accessing the Instance property. The component
    /// is merely reference by the Instance property.
    /// </summary>
    public class UniqueMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        public bool keepOnSceneChange;

        private static Dictionary<System.Type, Behaviour> referenceCache = new Dictionary<System.Type, Behaviour>();

        private static T current;
        public static T Current
        {
            get
            {
                return current;
            }
        }

		public static bool Exists()
		{
			if (Current == null)
			{
				DebugLog.Warning("{0} unique object's Awake() method has not been called. Attempting to find in scene.", typeof(T).Name);
				current = GameObject.FindObjectOfType<T>();
				if (current == null)
				{
					DebugLog.Warning("Unable to find '{0}' in scene.", typeof(T).Name);
					return false;
				}
			}

			return true;
		}

        /// <summary>
        /// Convenient method for quickly accessing a unique Component on the Unique GameObject (or a 
        /// child of the Unique GameObject)
        /// </summary>
        /// <typeparam name="C">The 1st type parameter.</typeparam>
        public static C Component<C>() where C : Behaviour
        {
            if (referenceCache.ContainsKey(typeof(C)))
            {
                return referenceCache[typeof(C)] as C;
            }
            else
            {
                Behaviour[] components = Current.GetComponentsInChildren<C>(true);
                if (components.Length > 0)
                {
                    referenceCache.Add(typeof(C), components[0]);
                    if (components.Length > 1)
                    {
                        DebugLog.Error("Found more than one component of type '{0}' within '{1}' GameObject hierarchy",
                                       typeof(C).Name,
                                       Current.gameObject.name);
                    }
                    return components[0] as C;
                }
            }

            return null;
        }

        protected virtual void Awake()
        {
            // If this is the first GameObject to be created, assign it to the instance. Otherwise,
            // destroy it since it is a duplicate.
            if (current == null)
            {
                referenceCache.Clear();
                current = this.GetComponent<T>();
                if (keepOnSceneChange)
                {
                    // Do not automatically destory the game object when loading a new scene
                    DontDestroyOnLoad(this);
                }
            }
            else
            {
                DebugLog.Info("'{0}' GameObject with '{1}' component already exists. Destroying '{2}' GameObject with duplicate component.",
                                 Current.name,
                                 typeof(T).Name, 
                                 this.name);
                GameObject.Destroy(gameObject);
            }
        }

		protected virtual void OnDestroy()
        {
            // If the containing GameObject is destroyed for the unique instance, clear the instance member.
            if (this == current)
            {
                current = null;
            }
        }
    }
}

