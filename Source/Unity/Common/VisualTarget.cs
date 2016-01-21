//---------------------------------------------------------------------------------------
using UnityEngine;
using System.Collections.Generic;

namespace IntelliMedia
{
	public class VisualTarget : MonoBehaviour 
	{
        public readonly static List<VisualTarget> All = new List<VisualTarget>();

        /// <summary>
        /// Value between 0 and 1 where 1 is the most interesting and zero is
        /// not interesting at all.
        /// </summary>
        public float DefaultInterestLevel = 0.25f;

        /// <summary>
        /// The maximum interest that this object can be. For example, a simple prop
        /// may never be very interesting.
        /// </summary>
        public float MaxInterestLevel = 1f;

        /// <summary>
        /// The minimum interest level for this object. Some things, like a important prop
        /// will always be interesting.
        /// </summary>
        public float MinInterestLevel = 0f;

        /// <summary>
        /// The current interest level, which may be temporarily elevated and depressed
        /// depending on the state of the prop, NPC, player, etc.
        /// </summary>
        public float InterestLevel { get; private set; }

        /// <summary>
        /// The joint that should be targeted by other characters' HeadLookTargets.
        /// </summary>
        [SerializeField]
        private Transform pointOfInterest;
        public Transform PointOfInterest 
        { 
            get 
            { 
                return (pointOfInterest != null ? pointOfInterest.transform : transform); 
            }
        }

        public void SetMaxInterestLevel()
        {
            InterestLevel = MaxInterestLevel;
        }

        public void SetDefaultInterestLevel()
        {
            InterestLevel = DefaultInterestLevel;
        }

        public void SetMinimumInterestLevel()
        {
            InterestLevel = MinInterestLevel;
        }

        private void Start()
        {
            InterestLevel = DefaultInterestLevel;
        }

        private void OnEnable()
        {
            All.Add(this);
        }
        
        private void OnDisable()
        {
            All.Remove(this);
        }
	}
}
