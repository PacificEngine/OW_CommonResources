using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PacificEngine.OW_CommonResources.Geometry
{
    public static class Orbit
    {
        public static Tuple<Vector3, Vector3> getPositionAndVelocity(Vector3 aphelion, Vector3 perihelon, float degreesFromPerihelon, float gravityConstant, float mass)
        {
            // Use negative degrees to dictate counter rotation
            if (aphelion.sqrMagnitude < perihelon.sqrMagnitude)
                return getPositionAndVelocity(perihelon, aphelion, (degreesFromPerihelon > 0) ? (degreesFromPerihelon + 180f) : (degreesFromPerihelon - 180f), gravityConstant, mass);
            var amag = aphelion.magnitude;
            var pmag = perihelon.magnitude;
            var focus = (amag - pmag) / 2f;
            var majorRadius = (amag + pmag) / 2f;
            var minorRadius = Ellipse.getMinorRadius(majorRadius, focus);
            var radius = new Vector2(majorRadius, minorRadius);
            var currentRadius = Ellipse.getDistanceFromCenter(degreesFromPerihelon, radius);
            var position = Ellipse.fromPolar(degreesFromPerihelon, radius) - (focus * Vector2.right);
            var slope = Ellipse.fromPolarToSlope(degreesFromPerihelon, radius);
            var velocity = (float)Math.Sqrt(2f * gravityConstant * mass * ((1f / currentRadius) - (1f / (radius.x + radius.y))));

            var angle = Coordinates.angleXYZ(aphelion - perihelon, Vector3.right);
            return Tuple.Create(Coordinates.rotatePoint(position, angle), velocity * Coordinates.rotatePoint(slope, angle).normalized);
        }
    }
}
