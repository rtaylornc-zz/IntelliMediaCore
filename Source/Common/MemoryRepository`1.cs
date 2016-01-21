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
using System.Reflection;
using System.Collections.Generic;

namespace IntelliMedia
{
    public class MemoryRepository<T> : Repository<T> where T : class, new()
    {
        private Dictionary<object, T> repository = new Dictionary<object, T>();

        public MemoryRepository()
        {
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

                object id = GetKey(instance);
                if (repository.ContainsKey(id))
                {
                    throw new Exception("Attempting to insert an object which already exists. id =" + id.ToString());
                }
                repository[id] = instance;
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
                object id = GetKey(instance);
                if (repository.ContainsKey(id))
                {
                    repository[id] = instance;
                }
                else
                {
                    throw new Exception("Unable to updated object that has not been inserted into the repository. id = " + id.ToString());
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
                object id = GetKey(instance);
                repository.Remove(id);
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
                filteredItems = repository.Values.Where(predicate).ToList();
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
                    if (repository.ContainsKey(key))
                    {
                        instances.Add(repository[key]);
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
    }
}
