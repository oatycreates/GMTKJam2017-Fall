using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lib
{
    /// <summary>
    /// Contains methods for handling direction-based calculations.
    /// </summary>
    public class DirectionHelpers
    {
        /// <summary>
        /// Quantised direction, represents a direction angle reduced to a series of direction cones.
        /// </summary>
        public enum Direction
        {
            NONE,
            UP,
            DOWN,
            LEFT,
            RIGHT
        }

        /// <summary>
        /// Returns the quantised direction for the input contact points.
        /// </summary>
        /// <param name="myPos">Position of the object.</param>
        /// <param name="contactPoints">Contact points the object is colliding against.</param>
        /// <returns>Quantised direction or NONE.</returns>
        public static Direction GetCollisionDirection(Vector2 myPos, ContactPoint2D[] contactPoints)
        {
            // Work out where the contact point is relative to the object
            Vector2 collisionPoint = AverageCollisionPoint(contactPoints);
            return GetQuantisedDirection(myPos, collisionPoint);
        }

        /// <summary>
        /// Returns the quantised direction for the input reference points from an object to a point of interest.
        /// </summary>
        /// <param name="myPos">Position of the object.</param>
        /// <param name="otherPointPos">Position of the point of interest.</param>
        /// <returns>Quantised direction or NONE.</returns>
        public static Direction GetQuantisedDirection(Vector2 myPos, Vector2 otherPointPos)
        {
            Direction outDirection = Direction.NONE;

            Vector2 collisionDirRaw = (otherPointPos - myPos).normalized;

            // If the collision offset is in a 45 degree cardinal cone, lump it into that direction
            float collisionDotUp = Vector2.Dot(collisionDirRaw, Vector2.up);
            float collisionDotRight = Vector2.Dot(collisionDirRaw, Vector2.right);
            // Dot values greater than this cone align, values less than this don't align with the cone
            float alignmentCone = Mathf.Sin(45 * Mathf.Deg2Rad);

            // Compare the collision dot products with the alignment cones to determine which direction the collision is in
            if (collisionDotUp >= alignmentCone)
            {
                outDirection = Direction.UP;
            }
            else if (-collisionDotUp >= alignmentCone)
            {
                outDirection = Direction.DOWN;
            }
            else if (collisionDotUp <= alignmentCone &&
                collisionDotRight >= alignmentCone)
            {
                outDirection = Direction.RIGHT;
            }
            else if (collisionDotUp <= alignmentCone &&
                -collisionDotRight >= alignmentCone)
            {
                outDirection = Direction.LEFT;
            }

            return outDirection;
        }

        /// <summary>
        /// Returns the average collision point.
        /// </summary>
        /// <param name="contactPoints">Contact points to average the position of.</param>
        /// <returns>Average collision point.</returns>
        public static Vector2 AverageCollisionPoint(ContactPoint2D[] contactPoints)
        {
            return new Vector2(
                contactPoints.Average(contact => contact.point.x),
                contactPoints.Average(contact => contact.point.y)
            );
        }

        /// <summary>
        /// Returns the rotation angle in radians for the input normalised direction vector.
        /// </summary>
        /// <param name="normalizedDirection">Normalised direction vector.</param>
        /// <returns>Rotation angle in radians.</returns>
        public static float RotationAngleForVector(Vector2 normalizedDirection)
        {
            return Mathf.Atan2(normalizedDirection.y, normalizedDirection.x) - (90.0f * Mathf.Deg2Rad);
        }
    }
}
