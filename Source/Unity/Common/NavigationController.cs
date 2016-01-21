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
using System.Collections;
using IntelliMedia;
using IntelliMedia.DecisionMaking;

namespace IntelliMedia
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavigationController : MonoBehaviour, INavigationController
    {
    	protected NavMeshAgent		navMeshAgent;
    	protected Animator			animator;
    	
    	protected Locomotion locomotion;

    	public Vector3 Destination
    	{
    		get
    		{
    			return navMeshAgent.destination;
    		}
    	}
    	
    	// Use this for initialization
    	void Start () 
        {
    		navMeshAgent = GetComponent<NavMeshAgent>();
    		navMeshAgent.updateRotation = false;
    		
    		animator = GetComponent<Animator>();
    		locomotion = new Locomotion(animator);
    	}
    	
    	public void SetDestination(Vector3 destination)
    	{
            navMeshAgent.updatePosition = true;
    		navMeshAgent.destination = destination;
    	}

        float toleranceDegrees;
        public void TurnToward(Vector3 position, float toleranceAngle = 5f)
        {
            // Setting updatePosition to false indicates we are turning only.
            navMeshAgent.updatePosition = false;
            toleranceDegrees = toleranceAngle;
            navMeshAgent.destination = position;
        }

    	protected void SetupAgentLocomotion()
    	{
    		if (IsDone)
    		{
    			locomotion.Do(0, 0);
    		}
            else
            {
                float speed = navMeshAgent.desiredVelocity.magnitude;
    			
    			Vector3 velocity = Quaternion.Inverse(transform.rotation) * navMeshAgent.desiredVelocity;
    			
    			float angle = Mathf.Atan2(velocity.x, velocity.z) * 180.0f / 3.14159f;
    			
    			locomotion.Do(speed, angle);    
            }
    	}
    	
    	void OnAnimatorMove()
    	{
    		navMeshAgent.velocity = animator.deltaPosition / Time.deltaTime;
    		transform.rotation = animator.rootRotation;
    	}
    	
    	protected bool AgentStopping()
    	{
            if (navMeshAgent.updatePosition)
            {
                // Arrived at destination?
    		    return navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance;
            }
            else
            {
                // Done turning?
                Vector3 heading = navMeshAgent.destination - transform.position;
                float angle = Vector3.Angle(transform.forward, heading);

                return angle < toleranceDegrees;
            }
    	}

    	void Update () 
    	{
    		SetupAgentLocomotion();
    	}

    	void OnDrawGizmos() 
    	{
    		if (navMeshAgent != null && !IsDone)
    		{
    			Gizmos.color = Color.yellow;
    			Gizmos.DrawSphere(navMeshAgent.destination, 0.125f);
    		}
    	}

        #region INavigationController implementation

        string destinationName;
        public string DestinationName
        {
            get
            {
                return destinationName;
            }

            set
            {
                if (value != destinationName)
                {
                    destinationName = value;

                    if (string.IsNullOrEmpty(destinationName))
                    {
                        DebugLog.Warning("{0} NavigationController destination name is empty or null", name);
                        return;
                    }

                    Transform destinationTransform = GameObject.Find(destinationName).transform;
                    if (destinationTransform != null)
                    {
                        SetDestination(destinationTransform.position);
                    }
                    else
                    {
                        DebugLog.Error("{0} NavigationController unable to find destinatation object: '{1}'", name, destinationName);
                    }
                }
            }
        }

        public bool IsDone 
        {
            get 
            {
                return (navMeshAgent != null && !navMeshAgent.pathPending && AgentStopping());
            }
        }

        public void Stop()
        {
            navMeshAgent.Stop();
        }

        public void Resume()
        {
            navMeshAgent.Stop();
        }

        #endregion
    }
}
