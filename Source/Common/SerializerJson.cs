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
using Newtonsoft.Json;

namespace IntelliMedia
{
    /// <summary>
	/// Serialize/deserialize C# objects to JSON. using Newtonsoft.Json;
    /// WARNING: Newtonsoft.Json fails in Unity 3D WebPlayer and on iOS: 
    /// http://forum.unity3d.com/threads/webplayer-and-system-collections-objectmodel-keyedcollection.133505/
    /// </summary>
    public class SerializerJson : ISerializer
    {
        public readonly static SerializerJson Instance = new SerializerJson();

        static readonly JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings()
        {
            TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Objects,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        #region ISerializer implementation

        public string FilenameExtension { get { return "json"; }}

        public T Clone<T> (T obj) where T : class, new()
        {
            return Deserialize<T>(Serialize<T>(obj));
        }

        public string Serialize<T> (T obj, bool readableOutput = false) where T : class, new()
        {
            try
            {
                Formatting formatting = (readableOutput ? Formatting.Indented : Formatting.None);
                string json = JsonConvert.SerializeObject(obj, formatting, settings);
                //DebugLog.Info(Subsystem.Serialization, "Serialized JSON:\n{0}", json);
                return json;
            }
            catch (Exception e)
            {
                throw new Exception("Unable to serialize to JSON: " + DebugLog.SafeName(obj), e);
            }
        }

        public T Deserialize<T>(string json) where T : class, new()
        {
            try
            {                
                return JsonConvert.DeserializeObject(json, settings) as T;
            }
            catch (Exception e)
            {
                throw new Exception("Unable to deserialize JSON:\n" + json, e);
            }
        }

        public string GetHeader<T>() where T : class, new()
        {
            return "";
        }

        #endregion
    }
}
