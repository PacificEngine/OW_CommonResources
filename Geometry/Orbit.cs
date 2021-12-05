using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PacificEngine.OW_CommonResources.Geometry
{
    public static class Orbit
    {
        public class KeplerCoordinates
        {
            public static KeplerCoordinates zero { get; } = new KeplerCoordinates(0f, 0f, 0f, 0f, 0f, 0f);

            public float eccentricity { get; }
            public float semiMajorRadius { get; }
            public float semiMinorRadius { get { return Ellipse.getMinorRadius(semiMajorRadius, foci); } }
            public float foci { get { return Ellipse.getFocus(semiMajorRadius, eccentricity); } }
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

        //https://space.stackexchange.com/questions/19322/converting-orbital-elements-to-cartesian-state-vectors
        //https://web.archive.org/web/20160418175843/https://ccar.colorado.edu/asen5070/handouts/cart2kep2002.pdf
        public static KeplerCoordinates toKeplerCoordinates(float gravityConstant, float mass, float radiusExponent, float timeSinceStart, Vector3 startPosition, Vector3 startVelocity)
        {
            var product = Vector3.Dot(startPosition, startVelocity);
            var angularMomemntumVector = Vector3.Cross(startPosition, startVelocity);
            var angularMomemntum = (double)angularMomemntumVector.magnitude;

            var mu = getMu(gravityConstant, mass);
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
            if (eccentricity < 0.01)
            {
                // This is a hack
                eccentricity = 0;
            }
            var inclinationAngle = normalizeRadian(Math.Acos(angularMomemntumVector.z/angularMomemntum)) % Math.PI;
            var ascendingAngle = normalizeRadian(Math.Atan2(angularMomemntumVector.x, -1f * angularMomemntumVector.y));
            var latitudeAngle = normalizeRadian(Math.Atan2(startPosition.z / Math.Sin(inclinationAngle), (startPosition.x * Math.Cos(ascendingAngle)) + (startPosition.y * Math.Sin(ascendingAngle))));

            var semiAxisRectum = Ellipse.getAxisRectum((float)semiMajorRadius, (float)eccentricity);
            var trueAnomaly = normalizeRadian(Math.Atan2(Math.Sqrt(semiAxisRectum / mu) * product, semiAxisRectum - radius));
            var periapseAngle = normalizeRadian(latitudeAngle - trueAnomaly);
            var essentricAnomaly = getEsscentricAnomalyFromTrueAnomaly(eccentricity, trueAnomaly);
            var meanAnomaly = getMeanAnomalyFromEsscentricAnomaly(eccentricity, essentricAnomaly);
            var period = getPeriod(radiusExponent, mu, semiMajorRadius);
            var timeSincePeriapse = getTimeSincePeriapse(period, meanAnomaly, -1d * timeSinceStart);

            return new KeplerCoordinates((float)eccentricity, (float)semiMajorRadius, Angle.toDegrees((float)inclinationAngle), Angle.toDegrees((float)periapseAngle), Angle.toDegrees((float)ascendingAngle), (float)timeSincePeriapse);
        }

        public static Tuple<Vector3, Vector3> toCartesian(float gravityConstant, float mass, float radiusExponent, float timeSinceStart, KeplerCoordinates keplerCoordinates)
        {
            return toCartesian(gravityConstant, mass, radiusExponent, timeSinceStart, keplerCoordinates.eccentricity, keplerCoordinates.semiMajorRadius, keplerCoordinates.inclinationAngle, keplerCoordinates.periapseAngle, keplerCoordinates.ascendingAngle, keplerCoordinates.timeSincePeriapse); 
        }

        //https://space.stackexchange.com/questions/19322/converting-orbital-elements-to-cartesian-state-vectors
        //https://web.archive.org/web/20170810015111/http://ccar.colorado.edu/asen5070/handouts/kep2cart_2002.doc
        public static Tuple<Vector3, Vector3> toCartesian(float gravityConstant, float mass, float radiusExponent, float timeSinceStart, float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float timeSincePeriapse)
        {
            var mu = getMu(gravityConstant, mass);
            var period = getPeriod(radiusExponent, mu, semiMajorRadius);

            var meanAnomaly = getMeanAnomalyFromTime(period, timeSincePeriapse + timeSinceStart);
            return toCartesianMeanAnomaly(gravityConstant, mass, radiusExponent, eccentricity, semiMajorRadius, inclinationAngle, periapseAngle, ascendingAngle, Angle.toDegrees((float)meanAnomaly));
        }

        public static Tuple<Vector3, Vector3> toCartesianMeanAnomaly(float gravityConstant, float mass, float radiusExponent, float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float meanAnomaly)
        {
            meanAnomaly = Angle.toRadian(meanAnomaly);

            var esscentricAnomalyAngle = getEsscentricAnomalyFromMeanAnomaly(eccentricity, meanAnomaly); 
            return toCartesianEsscentricAnomaly(gravityConstant, mass, radiusExponent, eccentricity, semiMajorRadius, inclinationAngle, periapseAngle, ascendingAngle, Angle.toDegrees((float)esscentricAnomalyAngle));
        }

        public static Tuple<Vector3, Vector3> toCartesianEsscentricAnomaly(float gravityConstant, float mass, float radiusExponent, float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float esscentricAnomaly)
        {
            esscentricAnomaly = Angle.toRadian(esscentricAnomaly);

            var essentricAnomalyAngle = getEsscentricAnomalyFromTrueAnomaly(eccentricity, esscentricAnomaly);
            return toCartesianTrueAnomaly(gravityConstant, mass, radiusExponent, eccentricity, semiMajorRadius, inclinationAngle, periapseAngle, ascendingAngle, Angle.toDegrees((float)essentricAnomalyAngle));
        }

        public static Tuple<Vector3, Vector3> toCartesianTrueAnomaly(float gravityConstant, float mass, float radiusExponent, float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float trueAnomaly)
        {
            inclinationAngle = Angle.toRadian(inclinationAngle);
            periapseAngle = Angle.toRadian(periapseAngle);
            ascendingAngle = Angle.toRadian(ascendingAngle);
            trueAnomaly = Angle.toRadian(trueAnomaly);

            var mu = getMu(gravityConstant, mass);
            var semiAxisRectum = Ellipse.getAxisRectum(semiMajorRadius, eccentricity);
            var radius = semiAxisRectum / (1.0 + eccentricity * Math.Cos(trueAnomaly));

            double angularMomentum;
            if (radiusExponent < 1.5d)
            {
                angularMomentum = Math.Sqrt(Math.Abs(mu * semiAxisRectum * semiAxisRectum));
            }
            else
            {
                angularMomentum = Math.Sqrt(Math.Abs(mu * semiAxisRectum));
            }

            var sinTrue = Math.Sin(trueAnomaly);
            var sinsAscend = Math.Sin(ascendingAngle);
            var cosAscend = Math.Cos(ascendingAngle);
            var sinPeriapseTrue = Math.Sin(periapseAngle + trueAnomaly);
            var cosPeriapseTrue = Math.Cos(periapseAngle + trueAnomaly);
            var sinInclination = Math.Sin(inclinationAngle);
            var cosInclination = Math.Cos(inclinationAngle);

            var X = radius * ((cosAscend * cosPeriapseTrue) - (sinsAscend * sinPeriapseTrue * cosInclination));
            var Y = radius * ((sinsAscend * cosPeriapseTrue) + (cosAscend * sinPeriapseTrue * cosInclination));
            var Z = radius * (sinPeriapseTrue * sinInclination);

            var dX = (((X * angularMomentum * eccentricity) / (radius * semiAxisRectum)) * sinTrue) - ((angularMomentum / radius) * ((cosAscend * sinPeriapseTrue) + (sinsAscend * cosPeriapseTrue * cosInclination)));
            var dY = (((Y * angularMomentum * eccentricity) / (radius * semiAxisRectum)) * sinTrue) - ((angularMomentum / radius) * ((sinsAscend * sinPeriapseTrue) + (cosAscend * cosPeriapseTrue * cosInclination)));
            var dZ = (((Z * angularMomentum * eccentricity) / (radius * semiAxisRectum)) * sinTrue) + ((angularMomentum / radius) * (cosPeriapseTrue * sinInclination));

            return Tuple.Create(new Vector3((float)X, (float)Y, (float)Z), new Vector3((float)dX, (float)dY, (float)dZ));
        }

        public static Tuple<Vector3, Vector3> getPeriapsis(float gravityConstant, float mass, float radiusExponent, KeplerCoordinates keplerCoordinates)
        {
            return toCartesianEsscentricAnomaly(gravityConstant, mass, radiusExponent, keplerCoordinates.eccentricity, keplerCoordinates.semiMajorRadius, keplerCoordinates.inclinationAngle, keplerCoordinates.periapseAngle, keplerCoordinates.ascendingAngle, 0);
        }

        public static Tuple<Vector3, Vector3> getDecending(float gravityConstant, float mass, float radiusExponent, KeplerCoordinates keplerCoordinates)
        {
            return toCartesianTrueAnomaly(gravityConstant, mass, radiusExponent, keplerCoordinates.eccentricity, keplerCoordinates.semiMajorRadius, keplerCoordinates.inclinationAngle, keplerCoordinates.periapseAngle, keplerCoordinates.ascendingAngle, 90);
        }

        public static Tuple<Vector3, Vector3> getSemiMinorDecending(float gravityConstant, float mass, float radiusExponent, KeplerCoordinates keplerCoordinates)
        {
            return toCartesianEsscentricAnomaly(gravityConstant, mass, radiusExponent, keplerCoordinates.eccentricity, keplerCoordinates.semiMajorRadius, keplerCoordinates.inclinationAngle, keplerCoordinates.periapseAngle, keplerCoordinates.ascendingAngle, 90);
        }

        public static Tuple<Vector3, Vector3> getApoapsis(float gravityConstant, float mass, float radiusExponent, KeplerCoordinates keplerCoordinates)
        {
            return toCartesianEsscentricAnomaly(gravityConstant, mass, radiusExponent, keplerCoordinates.eccentricity, keplerCoordinates.semiMajorRadius, keplerCoordinates.inclinationAngle, keplerCoordinates.periapseAngle, keplerCoordinates.ascendingAngle, 180);
        }

        public static Tuple<Vector3, Vector3> getAscending(float gravityConstant, float mass, float radiusExponent, KeplerCoordinates keplerCoordinates)
        {
            return toCartesianTrueAnomaly(gravityConstant, mass, radiusExponent, keplerCoordinates.eccentricity, keplerCoordinates.semiMajorRadius, keplerCoordinates.inclinationAngle, keplerCoordinates.periapseAngle, keplerCoordinates.ascendingAngle, 270);
        }

        public static Tuple<Vector3, Vector3> getSemiMinorAscending(float gravityConstant, float mass, float radiusExponent, KeplerCoordinates keplerCoordinates)
        {
            return toCartesianEsscentricAnomaly(gravityConstant, mass, radiusExponent, keplerCoordinates.eccentricity, keplerCoordinates.semiMajorRadius, keplerCoordinates.inclinationAngle, keplerCoordinates.periapseAngle, keplerCoordinates.ascendingAngle, 270);
        }

        public static float getMeanAnomalyAngle(float gravityConstant, float mass, float radiusExponent, float timeSinceStart, KeplerCoordinates keplerCoordinates)
        {
            var mu = getMu(gravityConstant, mass);
            var semiMajorRadius = keplerCoordinates.semiMajorRadius;
            var timeSincePeriapse = keplerCoordinates.timeSincePeriapse;
            var eccentricity = keplerCoordinates.eccentricity;

            var period = getPeriod(radiusExponent, mu, semiMajorRadius);
            return Angle.toDegrees((float)getMeanAnomalyFromTime(period, timeSinceStart + timeSincePeriapse));
        }

        public static float getEsscentricAnomalyAngle(float gravityConstant, float mass, float radiusExponent, float timeSinceStart, KeplerCoordinates keplerCoordinates)
        {
            var meanAnomaly = Angle.toRadian(getMeanAnomalyAngle(gravityConstant, mass, radiusExponent, timeSinceStart, keplerCoordinates));
            return Angle.toDegrees((float)getEsscentricAnomalyFromMeanAnomaly(keplerCoordinates.eccentricity, meanAnomaly));
        }

        public static float getTrueAnomalyAngle(float gravityConstant, float mass, float radiusExponent, float timeSinceStart, KeplerCoordinates keplerCoordinates)
        {
            var esscentricAnomaly = Angle.toRadian(getEsscentricAnomalyAngle(gravityConstant, mass, radiusExponent, timeSinceStart, keplerCoordinates));
            return Angle.toDegrees((float)getTrueAnomalyFromEsscentricAnomaly(keplerCoordinates.eccentricity, esscentricAnomaly));
        }

        public static float getPeriod(float gravityConstant, float mass, float radiusExponent, double semiMajorRadius)
        {
            var mu = getMu(gravityConstant, mass);
            return (float)getPeriod(radiusExponent, mu, semiMajorRadius);
        }

        private static double getMu(double gravityConstant, double mass)
        {
            return Math.Abs(gravityConstant * mass);
        }

        private static double getPeriod(double radiusExponent, double mu, double semiMajorRadius)
        {
            if (radiusExponent < 1.5d)
            {
                return twoPi / Math.Sqrt(Math.Abs(mu / (semiMajorRadius * semiMajorRadius)));
            }
            else
            {
                return twoPi / Math.Sqrt(Math.Abs(mu / (semiMajorRadius * semiMajorRadius * semiMajorRadius)));
            }
        }

        /*private static double getTrueAnomalyFromMeanAnomaly(double eccentricity, double meanAnomaly)
        {
            return normalizeRadian(meanAnomaly
                + (2d * eccentricity) * Math.Sin(meanAnomaly));
            /*return normalizeRadian(meanAnomaly
                + (((2d * eccentricity) - ((0.25) * eccentricity * eccentricity * eccentricity)) * Math.Sin(meanAnomaly))
                + ((1.25) * eccentricity * eccentricity * Math.Sin(2.0 * meanAnomaly))
                + ((13d / 12d) * eccentricity * eccentricity * eccentricity * Math.Sin(3d * meanAnomaly)));*/
        //}

        private static double getTrueAnomalyFromEsscentricAnomaly(double eccentricity, double esscentricAnomaly)
        {
            /*(if (0.999f < eccentricity && eccentricity < 1.001f)
                return normalizeRadian(2d * Math.Atan2(Math.Sin(esscentricAnomaly) * Math.Sqrt(1d - (eccentricity * eccentricity)), Math.Cos(esscentricAnomaly) - eccentricity));
            return normalizeRadian(2d * Math.Atan(Math.Sqrt(Math.Abs((1d + eccentricity) / (1d - eccentricity))) * Math.Tan(esscentricAnomaly / 2d)));*/

            // return normalizeRadian(2d * Math.Atan(Math.Sqrt(Math.Abs((1d - eccentricity) / (1d + eccentricity))) * Math.Tan(esscentricAnomaly / 2d)));

            if (0.9999f < eccentricity && eccentricity < 1.0001f)
                return normalizeRadian(2d * Math.Atan2(Math.Sin(esscentricAnomaly) * Math.Sqrt(1d - (eccentricity * eccentricity)), Math.Cos(esscentricAnomaly) - eccentricity));
            return normalizeRadian(2d * Math.Atan(Math.Sqrt(Math.Abs((1d + eccentricity) / (1d - eccentricity))) * Math.Tan(esscentricAnomaly / 2d)));
        }

        private static double getEsscentricAnomalyFromTrueAnomaly(double eccentricity, double trueAnomaly)
        {
            /*if (0.999f < eccentricity && eccentricity < 1.001f)
                return normalizeRadian(2d * Math.Atan2(Math.Sin(trueAnomaly) * Math.Sqrt(1d - (eccentricity * eccentricity)), Math.Cos(trueAnomaly) - eccentricity));
            return normalizeRadian(2d * Math.Atan(Math.Sqrt(Math.Abs((1d + eccentricity) / (1d - eccentricity))) * Math.Tan(trueAnomaly / 2d)));*/
            //return normalizeRadian(Math.Atan((Math.Sqrt(1 - (eccentricity * eccentricity)) * Math.Sin(trueAnomaly)) / (eccentricity + Math.Cos(trueAnomaly))));

            return normalizeRadian(2d * Math.Atan(Math.Sqrt(Math.Abs((1d - eccentricity) / (1d + eccentricity))) * Math.Tan(trueAnomaly / 2d)));
        }

        private static double getMeanAnomalyFromEsscentricAnomaly(double eccentricity, double esscentricAnomaly)
        {
            return normalizeRadian(esscentricAnomaly - eccentricity * Math.Sin(esscentricAnomaly));
        }

        private static double getEsscentricAnomalyFromMeanAnomaly(double eccentricity, double meanAnomaly)
        {
            double estimate = meanAnomaly;
            for (int i = 0; i < 10; i++)
            {
                estimate += (meanAnomaly - getMeanAnomalyFromEsscentricAnomaly(eccentricity, estimate)) / 2.0;
            }
            return normalizeRadian(estimate);
        }

        private static double getMeanAnomalyFromTime(double period, double timeSincePeriapse)
        {
            var n = twoPi / period;
            return normalizeRadian(n * (timeSincePeriapse));
        }

        private static double getTimeSincePeriapse(double period, double meanAnomaly, double timeAdjustment)
        {
            var n = twoPi / period;
            var timeSincePeriapse = meanAnomaly / n;
            return getMeanAnomalyFromTime(period, timeAdjustment + timeSincePeriapse) / n;
        }
    }
}
