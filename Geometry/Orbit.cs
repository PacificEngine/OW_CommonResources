using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PacificEngine.OW_CommonResources.Geometry
{
    //https://space.stackexchange.com/questions/19322/converting-orbital-elements-to-cartesian-state-vectors
    public static class Orbit
    {
        public class KeplerCoordinates
        {
            public static KeplerCoordinates zero { get; } = new KeplerCoordinates(0f, 0f, 0f, 0f, 0f, 0f);

            public float eccentricity { get; }
            public float semiMajorRadius { get; }
            public float inclinationAngle { get; }
            public float periapseAngle { get; }
            public float ascendingAngle { get; }
            public float timeSincePeriapse { get; }

            public KeplerCoordinates(float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float timeSincePeriapse)
            {
                this.eccentricity = eccentricity;
                this.semiMajorRadius = semiMajorRadius;
                this.inclinationAngle = inclinationAngle;
                this.periapseAngle = periapseAngle;
                this.ascendingAngle = ascendingAngle;
                this.timeSincePeriapse = timeSincePeriapse;
            }

            public override string ToString()
            {
                return $"({Math.Round(eccentricity, 4).ToString("G9")}, {Math.Round(semiMajorRadius, 7).ToString("G9")}, {Math.Round(inclinationAngle, 4).ToString("G9")}, {Math.Round(periapseAngle, 4).ToString("G9")}, {Math.Round(ascendingAngle, 4).ToString("G9")}, {Math.Round(timeSincePeriapse, 5).ToString("G9")})";
            }

            public override bool Equals(System.Object other)
            {
                if (other != null && other is KeplerCoordinates)
                {
                    var obj = other as KeplerCoordinates;
                    return eccentricity == obj.eccentricity
                        && semiMajorRadius == obj.semiMajorRadius
                        && inclinationAngle == obj.inclinationAngle
                        && periapseAngle == obj.periapseAngle
                        && ascendingAngle == obj.ascendingAngle
                        && timeSincePeriapse == obj.timeSincePeriapse;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return (eccentricity.GetHashCode() * 4)
                    + (semiMajorRadius.GetHashCode() * 16)
                    + (inclinationAngle.GetHashCode() * 64)
                    + (periapseAngle.GetHashCode() * 256)
                    + (ascendingAngle.GetHashCode() * 1024)
                    + (timeSincePeriapse.GetHashCode() * 16384);
            }

            public bool isOrbit()
            {
                return
                    !float.IsNaN(eccentricity) && !float.IsInfinity(eccentricity) && 0f <= eccentricity && eccentricity <= 1f
                        && !float.IsNaN(semiMajorRadius) && !float.IsInfinity(semiMajorRadius)
                        && !float.IsNaN(inclinationAngle) && !float.IsInfinity(inclinationAngle) && 0f <= inclinationAngle && inclinationAngle <= 180f
                        && !float.IsNaN(periapseAngle) && !float.IsInfinity(periapseAngle) && 0f <= periapseAngle && periapseAngle <= 360f
                        && !float.IsNaN(ascendingAngle) && !float.IsInfinity(ascendingAngle) && 0f <= ascendingAngle && ascendingAngle <= 360f
                        && !float.IsNaN(timeSincePeriapse) && !float.IsInfinity(timeSincePeriapse);
            }
        }

        private const double twoPi = (2d * Math.PI);

        public static double normalizeRadian(double radians)
        {
            return (((radians % twoPi) + twoPi) % twoPi);
        }

        //https://web.archive.org/web/20160418175843/https://ccar.colorado.edu/asen5070/handouts/cart2kep2002.pdf
        public static KeplerCoordinates toKeplerCoordinates(float gravityConstant, float mass, float radiusExponent, float timeSinceStart, Vector3 startPosition, Vector3 startVelocity)
        {
            var product = (double)startPosition.x * startVelocity.x + startPosition.y * startVelocity.y + startPosition.z * startVelocity.z;
            var angularMomemntumVector = new Vector3(startPosition.y * startVelocity.z - startPosition.z * startVelocity.y, startPosition.z * startVelocity.x - startPosition.x * startVelocity.z, startPosition.x * startVelocity.y - startPosition.y * startVelocity.x);
            var angularMomemntum = (double)angularMomemntumVector.magnitude;

            var mu = Math.Abs((double)gravityConstant * mass);
            var radius = (double)startPosition.magnitude;
            var speed = (double)startVelocity.magnitude;
            double specificEnergy;
            double semiMajorRadius;
            double eccentricity;
            if (radiusExponent < 1.5d)
            {
                specificEnergy = (speed * speed) / 2d - mu;
                eccentricity = Math.Abs(Math.Sqrt(((2d * specificEnergy) / mu) + 2d) - 1d);
                semiMajorRadius = angularMomemntum / (Math.Sqrt(mu) * (1d - (eccentricity * eccentricity)));
            }
            else
            {
                specificEnergy = (speed * speed) / 2d - (mu / radius);
                semiMajorRadius = Math.Abs(mu / (2d * specificEnergy));
                eccentricity = Math.Sqrt(Math.Abs(1 - ((angularMomemntum * angularMomemntum) / (semiMajorRadius * mu))));
            }
            var inclinationAngle = normalizeRadian(Math.Acos(angularMomemntumVector.z/angularMomemntum)) % Math.PI;
            var ascendingAngle = normalizeRadian(Math.Atan2(angularMomemntumVector.x, -1f * angularMomemntumVector.y));
            var latitudeAngle = normalizeRadian(Math.Atan2(startPosition.z / Math.Sin(inclinationAngle), (startPosition.x * Math.Cos(ascendingAngle)) + (startPosition.y * Math.Sin(ascendingAngle))));

            var semiAxisRectum = (semiMajorRadius * (1d - eccentricity * eccentricity));
            var trueAnomalyAngle = normalizeRadian(Math.Atan2(Math.Sqrt(semiAxisRectum / mu) * product, semiAxisRectum - radius));
            var periapseAngle = normalizeRadian(latitudeAngle - trueAnomalyAngle);
            var essentricAnomaly = normalizeRadian(2d * Math.Atan(Math.Sqrt(Math.Abs((1d - eccentricity)/(1d + eccentricity))) * Math.Tan(trueAnomalyAngle/2d)));
            double period;
            if (radiusExponent < 1.5d)
            {
                period = twoPi / Math.Sqrt(Math.Abs(mu / (semiMajorRadius * semiMajorRadius)));
            }
            else
            {
                period = twoPi / Math.Sqrt(Math.Abs(mu / (semiMajorRadius * semiMajorRadius * semiMajorRadius)));
            }
            var n = twoPi / period;
            var meanAnomaly = normalizeRadian(essentricAnomaly - eccentricity * Math.Sin(essentricAnomaly));
            var timeSincePeriapse = meanAnomaly / n;
            timeSincePeriapse = (-1d * timeSinceStart) + timeSincePeriapse;
            meanAnomaly = normalizeRadian(n * timeSincePeriapse);
            timeSincePeriapse = meanAnomaly / n;

            return new KeplerCoordinates((float)eccentricity, (float)semiMajorRadius, Angle.toDegrees((float)inclinationAngle), Angle.toDegrees((float)periapseAngle), Angle.toDegrees((float)ascendingAngle), (float)timeSincePeriapse);
        }

        public static Tuple<Vector3, Vector3> toCartesian(float gravityConstant, float mass, float radiusExponent, float timeSinceStart, KeplerCoordinates keplerCoordinates)
        {
            return toCartesian(gravityConstant, mass, radiusExponent, timeSinceStart, keplerCoordinates.eccentricity, keplerCoordinates.semiMajorRadius, keplerCoordinates.inclinationAngle, keplerCoordinates.periapseAngle, keplerCoordinates.ascendingAngle, keplerCoordinates.timeSincePeriapse); 
        }

        //https://web.archive.org/web/20170810015111/http://ccar.colorado.edu/asen5070/handouts/kep2cart_2002.doc
        public static Tuple<Vector3, Vector3> toCartesian(float gravityConstant, float mass, float radiusExponent, float timeSinceStart, float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float timeSincePeriapse)
        {
            inclinationAngle = Angle.toRadian(inclinationAngle);
            periapseAngle = Angle.toRadian(periapseAngle);
            ascendingAngle = Angle.toRadian(ascendingAngle);

            var mu = gravityConstant * mass;
            double period;
            if (radiusExponent < 1.5d)
            {
                period = twoPi / Math.Sqrt(Math.Abs(mu / (semiMajorRadius * semiMajorRadius)));
            }
            else
            {
                period = twoPi / Math.Sqrt(Math.Abs(mu / (semiMajorRadius * semiMajorRadius * semiMajorRadius)));
            }

            var n = twoPi / period;
            timeSincePeriapse += timeSinceStart;
            var meanAnomaly = normalizeRadian(n * (timeSincePeriapse));
            var trueAnomalyAngle = normalizeRadian(meanAnomaly + (((2d * eccentricity) - ((1d / 4d) * eccentricity * eccentricity * eccentricity)) * Math.Sin(meanAnomaly)) + ((5d / 4d) * eccentricity * eccentricity * Math.Sin(2.0 * meanAnomaly)) + ((13d / 12d) * eccentricity * eccentricity * eccentricity * Math.Sin(3d * meanAnomaly)));
            var essentricAnomalyAngle = normalizeRadian(2d * Math.Atan(Math.Sqrt(Math.Abs((1d - eccentricity) / (1d + eccentricity))) * Math.Tan(trueAnomalyAngle / 2d)));
            var radius = semiMajorRadius * (1d - eccentricity * Math.Cos(essentricAnomalyAngle));
            var semiAxisRectum = semiMajorRadius * (1d - (eccentricity * eccentricity));

            double angularMomentum;
            if (radiusExponent < 1.5d)
            {
                angularMomentum = Math.Sqrt(Math.Abs(mu * semiAxisRectum * semiAxisRectum));
            }
            else
            {
                angularMomentum = Math.Sqrt(Math.Abs(mu * semiAxisRectum));
            }

            var sinTrue = Math.Sin(trueAnomalyAngle);
            var sinsAscend = Math.Sin(ascendingAngle);
            var cosAscend = Math.Cos(ascendingAngle);
            var sinPeriapseTrue = Math.Sin(periapseAngle + trueAnomalyAngle);
            var cosPeriapseTrue = Math.Cos(periapseAngle + trueAnomalyAngle);
            var sinInclination = Math.Sin(inclinationAngle);
            var cosInclination = Math.Cos(inclinationAngle);

            var X = radius * ((cosAscend * cosPeriapseTrue) - (sinsAscend * sinPeriapseTrue * cosInclination));
            var Y = radius * ((sinsAscend * cosPeriapseTrue) + (cosAscend * sinPeriapseTrue * cosInclination));
            var Z = radius * (sinPeriapseTrue * sinInclination);

            var dX = (((X * angularMomentum * eccentricity) / (radius * semiAxisRectum)) * sinTrue) -  ((angularMomentum / radius) * ((cosAscend * sinPeriapseTrue) + (sinsAscend * cosPeriapseTrue * cosInclination)));
            var dY = (((Y * angularMomentum * eccentricity) / (radius * semiAxisRectum)) * sinTrue) - ((angularMomentum / radius) * ((sinsAscend * sinPeriapseTrue) + (cosAscend * cosPeriapseTrue * cosInclination)));
            var dZ = (((Z * angularMomentum * eccentricity) / (radius * semiAxisRectum)) * sinTrue) + ((angularMomentum / radius) * (cosPeriapseTrue * sinInclination));

            return Tuple.Create(new Vector3((float)X, (float)Y, (float)Z), new Vector3((float)dX, (float)dY, (float)dZ));
        }
    }
}
