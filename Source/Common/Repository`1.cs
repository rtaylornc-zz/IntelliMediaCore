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
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;

namespace IntelliMedia
{
    public abstract class Repository<T> where T : class, new()
    {
        public class Response
        {
            public bool Success { get { return Error == null; }}
            public string Error { get; private set; }
            public T Item { get; set; }
            public List<T> Items { get; set; }

            public Response(string error)
            {
                Error = error;
            } 
           
            public Response(T item, string error)
            {
                Item = item;
                Error = error;
            }           

            public Response(List<T> items, string error)
            {
                Items = items;
                if (Items != null && Items.Count == 1)
                {
                    Item = Items[0];
                }
                Error = error;
            }
        }
        public delegate void ResponseHandler(Response response);

        protected RepositoryKey RepositoryKey { get; private set; }
        protected PropertyInfo KeyPropertyInfo { get; private set; }

        public Repository()
        {                       
            FindKeyPropertyInDataType();            
        }

        public abstract void Insert(T instance, ResponseHandler callback);
        public abstract void Update(T instance, ResponseHandler callback);
        public abstract void Delete(T instance, ResponseHandler callback);
        public abstract IQuery<T> Where(Expression<Func<T, Boolean>> predicate);
        public abstract void Get(System.Func<T, bool> predicate, ResponseHandler callback);
        public abstract void GetByKeys(object[] keys, ResponseHandler callback);
        public abstract void GetByKey(object key, ResponseHandler callback);

        protected object GetKey(T instance)
        {
            Contract.ArgumentNotNull("instance", instance);
            Contract.PropertyNotNull("KeyPropertyInfo", KeyPropertyInfo);

            return KeyPropertyInfo.GetValue(instance, null);
        }

        protected void AssignUniqueId(T instance)
        {
            Contract.ArgumentNotNull("instance", instance);
            Contract.PropertyNotNull("KeyPropertyInfo", KeyPropertyInfo);
            
            KeyPropertyInfo.SetValue(instance, Guid.NewGuid().ToString(), null);
        }

        protected bool IsIdNull(T instance)
        {
            return (GetKey(instance) == null);
        }

        /// <summary>
        /// Scan the properties in the data type to find one and only one decorated with the RepositoryKey
        /// attribute.
        /// </summary>
        private void FindKeyPropertyInDataType()
        {
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties ()) 
            {
                object[] attributes = propertyInfo.GetCustomAttributes (typeof(RepositoryKey), false);
                if (attributes != null && attributes.Length > 0) 
                {
                    if (attributes.Length > 1) 
                    {
                        throw new Exception (string.Format ("Multiple {0} attributes assigned to propery: {1}.{2}", 
                                                            typeof(RepositoryKey).Name, 
                                                            typeof(T).Name, 
                                                            propertyInfo.Name));
                    }
                    if (KeyPropertyInfo == null) 
                    {
                        RepositoryKey = attributes [0] as RepositoryKey;
                        KeyPropertyInfo = propertyInfo;
                    }
                    else 
                    {
                        throw new Exception (string.Format ("More than one property has {0} attribute in {1} class", 
                                                            typeof(RepositoryKey).Name, 
                                                            typeof(T).Name));
                    }
                }
            }
            if (KeyPropertyInfo == null) 
            {
                throw new Exception (string.Format ("Exactly one property must have {0} attribute in {1} class", 
                                                    typeof(RepositoryKey).Name, 
                                                    typeof(T).Name));
            }
        }
    }
}
