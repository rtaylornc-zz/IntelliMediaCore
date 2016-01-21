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
using System;
using System.Collections;
using System.Collections.Generic;

namespace IntelliMedia
{
    public sealed class SceneService : MonoBehaviourSingleton<SceneService>
    {
        public delegate void LoadScenesHandler(bool success, string error);

        public TaskProgress LoadScene(string sceneName, bool isAdditive, LoadScenesHandler callback)
        {
            return LoadScenes(new List<SceneInfo>() { new SceneInfo() { DisplayName = sceneName, Name = sceneName }}, isAdditive, callback);
        }

        public TaskProgress LoadScenes(List<SceneInfo> Scenes, bool isAdditive, LoadScenesHandler callback)
        {
			Contract.ArgumentNotNull("Scenes", Scenes);
			Contract.ArgumentNotNull("callback", callback);

            TaskProgress progress = new TaskProgress();
            StartCoroutine(LoadScenesAsync(Scenes, isAdditive, progress, callback));

            return progress;
        }

        private IEnumerator LoadScenesAsync(List<SceneInfo> Scenes, bool isAdditive, TaskProgress progress, LoadScenesHandler callback) 
        {
            // Release the process to allow caller to attached to Updated event to monitor progress
            yield return new WaitForEndOfFrame();

            string error = null;
            try
            {
                for (int index = 0; index < Scenes.Count && !progress.IsCancelled; ++index)
                {
					float percentComplete =  (100 * index / Scenes.Count);
					string message = Scenes[index].DisplayName;
					progress.Update(percentComplete, message);

                    // Release the process to update the UI
                    yield return new WaitForSeconds(1.0f);

                    AsyncOperation asyncOp;
                    if (!isAdditive)
                    {
                        if (Application.loadedLevelName.CompareTo(Scenes[index].Name) != 0)
                        {
                            // Add the remaing scenes
                            asyncOp = Application.LoadLevelAsync(Scenes[index].Name);
                        }
                        else
                        {
                            continue;
                        }
                        isAdditive = true;
                    }
                    else
                    {
                        // Load the first scene
                        asyncOp = Application.LoadLevelAdditiveAsync(Scenes[index].Name);
                    }
                    
                    while (!asyncOp.isDone && !progress.IsCancelled)
                    {
						progress.Update(((asyncOp.progress/Scenes.Count) + (index / Scenes.Count)) * 100);
                        yield return new WaitForEndOfFrame();
                    }
                }   

				// Release to allow Awake() methods to be called on newly loaded game objects
				yield return new WaitForSeconds(2.0f);
            }
            finally
            {
                progress.Finished();
                if (callback != null)
                {
                    callback(error == null, error);
                }
            }
        }
    }
}
