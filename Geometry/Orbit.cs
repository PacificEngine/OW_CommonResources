using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PacificEngine.OW_CommonResources.Geometry
{
    public static class Orbit
    {
        public class KelperCoordinates
        {
            public static KelperCoordinates zero { get; } = new KelperCoordinates(0f, 0f, 0f, 0f, 0f, 0f, 0f);

            public float eccentricity { get; }
            public float semiMajorRadius { get; }
            public float inclinationAngle { get; }
            public float periapseAngle { get; }
            public float ascendingAngle { get; }
            public float essentricAnomaly { get; }
            public float timeSincePeriapse { get; }

            public KelperCoordinates(float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float essentricAnomaly, float timeSincePeriapse)
            {
                this.eccentricity = eccentricity;
                this.semiMajorRadius = semiMajorRadius;
                this.inclinationAngle = inclinationAngle;
                this.periapseAngle = periapseAngle;
                this.ascendingAngle = ascendingAngle;
                this.essentricAnomaly = essentricAnomaly;
                this.timeSincePeriapse = timeSincePeriapse;
            }

            public override string ToString()
            {
                return "(" + eccentricity + "," + semiMajorRadius + "," + inclinationAngle + "," + periapseAngle + "," + ascendingAngle + "," + essentricAnomaly + "," + timeSincePeriapse + ")";
            }

            public override bool Equals(System.Object other)
            {
                if (other != null && other is KelperCoordinates)
                {
                    var obj = other as KelperCoordinates;
                    return eccentricity == obj.eccentricity
                        && semiMajorRadius == obj.semiMajorRadius
                        && inclinationAngle == obj.inclinationAngle
                        && periapseAngle == obj.periapseAngle
                        && ascendingAngle == obj.ascendingAngle
                        && essentricAnomaly == obj.essentricAnomaly
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
                    + (essentricAnomaly.GetHashCode() * 4096)
                    + (timeSincePeriapse.GetHashCode() * 16384);
            }
        }

        private const double twoPi = (2d * Math.PI);

        public static double normalizeRadian(double radians)
        {
            return (((radians % twoPi) + twoPi) % twoPi);
        }

        public static KelperCoordinates toKeplerCoordinates(float gravityConstant, float mass, float timeSinceStart, Vector3 startPosition, Vector3 startVelocity)
        {
            var product = (double)startPosition.x * startVelocity.x + startPosition.y * startVelocity.y + startPosition.z * startVelocity.z;
            var angularMomemntumVector = new Vector3(startPosition.y * startVelocity.z - startPosition.z * startVelocity.y, startPosition.z * startVelocity.x - startPosition.x * startVelocity.z, startPosition.x * startVelocity.y - startPosition.y * startVelocity.x);
            var angularMomemntum = (double)angularMomemntumVector.magnitude;

            var mu = (double)gravityConstant * mass;
            var radius = (double)startPosition.magnitude;
            var speed = (double)startVelocity.magnitude;
            var specificEnergy = (double)(speed * speed) / 2f - (mu / radius);
            var semiMajorRadius = (-1f * mu) / (2f * specificEnergy);
            var eccentricity = Math.Sqrt(1 - ((angularMomemntum * angularMomemntum) / (semiMajorRadius * mu)));
            var inclinationAngle = normalizeRadian(Math.Acos(angularMomemntumVector.z/angularMomemntum)) % Math.PI;
            var ascendingAngle = normalizeRadian(Math.Atan2(angularMomemntumVector.x, -1f * angularMomemntumVector.y));
            var latitudeAngle = normalizeRadian(Math.Atan2(startPosition.z / Math.Sin(inclinationAngle), (startPosition.x * Math.Cos(ascendingAngle)) + (startPosition.y * Math.Sin(ascendingAngle))));

            var semiMinorRadius = (semiMajorRadius * (1f - eccentricity * eccentricity));
            var trueAnomalyAngle = normalizeRadian(Math.Atan2(Math.Sqrt(semiMinorRadius / mu) * product, semiMinorRadius - radius));
            var periapseAngle = normalizeRadian(latitudeAngle - trueAnomalyAngle);
            var essentricAnomaly = normalizeRadian(2f * Math.Atan(Math.Sqrt((1f - eccentricity)/(1f + eccentricity)) * Math.Tan(trueAnomalyAngle/2f)));
            var n = Math.Sqrt(mu / (semiMajorRadius * semiMajorRadius * semiMajorRadius));
            var timeSincePeriapse = timeSinceStart - (1f / n) * (essentricAnomaly - eccentricity * Math.Sin(essentricAnomaly));

            return new KelperCoordinates((float)eccentricity, (float)semiMajorRadius, Angle.toDegrees((float)inclinationAngle), Angle.toDegrees((float)periapseAngle), Angle.toDegrees((float)ascendingAngle), Angle.toDegrees((float)essentricAnomaly), (float)timeSincePeriapse);
        }

        public static Tuple<Vector3, Vector3> toCartesian(float gravityConstant, float mass, float timeSinceStart, KelperCoordinates keplerCoordinates)
        {
            return toCartesian(gravityConstant, mass, timeSinceStart, keplerCoordinates.eccentricity, keplerCoordinates.semiMajorRadius, keplerCoordinates.inclinationAngle, keplerCoordinates.periapseAngle, keplerCoordinates.ascendingAngle, keplerCoordinates.essentricAnomaly, keplerCoordinates.timeSincePeriapse); 
        }

        public static Tuple<Vector3, Vector3> toCartesian(float gravityConstant, float mass, float timeSinceStart, float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float essentricAnomaly, float timeSincePeriapse)
        {
            inclinationAngle = Angle.toRadian(inclinationAngle);
            periapseAngle = Angle.toRadian(periapseAngle);
            ascendingAngle = Angle.toRadian(ascendingAngle);
            essentricAnomaly = Angle.toRadian(essentricAnomaly);

            var mu = gravityConstant * mass;
            var n = Math.Sqrt(mu / (semiMajorRadius * semiMajorRadius * semiMajorRadius));
            var mean = n * (timeSinceStart - timeSincePeriapse);
            var meanAnomaly = normalizeRadian(essentricAnomaly - eccentricity * Math.Sin(essentricAnomaly));
            var trueAnomalyAngle = normalizeRadian(2f * Math.Atan(Math.Sqrt((1f + eccentricity) / (1f - eccentricity)) * Math.Tan(essentricAnomaly / 2f)));
            var radius = semiMajorRadius * (1f - eccentricity * Math.Cos(essentricAnomaly));
            var angularMomentum = Math.Sqrt(mu * semiMajorRadius * (1f - (eccentricity * eccentricity)));

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

            var semiMinorRadius = semiMajorRadius * (1f - (eccentricity * eccentricity));

            var dX = (((X * angularMomentum * eccentricity)/(radius * semiMinorRadius)) * sinTrue) -  ((angularMomentum / radius) * ((cosAscend * sinPeriapseTrue) + (sinsAscend * cosPeriapseTrue * cosInclination)));
            var dY = (((Y * angularMomentum * eccentricity) / (radius * semiMinorRadius)) * sinTrue) - ((angularMomentum / radius) * ((sinsAscend * sinPeriapseTrue) + (cosAscend * cosPeriapseTrue * cosInclination)));
            var dZ = (((Z * angularMomentum * eccentricity) / (radius * semiMinorRadius)) * sinTrue) + ((angularMomentum / radius) * (cosPeriapseTrue * sinInclination));

            return Tuple.Create(new Vector3((float)X, (float)Y, (float)Z), new Vector3((float)dX, (float)dY, (float)dZ));
        }
    }
}
