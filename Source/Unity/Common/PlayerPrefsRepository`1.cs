#region Copyright 2014 North Carolina State University
//---------------------------------------------------------------------------------------
// Copyright 2015 North Carolina State University
//
// Computer Science Department
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
#endregion

using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;

namespace IntelliMedia
{
    public class PlayerPrefsRepository<T> : Repository<T> where T : class, new()
    {
        protected ISerializer Serializer { get; set; }

        private string KeysIndexName;

        private List<object> keysIndex;
        private List<object> KeysIndex
        {
            get
            {
                if (keysIndex == null)
                {
                    keysIndex = LoadIndex();
                }

                return keysIndex;
            }
        }

        public PlayerPrefsRepository(ISerializer serializer = null)
        {
            KeysIndexName = GetKeyPath("KeysIndex");

            // Default to XML serializer
            Serializer = (serializer != null ? serializer : SerializerXml.Instance);
        }

        #region IRepository implementation

        public override void Insert(T instance, ResponseHandler callback)
        {
            string error = null;
            try
            {
                if (IsIdNull(instance))
                {
                    AssignUniqueId(instance);
                }

                string keyPath = GetKeyFromInstance(instance);
                if (PlayerPrefs.HasKey(keyPath))
                {
                    throw new Exception("Attempting to insert an object that already exists. Key=" + keyPath);
                }

                string serializedObject = Serializer.Serialize<T>(instance, true);
                PlayerPrefs.SetString(keyPath, serializedObject);
                KeysIndex.Add(GetKey(instance));
                OnPlayerPrefsChanged();
            }
            catch(Exception e)
            {
                error = e.Message;
            }
            finally
            {
                if (callback != null)
                {
                    callback(new Response(instance, error));
                }
            }
        }

        public override void Update(T instance, ResponseHandler callback)
        {
            string error = null;
            try
            {
                string serializedObject = Serializer.Serialize<T>(instance, true);
                string keyPath = GetKeyFromInstance(instance);
                if (!PlayerPrefs.HasKey(keyPath))
                {
                    throw new Exception("Attempting to update an object that has not been inserted. Key=" + keyPath);
                }
                PlayerPrefs.SetString(keyPath, serializedObject);
                OnPlayerPrefsChanged();
            }
            catch(Exception e)
            {
                error = e.Message;
            }
            finally
            {
                if (callback != null)
                {
                    callback(new Response(instance, error));
                }
            }
        }

        public override void Delete(T instance, ResponseHandler callback)
        {
            string error = null;
            try
            {
                string keyPath = GetKeyFromInstance(instance);
                PlayerPrefs.DeleteKey(keyPath);
                KeysIndex.Remove(GetKey(instance));
                OnPlayerPrefsChanged();
            }
            catch(Exception e)
            {
                error = e.Message;
            }
            finally
            {
                if (callback != null)
                {
                    callback(new Response(error));
                }
            }
        }

        public override IQuery<T> Where(System.Linq.Expressions.Expression<System.Func<T, bool>> predicate)
        {
            throw new System.NotImplementedException ();
        }

        public override void Get(System.Func<T, bool> predicate, ResponseHandler callback)
        {
            string error = null;
            List<T> filteredItems = null;
            try
            {
                filteredItems = GetAll().Where(predicate).ToList();
                callback(new Response(filteredItems, null));
            }
            catch(Exception e)
            {
                error = e.Message;
            }
            finally
            {
                if (callback != null)
                {
                    callback(new Response(filteredItems, error));
                }
            }
        }

        public override void GetByKeys(object[] keys, ResponseHandler callback)
        {
            List<T> instances = new List<T>();
            string error = null;
            try
            {
                foreach (object key in keys)
                {
                    string keyString = GetKeyPath(key);
                    if (PlayerPrefs.HasKey(keyString))
                    {
                        string serializeObject = PlayerPrefs.GetString(keyString);
                        instances.Add(Serializer.Deserialize<T>(serializeObject));
                    }
                    else
                    {
                        throw new Exception(String.Format("Unable to find {0} with id = {1}", typeof(T).Name, key.ToString()));
                    }
                }
            }
            catch(Exception e)
            {
                error = e.Message;
            }
            finally
            {
                if (callback != null)
                {
                    callback(new Response(instances, error));
                }
            }
        }

        public override void GetByKey(object key, ResponseHandler callback)
        {
            GetByKeys(new object[] { key }, callback);
        }

        #endregion 

        protected virtual void OnPlayerPrefsChanged()
        {
            SaveIndex();
            PlayerPrefs.Save();
        }

        private static string GetKeyPath(object key)
        {
            if (key == null)
            {
                throw new Exception(string.Format("Key property for {0} is null",
                                                  typeof(T).Name));
            }
                       
            return string.Format("{0}/{1}", typeof(T).Name, key.ToString());
        }

        private string GetKeyFromInstance(T instance)
        {
            return GetKeyPath(GetKey(instance));
        }

        private List<T> GetAll()
        {
            List<T> items = new List<T>();

            foreach (String key in KeysIndex)
            {
                string keyPath = GetKeyPath(key);
                if (PlayerPrefs.HasKey(keyPath))
                {
                    string serializeObject = PlayerPrefs.GetString(keyPath);
                    items.Add(Serializer.Deserialize<T>(serializeObject));
                }
            }

            return items;
        }

        private List<object> LoadIndex()
        {
            if (PlayerPrefs.HasKey(KeysIndexName))
            {
                string serializeObject = PlayerPrefs.GetString(KeysIndexName);
                return Serializer.Deserialize<List<object>>(serializeObject);
            }
            else
            {
                return new List<object>();
            }
        }
        
        private void SaveIndex()
        {
            string serializedObject = Serializer.Serialize<List<object>>(KeysIndex, true);
            PlayerPrefs.SetString(KeysIndexName, serializedObject);
        }
    }
}
