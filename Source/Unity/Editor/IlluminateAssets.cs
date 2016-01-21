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
using UnityEditor;
using System.IO;
using System;
using IntelliMedia.DecisionMaking;

namespace IntelliMedia
{
    public class IlluminateAssets : EditorWindow
    {        
        [MenuItem("Assets/Create/Illuminate/Brain")]   
        public static void CreateBrain ()
        {     
            BehaviorTree behaviorTree = new BehaviorTree();
            behaviorTree.Root = new Sequence(
                new SelectDestination("TestDest1", "TestDest2", "TestDest3"),
                new MoveTo(),
                new Wait(5));

    //        behaviorTree.Root = 
    //            new Selector( 
    //                new Sequence(
    //                    new SelectDestination("TestDest1", "TestDest2", "TestDest3"),
    //                    new MoveTo(),
    //                    new Wait(5)));

            Brain brain = CreateAsset<Brain>("Brain"); 
            brain.DecisionMaker = behaviorTree;
            EditorUtility.SetDirty(brain);
        }

        public static T CreateAsset<T>(string name = null) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            
            string path = AssetDatabase.GetAssetPath (Selection.activeObject);
            if (path == "") 
            {
                path = "Assets";
            } 
            else if (Path.GetExtension (path) != "") 
            {
                path = path.Replace (Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
            }

            if (name == null)
            {
                name = "New " + typeof(T).ToString();
            }
            
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path + "/" + name + ".asset");
            
            AssetDatabase.CreateAsset (asset, assetPathAndName);
            
            AssetDatabase.SaveAssets ();
            EditorUtility.FocusProjectWindow ();
            Selection.activeObject = asset;

            return asset;
        }
    }
}
