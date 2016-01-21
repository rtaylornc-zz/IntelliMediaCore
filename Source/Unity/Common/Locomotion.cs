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

namespace IntelliMedia
{
    public class Locomotion
    {
        private Animator m_Animator = null;
        
        private int m_SpeedId = 0;
        private int m_AngularSpeedId = 0;
        private int m_DirectionId = 0;

        public float m_SpeedDampTime = 0.1f;
        public float m_AnguarSpeedDampTime = 0.25f;
        public float m_DirectionResponseTime = 0.2f;
        
        public Locomotion(Animator animator)
        {
            m_Animator = animator;

            m_SpeedId = Animator.StringToHash("Speed");
            m_AngularSpeedId = Animator.StringToHash("AngularSpeed");
            m_DirectionId = Animator.StringToHash("Direction");
        }

        public void Do(float speed, float direction)
        {
    		if (speed > 0)
    		{
    			//Debug.Log(string.Format("Locomation speed={0} direction={1}", speed, direction));
    		}

            AnimatorStateInfo state = m_Animator.GetCurrentAnimatorStateInfo(0);

            bool inTransition = m_Animator.IsInTransition(0);
            bool inIdle = state.IsName("Locomotion.Idle");
            bool inTurn = state.IsName("Locomotion.TurnOnSpot") || state.IsName("Locomotion.PlantNTurnLeft") || state.IsName("Locomotion.PlantNTurnRight");
            bool inWalkRun = state.IsName("Locomotion.WalkRun");

            float speedDampTime = inIdle ? 0 : m_SpeedDampTime;
            float angularSpeedDampTime = inWalkRun || inTransition ? m_AnguarSpeedDampTime : 0;
            float directionDampTime = inTurn || inTransition ? 1000000 : 0;

            float angularSpeed = direction / m_DirectionResponseTime;
            
            m_Animator.SetFloat(m_SpeedId, speed, speedDampTime, Time.deltaTime);
            m_Animator.SetFloat(m_AngularSpeedId, angularSpeed, angularSpeedDampTime, Time.deltaTime);
            m_Animator.SetFloat(m_DirectionId, direction, directionDampTime, Time.deltaTime);
        }	
    }
}
