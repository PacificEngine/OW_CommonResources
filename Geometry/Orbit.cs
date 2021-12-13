using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PacificEngine.OW_CommonResources.Geometry
{
    // TODO: Figure out why Kepler to Cartisian is broken for non-circular orbits
    public static class Orbit
    {
        public class Gravity
        {
            private const float twoPi = (float)(2d * Math.PI);

            public float gravityConstant { get; }
            public float exponent { get; }
            public float mass { get; }
            public float mu { get { return Math.Abs(gravityConstant * mass); } }

            public Gravity(float gravityConstant, float exponent, float mass)
            {
                this.gravityConstant = gravityConstant;
                this.exponent = exponent;
                this.mass = mass;
            }

            public static Gravity of(float exponent, float mass)
            {
                return new Gravity(GravityVolume.GRAVITATIONAL_CONSTANT, exponent, mass);
            }

            public float getPeriod(float semiMajorRadius)
            {
                var mu = this.mu;
                if (exponent < 1.5f)
                {
                    return twoPi / (float)Math.Sqrt(Math.Abs(mu / (semiMajorRadius * semiMajorRadius)));
                }
                else
                {
                    return twoPi / (float)Math.Sqrt(Math.Abs(mu / (semiMajorRadius * semiMajorRadius * semiMajorRadius)));
                }
            }

            public float getAngularMomentum(float semiAxisRectum)
            {
                var mu = this.mu;
                if (exponent < 1.5f)
                {
                    return (float)Math.Sqrt(Math.Abs(mu * semiAxisRectum * semiAxisRectum));
                }
                else
                {
                    return (float)Math.Sqrt(Math.Abs(mu * semiAxisRectum));
                }
            }

            public override string ToString()
            {
                return $"({Math.Round(gravityConstant, 4).ToString("G4")}, {Math.Round(exponent, 1).ToString("G1")}, {Math.Round(mass, 4).ToString("G4")})";
            }

            public override bool Equals(System.Object other)
            {
                if (other != null && other is Gravity)
                {
                    var obj = other as Gravity;
                    return gravityConstant == obj.gravityConstant
                        && exponent == obj.exponent
                        && mass == obj.mass;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return (gravityConstant.GetHashCode() * 4)
                    + (exponent.GetHashCode() * 16)
                    + (mass.GetHashCode() * 64);
            }
        }

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
        //https://downloads.rene-schwarz.com/download/M002-Cartesian_State_Vectors_to_Keplerian_Orbit_Elements.pdf
        public static KeplerCoordinates toKeplerCoordinates(Gravity parent, float timeSinceStart, Vector3 startPosition, Vector3 startVelocity)
        {
            var product = Vector3.Dot(startPosition, startVelocity);
            var orbitalMomentum = Vector3.Cross(startPosition, startVelocity);
            var angularMomemntum = (double)orbitalMomentum.magnitude;

            var mu = parent.mu;
            var radius = (double)startPosition.magnitude;
            var speed = (double)startVelocity.magnitude;
            double specificEnergy;
            double semiMajorRadius;
            double eccentricity;

            if (parent.exponent < 1.5d)
            {
                specificEnergy = (speed * speed) / 2d - mu;
                eccentricity = Math.Abs(Math.Sqrt(((2d * specificEnergy) / mu) + 2d) - 1d);
                semiMajorRadius = angularMomemntum / (Math.Sqrt(mu) * (1d - (eccentricity * eccentricity)));
            }
            else
            {
                //https://web.archive.org/web/20160418175843/https://ccar.colorado.edu/asen5070/handouts/cart2kep2002.pdf
                specificEnergy = (speed * speed) / 2d - (mu / radius);
                semiMajorRadius = Math.Abs(mu / (2d * specificEnergy));
                eccentricity = Math.Sqrt(Math.Abs(1 - ((angularMomemntum * angularMomemntum) / (semiMajorRadius * mu))));

                //https://downloads.rene-schwarz.com/download/M002-Cartesian_State_Vectors_to_Keplerian_Orbit_Elements.pdf
                //eccentricity = ((Vector3.Cross(startVelocity, orbitalMomentum) / mu) - (startPosition * (1f / (float)radius))).magnitude;
                //semiMajorRadius = 1d / Math.Abs((2d / startPosition.magnitude) - (startVelocity.magnitude / mu));
            }
            var inclinationAngle = normalizeRadian(Math.Acos(orbitalMomentum.z/angularMomemntum)) % Math.PI;
            var ascendingAngle = normalizeRadian(Math.Atan2(orbitalMomentum.x, -1f * orbitalMomentum.y));
            var latitudeAngle = normalizeRadian(Math.Atan2(startPosition.z / Math.Sin(inclinationAngle), (startPosition.x * Math.Cos(ascendingAngle)) + (startPosition.y * Math.Sin(ascendingAngle))));

            var semiAxisRectum = Ellipse.getAxisRectum((float)semiMajorRadius, (float)eccentricity);
            var trueAnomaly = normalizeRadian(Math.Atan2(Math.Sqrt(semiAxisRectum / mu) * product, semiAxisRectum - radius));
            var periapseAngle = normalizeRadian(latitudeAngle - trueAnomaly);
            var essentricAnomaly = getEsscentricAnomalyFromTrueAnomaly(eccentricity, trueAnomaly);
            var meanAnomaly = getMeanAnomalyFromEsscentricAnomaly(eccentricity, essentricAnomaly);
            var period = parent.getPeriod((float)semiMajorRadius);
            var timeSincePeriapse = getTimeSincePeriapse(period, meanAnomaly, -1d * timeSinceStart);

            return new KeplerCoordinates((float)eccentricity, (float)semiMajorRadius, Angle.toDegrees((float)inclinationAngle), Angle.toDegrees((float)periapseAngle), Angle.toDegrees((float)ascendingAngle), (float)timeSincePeriapse);
        }

        //https://space.stackexchange.com/questions/19322/converting-orbital-elements-to-cartesian-state-vectors
        //https://web.archive.org/web/20170810015111/http://ccar.colorado.edu/asen5070/handouts/kep2cart_2002.doc
        //https://downloads.rene-schwarz.com/download/M001-Keplerian_Orbit_Elements_to_Cartesian_State_Vectors.pdf
        public static Tuple<Vector3, Vector3> toCartesian(Gravity parent, float timeSinceStart, KeplerCoordinates keplerCoordinates)
        {
            return toCartesian(parent, timeSinceStart, keplerCoordinates.eccentricity, keplerCoordinates.semiMajorRadius, keplerCoordinates.inclinationAngle, keplerCoordinates.periapseAngle, keplerCoordinates.ascendingAngle, keplerCoordinates.timeSincePeriapse); 
        }

        public static Tuple<Vector3, Vector3> toCartesian(Gravity parent, float timeSinceStart, float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float timeSincePeriapse)
        {
            var period = parent.getPeriod(semiMajorRadius);

            var meanAnomaly = getMeanAnomalyFromTime(period, timeSincePeriapse + timeSinceStart);
            return toCartesianMeanAnomaly(parent, eccentricity, semiMajorRadius, inclinationAngle, periapseAngle, ascendingAngle, Angle.toDegrees((float)meanAnomaly));
        }

        public static Tuple<Vector3, Vector3> toCartesianMeanAnomaly(Gravity parent, float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float meanAnomaly)
        {
            meanAnomaly = Angle.toRadian(meanAnomaly);

            var esscentricAnomalyAngle = getEsscentricAnomalyFromMeanAnomaly(eccentricity, meanAnomaly); 
            return toCartesianEsscentricAnomaly(parent, eccentricity, semiMajorRadius, inclinationAngle, periapseAngle, ascendingAngle, Angle.toDegrees((float)esscentricAnomalyAngle));
        }

        public static Tuple<Vector3, Vector3> toCartesianEsscentricAnomaly(Gravity parent, float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float esscentricAnomaly)
        {
            esscentricAnomaly = Angle.toRadian(esscentricAnomaly);

            var essentricAnomalyAngle = getEsscentricAnomalyFromTrueAnomaly(eccentricity, esscentricAnomaly);
            return toCartesianTrueAnomaly(parent, eccentricity, semiMajorRadius, inclinationAngle, periapseAngle, ascendingAngle, Angle.toDegrees((float)essentricAnomalyAngle));
        }

        public static Tuple<Vector3, Vector3> toCartesianTrueAnomaly(Gravity parent, float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float trueAnomaly)
        {
            inclinationAngle = Angle.toRadian(inclinationAngle);
            periapseAngle = Angle.toRadian(periapseAngle);
            ascendingAngle = Angle.toRadian(ascendingAngle);
            trueAnomaly = Angle.toRadian(trueAnomaly);

            var semiAxisRectum = Ellipse.getAxisRectum(semiMajorRadius, eccentricity);
            var radius = semiAxisRectum / (1.0 + eccentricity * Math.Cos(trueAnomaly));

            double angularMomentum = parent.getAngularMomentum(semiAxisRectum);

            //https://web.archive.org/web/20170810015111/http://ccar.colorado.edu/asen5070/handouts/kep2cart_2002.doc
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

            //https://downloads.rene-schwarz.com/download/M001-Keplerian_Orbit_Elements_to_Cartesian_State_Vectors.pdf
            /*var escentricAngle = getEsscentricAnomalyFromTrueAnomaly(eccentricity, trueAnomaly);

            var sinsAscend = Math.Sin(ascendingAngle); // sun O
            var cosAscend = Math.Cos(ascendingAngle); // cos O
            var sinPeriapse = Math.Sin(periapseAngle); // sin w
            var cosPeriapse = Math.Cos(periapseAngle); // cos w
            var sinInclination = Math.Sin(inclinationAngle); // sin i
            var cosInclination = Math.Cos(inclinationAngle); // cos i

            var oX = radius * Math.Cos(trueAnomaly);
            var oY = radius * Math.Sin(trueAnomaly);

            var od = Math.Sqrt(parent.mu * semiMajorRadius) / radius;
            var odX = od * -1d * Math.Sin(escentricAngle);
            var odY = od * Math.Sqrt(1 - (eccentricity * eccentricity)) * Math.Cos(escentricAngle);

            var X = (oX * ((cosPeriapse * cosAscend) - (sinPeriapse * cosInclination * sinsAscend)) - oY * ((sinPeriapse * cosAscend) + (cosPeriapse * cosInclination * sinsAscend)));
            var Y = (oX * ((cosPeriapse * sinsAscend) - (sinPeriapse * cosInclination * cosAscend)) - oY * ((cosPeriapse * cosInclination * cosAscend) + (sinPeriapse * sinsAscend)));
            var Z = ((oX * sinPeriapse * sinInclination) + (oY * cosPeriapse * sinInclination));

            var dX = (odX * ((cosPeriapse * cosAscend) - (sinPeriapse * cosInclination * sinsAscend)) - odY * ((sinPeriapse * cosAscend) + (cosPeriapse * cosInclination * sinsAscend)));
            var dY = (odX * ((cosPeriapse * sinsAscend) - (sinPeriapse * cosInclination * cosAscend)) - odY * ((cosPeriapse * cosInclination * cosAscend) + (sinPeriapse * sinsAscend)));
            var dZ = ((odX * sinPeriapse * sinInclination) + (odY * cosPeriapse * sinInclination));*/

            return Tuple.Create(new Vector3((float)X, (float)Y, (float)Z), new Vector3((float)dX, (float)dY, (float)dZ));
        }

        public static Tuple<Vector3, Vector3> getPeriapsis(Gravity parent, KeplerCoordinates keplerCoordinates)
        {
            return toCartesianEsscentricAnomaly(parent, keplerCoordinates.eccentricity, keplerCoordinates.semiMajorRadius, keplerCoordinates.inclinationAngle, keplerCoordinates.periapseAngle, keplerCoordinates.ascendingAngle, 0);
        }

        public static Tuple<Vector3, Vector3> getDecending(Gravity parent, KeplerCoordinates keplerCoordinates)
        {
            return toCartesianTrueAnomaly(parent, keplerCoordinates.eccentricity, keplerCoordinates.semiMajorRadius, keplerCoordinates.inclinationAngle, keplerCoordinates.periapseAngle, keplerCoordinates.ascendingAngle, 90);
        }

        public static Tuple<Vector3, Vector3> getSemiMinorDecending(Gravity parent, KeplerCoordinates keplerCoordinates)
        {
            return toCartesianEsscentricAnomaly(parent, keplerCoordinates.eccentricity, keplerCoordinates.semiMajorRadius, keplerCoordinates.inclinationAngle, keplerCoordinates.periapseAngle, keplerCoordinates.ascendingAngle, 90);
        }

        public static Tuple<Vector3, Vector3> getApoapsis(Gravity parent, KeplerCoordinates keplerCoordinates)
        {
            return toCartesianEsscentricAnomaly(parent, keplerCoordinates.eccentricity, keplerCoordinates.semiMajorRadius, keplerCoordinates.inclinationAngle, keplerCoordinates.periapseAngle, keplerCoordinates.ascendingAngle, 180);
        }

        public static Tuple<Vector3, Vector3> getAscending(Gravity parent, KeplerCoordinates keplerCoordinates)
        {
            return toCartesianTrueAnomaly(parent, keplerCoordinates.eccentricity, keplerCoordinates.semiMajorRadius, keplerCoordinates.inclinationAngle, keplerCoordinates.periapseAngle, keplerCoordinates.ascendingAngle, 270);
        }

        public static Tuple<Vector3, Vector3> getSemiMinorAscending(Gravity parent, KeplerCoordinates keplerCoordinates)
        {
            return toCartesianEsscentricAnomaly(parent, keplerCoordinates.eccentricity, keplerCoordinates.semiMajorRadius, keplerCoordinates.inclinationAngle, keplerCoordinates.periapseAngle, keplerCoordinates.ascendingAngle, 270);
        }

        public static float getMeanAnomalyAngle(Gravity parent, float timeSinceStart, KeplerCoordinates keplerCoordinates)
        {
            var semiMajorRadius = keplerCoordinates.semiMajorRadius;
            var timeSincePeriapse = keplerCoordinates.timeSincePeriapse;
            var eccentricity = keplerCoordinates.eccentricity;

            var period = parent.getPeriod(semiMajorRadius);
            return Angle.toDegrees((float)getMeanAnomalyFromTime(period, timeSinceStart + timeSincePeriapse));
        }

        public static float getEsscentricAnomalyAngle(Gravity parent, float timeSinceStart, KeplerCoordinates keplerCoordinates)
        {
            var meanAnomaly = Angle.toRadian(getMeanAnomalyAngle(parent, timeSinceStart, keplerCoordinates));
            return Angle.toDegrees((float)getEsscentricAnomalyFromMeanAnomaly(keplerCoordinates.eccentricity, meanAnomaly));
        }

        public static float getTrueAnomalyAngle(Gravity parent, float timeSinceStart, KeplerCoordinates keplerCoordinates)
        {
            var esscentricAnomaly = Angle.toRadian(getEsscentricAnomalyAngle(parent, timeSinceStart, keplerCoordinates));
            return Angle.toDegrees((float)getTrueAnomalyFromEsscentricAnomaly(keplerCoordinates.eccentricity, esscentricAnomaly));
        }

        private static double getTrueAnomalyFromEsscentricAnomaly(double eccentricity, double esscentricAnomaly)
        {
            /*if (0.9999f < eccentricity && eccentricity < 1.0001f)
                return normalizeRadian(2d * Math.Atan2(Math.Sin(esscentricAnomaly) * Math.Sqrt(1d - (eccentricity * eccentricity)), Math.Cos(esscentricAnomaly) - eccentricity));
            return normalizeRadian(2d * Math.Atan(Math.Sqrt(Math.Abs((1d + eccentricity) / (1d - eccentricity))) * Math.Tan(esscentricAnomaly / 2d)));*/

            //https://downloads.rene-schwarz.com/download/M002-Cartesian_State_Vectors_to_Keplerian_Orbit_Elements.pdf
            return normalizeRadian(2d * Math.Atan2(Math.Sqrt(Math.Abs(1d + eccentricity)) * (Math.Sin(esscentricAnomaly) / 2d), Math.Sqrt(Math.Abs(1d - eccentricity)) * (Math.Cos(esscentricAnomaly) / 2d)));
        }

        private static double getEsscentricAnomalyFromTrueAnomaly(double eccentricity, double trueAnomaly)
        {
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
                estimate += (meanAnomaly - getMeanAnomalyFromEsscentricAnomaly(eccentricity, estimate)) / 1.2;

                //https://downloads.rene-schwarz.com/download/M002-Cartesian_State_Vectors_to_Keplerian_Orbit_Elements.pdf
                //estimate = normalizeRadian(estimate - (((estimate - (eccentricity * Math.Sin(estimate))) - meanAnomaly) / (estimate - (eccentricity * Math.Cos(estimate)))));
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
