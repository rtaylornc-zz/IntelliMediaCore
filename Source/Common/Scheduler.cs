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
using System.Collections.Generic;
using System.Diagnostics;

namespace IntelliMedia
{
    // Provides a task scheduler that ensures a maximum concurrency level while  
    // running on top of the thread pool.
    public class Scheduler : ISchedulable
    {
        static readonly Scheduler global = new Scheduler();
        public static Scheduler Global
        {
            get
            {
                return global;
            }
        }

        readonly List<ISchedulable> _tasks = new List<ISchedulable>(); // protected by lock(_tasks) 
        LoopingEnumerator<ISchedulable> runningTasks;
        LoopingEnumerator<ISchedulable> nonWaitingTasks;

        Stopwatch stopWatch = new Stopwatch();

        public Scheduler()
        {
            runningTasks = new LoopingEnumerator<ISchedulable>(
                _tasks, (
                ISchedulable s) =>
                {
                    return (s != null && s.IsRunning);
                });

            nonWaitingTasks = new LoopingEnumerator<ISchedulable>(
                _tasks, (
                ISchedulable s) =>
                {
                    return (s != null && s.IsRunning && !s.IsWaiting);
                });
        }

        // Queues a task to the scheduler.  
        internal void Enqueue(ISchedulable task)
        {
            if (!_tasks.Contains(task))
            {
                lock (_tasks)
                {
                    _tasks.Add(task);
                }
            }
        }

        internal void Dequeue(ISchedulable task)
        {
            if (_tasks.Contains(task))
            {
                lock (_tasks)
                {
                    _tasks.Remove(task);
                }
            }
        }

        public IEnumerable<ISchedulable> GetAllTasks()
        {
            foreach (ISchedulable task in _tasks)
            {
                yield return task;
            }

            yield break;
        }

        public bool AnyNonWaitingTasks 
        {
            get 
            {
                foreach (ISchedulable task in _tasks)
                {
                    if (task.IsRunning && !task.IsWaiting)
                    {
                        return true;
                    }
                }
                
                return false;
            }
        }
        
        #region ISchedulable implementation

        public void Start ()
        {
            //throw new NotImplementedException ();
        }

        /// <summary>
        /// Process cooperative tasks until the timeout is reached or all tasks are complete.
        /// </summary>
        /// <param name="timeout">Milliseconds.</param>
        public void Execute(Double timeout)
        {
            if (_tasks.Count == 0)
            {
                return;
            }
                        
            double availableExecutionTime = timeout;

            stopWatch.Reset();
            stopWatch.Start();

            // Execute all tasks (running and waiting) once
            runningTasks.Reset();
            while (availableExecutionTime >= 0 && runningTasks.MoveNext())
            {
                runningTasks.Current.Execute(availableExecutionTime);
                availableExecutionTime = timeout - stopWatch.Elapsed.TotalMilliseconds;
            }

            // Execute tasks that are not waiting with any remaining time
            while (availableExecutionTime > 0 && AnyNonWaitingTasks)
            {
                nonWaitingTasks.Reset();
                while (nonWaitingTasks.MoveNext())
                {
                    nonWaitingTasks.Current.Execute(availableExecutionTime);

                    availableExecutionTime = timeout - stopWatch.Elapsed.TotalMilliseconds;
                    if (availableExecutionTime < 0)
                    {
                        break;
                    }
                }
            }
        }

        public virtual void Cancel()
        {
            foreach (ISchedulable task in _tasks)
            {
                task.Cancel();
            }
        }

        public bool IsRunning 
        {
            get 
            {
                foreach (ISchedulable task in _tasks)
                {
                    if (task.IsRunning)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool IsWaiting 
        {
            get 
            {
                foreach (ISchedulable task in _tasks)
                {
                    if (task.IsRunning && !task.IsWaiting)
                    {
                        return false;
                    }
                }
                
                return true;
            }
        }

        public bool IsFinished { get { return !IsRunning; } }

        #endregion
    }
}
