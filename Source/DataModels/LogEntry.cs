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
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.ComponentModel;

namespace IntelliMedia
{
    public class LogEntry : IDictionary<string, object>
    {
        public DateTime TimeStamp { get; set; }
        public string Action { get; set; }
        public int SequenceNumber { get; set; }

        [CsvMapToColumns("Context")]
        public Dictionary<string, object> Context { get; set; }
               
        public const int MaxAttributes = 125;


        public LogEntry()
        {
            Context = new Dictionary<string, object>();
            Action = null;
            TimeStamp = DateTime.Now;
            SequenceNumber = 0;
        }

        public LogEntry Clone()
        {
            LogEntry clone = new LogEntry()
            {
                Action = this.Action,
                TimeStamp = this.TimeStamp,
                SequenceNumber = this.SequenceNumber
            };

            foreach (KeyValuePair<string, object> attribue in Context)
            {
                clone.Add(attribue);
            }

            return clone;
        }

        public LogEntry(string sessionId, int sequenceNumber, string message, object[] Context) : this()
        {
            this.SequenceNumber = sequenceNumber;
            this.Action = message;
            Add(Context);
        }

        public string GetName()
        {
            object actor = null;
            TryGetValue("Actor", out actor);

            object target = null;
            TryGetValue("Target", out target);

            return string.Format("{0}-{1}-{2}",
                (actor != null ? actor : "[null]"),
                (Action != null ? Action : "[null]"),
                (target != null ? target : "[null]"));              
        }
            
		public bool ActionIs(params string[] actions)
		{
			if (Action != null && actions != null)
			{
				foreach (String action in actions)
				{
					if (Action.CompareTo(action) == 0)
					{
						return true;
					}
				}
		    }

		    return false;
		}
            
        public bool ContextEquals(string key, object targetValue)
        {
            return ContainsKey(key) && object.Equals(this[key], targetValue);
        }

        public T ContextValueAs<T>(string name)
        {
            object value = this[name];
            if (value is T)
            {
                return (T)value;
            }

            return (T)System.Convert.ChangeType(this[name], typeof(T));

        }
            
        public void Add(IEnumerable<object> attributes)
        {
            if (attributes == null)
            {
                return;
            }

            using(IEnumerator<object> iterator = attributes.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    if (iterator.Current is IEnumerable<object>)
                    {
                        Add((IEnumerable<object>)iterator.Current);
                    }
                    else if (iterator.Current is IEnumerable<string>)
                    {
                        Add(((IEnumerable<string>)iterator.Current).Cast<object>());
                    }
					else if (iterator.Current is Dictionary<string, object>)
					{
						Dictionary<string, object> ContextMap = (Dictionary<string, object>)iterator.Current;
						foreach(KeyValuePair<string, object> keyValue in ContextMap)
						{
							Add(keyValue.Key, keyValue.Value);
						}
					}
                    else
                    {
                        string key = iterator.Current.ToString();
                        if (!iterator.MoveNext())
                        {
                            throw new Exception(String.Format("Unable to log '{0}' message. List of Context attributes must contain a name and value (the collection must contain an even number of elements).", Action));
                        }

                        object value = iterator.Current;
                        Context.Add(key, value);
                    }
                }
            }

            if (Context.Count > MaxAttributes)
            {
                throw new Exception(String.Format("LogEntry {0} attributes exceeded {1} maximum allowed for Context.", Context.Count, MaxAttributes));
            }
        }

        public void Add(Dictionary<string, string> ContextMap)
        {
            foreach(KeyValuePair<string, string> keyValue in ContextMap)
            {
                Add(keyValue.Key, keyValue.Value);
            }            
        }



        // IDictionary Implementation
        [XmlIgnore]
        [CsvIgnore]
        public object this[string index]
        {
            get
            {
                if (String.Compare(index, "TimeStamp") == 0)
                {
                    return TimeStamp;
                }
                else if (String.Compare(index, "Action") == 0)
                {
                    return Action;
                }
                else
                {                    
                    return Context[index];
                }
            }
            set
            {
                Context[index] = value;
            }
        }

        public void Add(string key, object value)
        {
            Context.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return String.Compare(key, "TimeStamp") == 0
                || String.Compare(key, "Action") == 0
                || Context.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return Context.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return Context.TryGetValue(key, out value);
        }

        public ICollection<string> Keys
        {
            get
            {
                return Context.Keys;
            }
        }

        public ICollection<object> Values
        {
            get
            {
                return Context.Values;
            }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Context.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            Context.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return Context.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return Context.Remove(item.Key);
        }

        public int Count
        {
            get
            {
                return Context.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return Context.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Context.GetEnumerator();
        }
    }
}