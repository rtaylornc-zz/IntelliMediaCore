//---------------------------------------------------------------------------------------
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace IntelliMedia
{
    /// <summary>
    /// This component is used to act as the virtual eye for an agent to detect 
    /// nearby VisualTargets.
    /// </summary>
	public class VisualSensor : MonoBehaviour 
	{
        // This data model is used to keep track of detected visual objects
        // between frames.
        public class Contact
        {
            public VisualTarget Target { get; set;}
            public float Distance { get; set;}
            public float Angle { get; set;}

            /// <summary>
            /// Have the InterestLevel of the VisualTarget increased since the last frame?
            /// </summary>
            public bool InterestIncreased { get; private set; }

            private float previousInterest;
            public void CheckForInterestLevelChange()
            {
                InterestIncreased = Target.InterestLevel > previousInterest;
                previousInterest = Target.InterestLevel;
            }

            public float Duration { get { return (GameTime.time - StartContactTime); }}
            public float StartContactTime { get; set; }

            public int LastFrameCount { get; set; }
        }

        public readonly List<Contact> Contacts = new List<Contact>();

        /// <summary>
        /// The joint that should be targeted by other characters' HeadLookTargets.
        /// </summary>
        public Transform sensorMountPoint;

        /// <summary>
        /// The maximum angle at which the character to which this script is attached should track a
        /// target.
        /// </summary>
        public float maxLookAngle;
        
        /// <summary>
        /// The maximum distance at which the character to which this script is attached should track a
        /// target.
        /// </summary>
        public float maxLookDistance;

        private void LateUpdate()
        {
            foreach (VisualTarget possibleContact in VisualTarget.All)
            {
                // Ignore self
                if (possibleContact.gameObject == gameObject)
                {
                    continue;
                }

                float distance = CalculateDistanceToTarget(possibleContact);
                float angle = CalculateAngleToTarget(possibleContact);

                if (distance <= maxLookDistance && angle <= maxLookAngle)
                {
                    Contact contact = Contacts.FirstOrDefault(c => c.Target == possibleContact);
                    if (contact == null)
                    {
                        contact = new Contact()
                        {
                            Target = possibleContact,
                            StartContactTime = GameTime.time,
                        };
                        Contacts.Add(contact);
                    }

                    contact.Distance = distance;
                    contact.Angle = angle;
                    contact.CheckForInterestLevelChange();
                    contact.LastFrameCount = Time.frameCount;
                }
            }

            // Remove stale contacts (visual targets that were not updated during this frame)
            Contacts.RemoveAll(c => c.LastFrameCount < Time.frameCount);
        }

        private float CalculateDistanceToTarget(VisualTarget target)
        {
            return (target != null ? Vector3.Distance(transform.position, target.transform.position) : 0);
        }

        private float CalculateAngleToTarget(VisualTarget target)
        {
            return (target != null ? Vector3.Angle(target.transform.position - transform.position, transform.forward) : 0);
        }
	}
}
