//---------------------------------------------------------------------------------------
using UnityEngine;
using System.Collections.Generic;

namespace IntelliMedia
{
    /// <summary>
    /// This class coordinates the Unity HeadLookController and Animator components.
    /// </summary>
    /// <description>
    /// This class animates the 'effect' of the HeadLookController and sets Mecanim state
    /// machine parameters to control an animated model's gaze behavior. We deliberately 
    /// did not want to modify or subclass the HeadLookController class since it is 
    /// provided by Unity and was already applied to lots of existing GameObjects and
    /// Prefabs.
    /// </description>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(HeadLookController))]
	public class HeadLookAnimator  : MonoBehaviour
	{
        /// <summary>
        /// If the InterestLevel of the object being observed is above this value,
        /// the actor will stop what it is doing and idle while observing.
        /// </summary>
        public float idleInterestLevel = 0.5f;

        /// <summary>
        /// The time in seconds it takes to look at the target.
        /// </summary>
        public float lookAtDuration = 1f;

        /// <summary>
        /// The time in seconds it takes to look away from the target and resume 
        /// original animation.
        /// </summary>
        public float lookAwayDuration = 1f;

        private HeadLookController controller;
        private Animator animator;

        private VisualTarget target;
        public VisualTarget Target
        {
            get { return target; }
            set 
            { 
                if (value != target) 
                { 
                    VisualTarget old = target; 
                    target = value; 
                    OnCurrentTargetChanged(old, target); 
                }
            }
        }

        private void OnCurrentTargetChanged(VisualTarget oldTarget, VisualTarget newTarget)
        {
            if (oldTarget == null && newTarget != null)
            {
                StartLooking();
            }
            else if (newTarget == null && oldTarget != null)
            {
                StopLooking();
            }
        }

        private string itweenAnimationName;

        private void Start()        
        {
            this.controller = GetComponent<HeadLookController>();           
            this.animator = GetComponent<Animator>();

            itweenAnimationName = this.gameObject.name + "HeadLookAnimation";
            iTween.Init(this.gameObject);
        }

        private void Update()
        {
            if (Target != null)
            {
                controller.target = Target.PointOfInterest.position;
            }
        }

        private void StartLooking()
        {
            iTween.StopByName(this.gameObject, itweenAnimationName, false);

            animator.SetBool("IdleLookAt", Target.InterestLevel >= idleInterestLevel);

            iTween.ValueTo(this.gameObject, iTween.Hash(
                "name", itweenAnimationName,
                "from", 0.0f,
                "to", 1f,
                "time", lookAtDuration,
                "easetype", iTween.EaseType.easeOutQuad,
                "onupdate", (System.Action<object>)(val => controller.effect = (float)val ),
                "oncomplete", "StartLookingComplete"));
        }

        private void StartLookingComplete()
        {
        }

        private void StopLooking()
        {
            iTween.StopByName(this.gameObject, itweenAnimationName, false);
            iTween.ValueTo(this.gameObject, iTween.Hash(
                "name", itweenAnimationName,
                "from", 1.0f,
                "to", 0.0f,
                "time", lookAwayDuration,
                "easetype", iTween.EaseType.easeOutQuad,
                "onupdate", (System.Action<object>)(val => controller.effect = (float)val),
                "oncomplete", "StopLookingComplete"));
        }

        private void StopLookingComplete()
        {
            animator.SetBool("IdleLookAt", false);
        }
	}
}
