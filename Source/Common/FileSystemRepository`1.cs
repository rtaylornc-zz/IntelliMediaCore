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
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace IntelliMedia
{
    public class FileSystemRepository<T> : Repository<T> where T : class, new()
    {
        public string DataDirectory { get; private set; }

        protected ISerializer Serializer { get; set; }

        public FileSystemRepository(string pathToDataDirectory, ISerializer serializer = null)
        {
            Contract.ArgumentNotNull("pathToDataDirectory", pathToDataDirectory);

            DataDirectory = Path.Combine(pathToDataDirectory, typeof(T).Name);

            // Default to XML serializer
            Serializer = (serializer != null ? serializer : SerializerXml.Instance);
            
            if (!Directory.Exists(DataDirectory))
            {
                Directory.CreateDirectory(DataDirectory);
            }
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

                string filename = GetFilenameFromInstance(instance);
                if (File.Exists(filename))
                {
                    throw new Exception("Attempting to insert an object which already exists. Filename=" + filename);
                }
 
                string serializedObject = Serializer.Serialize<T>(instance, true);
                using (StreamWriter stream = new StreamWriter(filename))
                {
                    stream.Write(serializedObject);
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
                using (StreamWriter stream = new StreamWriter(GetFilenameFromInstance(instance), false))
                {
                    stream.Write(serializedObject);
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
                    callback(new Response(instance, error));
                }
            }
        }

        public override void Delete(T instance, ResponseHandler callback)
        {
            string error = null;
            try
            {
                File.Delete(GetFilenameFromInstance(instance));
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
            GetAll((Response response) =>
            {
                if (!response.Success)
                {
                    throw new Exception(response.Error);
                }

                try
                {
                    List<T> filteredItems = response.Items.Where(predicate).ToList();
                    callback(new Response(filteredItems, null));
                }
                catch(Exception e)
                {
                    callback(new Response(e.Message));
                }
            });
        }

        public override void GetByKeys(object[] keys, ResponseHandler callback)
        {
            List<T> instances = new List<T>();
            string error = null;
            try
            {
                foreach (object key in keys)
                {
                    using(StreamReader reader = new StreamReader(GetFilenameFromKey(key)))
                    {
                        string serializeObject = reader.ReadToEnd();
                        instances.Add(Serializer.Deserialize<T>(serializeObject));
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

        private string GetFilenameFromKey(object key)
        {
            if (key == null)
            {
                throw new Exception(string.Format("Key property ({0}.{1}) is null",
                                                  typeof(T).Name,
                                                  KeyPropertyInfo.Name));
            }
             
            return Path.Combine(DataDirectory, string.Format("{0}.{1}", key.ToString(), Serializer.FilenameExtension));
        }

        private string GetFilenameFromInstance(T instance)
        {
            return GetFilenameFromKey(GetKey(instance));
        }

        protected virtual void GetAll(ResponseHandler callback)
        {
            string error = null;
            List<T> instances = new List<T>();
            try
            {
                foreach(string filename in Directory.GetFiles(DataDirectory))
                {
                    using(StreamReader reader = new StreamReader(filename))
                    {
                        string serializeObject = reader.ReadToEnd();
                        instances.Add(Serializer.Deserialize<T>(serializeObject));
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
    }
}
