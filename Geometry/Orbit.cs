using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PacificEngine.OW_CommonResources.Geometry
{
    public static class Orbit
    {
        public class Gravity
        {
            private const float twoPi = (float)(2d * Math.PI);

            private float _mu = float.NaN;

            public float gravityConstant { get; }
            public float exponent { get; }
            public float mass { get; }
            public float mu { get { if (float.IsNaN(_mu)) _mu = Math.Abs(gravityConstant * mass); return _mu; } }

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
            private const float twoPi = (float)(2d * Math.PI);

            public static KeplerCoordinates zero { get; } = new KeplerCoordinates(0f, 0f, 0f, 0f, 0f, 0f);

            private float _semiMinorRadius = float.NaN;
            private float _foci = float.NaN;
            private float _esccentricAnomaly = float.NaN;
            private float _meanAnomaly = float.NaN;


            public float eccentricity { get; }
            public float semiMajorRadius { get; }
            public float inclinationAngle { get; }
            public float periapseAngle { get; }
            public float ascendingAngle { get; }
            public float trueAnomaly { get; }

            public float semiMinorRadius { get { if (float.IsNaN(_semiMinorRadius)) _semiMinorRadius = Ellipse.getMinorRadius(semiMajorRadius, eccentricity); return _semiMinorRadius; } }
            public float foci { get { if (float.IsNaN(_foci)) _foci = Ellipse.getFocus(semiMajorRadius, eccentricity); return _foci; } }
            public float apoapsis { get { return semiMajorRadius + foci; } }
            public float periapsis { get { return semiMajorRadius - foci; } }
            public float esccentricAnomaly { get { if (float.IsNaN(_esccentricAnomaly)) _esccentricAnomaly = Angle.toDegrees(getEsscentricAnomalyFromTrueAnomaly(eccentricity, Angle.toRadian(trueAnomaly))); return _esccentricAnomaly; } }
            public float meanAnomaly { get { if (float.IsNaN(_meanAnomaly)) _meanAnomaly = Angle.toDegrees(getMeanAnomalyFromEsscentricAnomaly(eccentricity, Angle.toRadian(esccentricAnomaly))); return _meanAnomaly; } }

            private KeplerCoordinates(float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float trueAnomaly)
            {
                this.eccentricity = eccentricity;
                this.semiMajorRadius = semiMajorRadius;
                inclinationAngle = Angle.normalizeDegrees(inclinationAngle);
                if (inclinationAngle >= 180f)
                {
                    inclinationAngle = 180f - (inclinationAngle - 180f);
                }
                this.inclinationAngle = inclinationAngle;
                this.periapseAngle = Angle.normalizeDegrees(periapseAngle);
                this.ascendingAngle = Angle.normalizeDegrees(ascendingAngle);
                this.trueAnomaly = Angle.normalizeDegrees(trueAnomaly);
            }

            public static KeplerCoordinates fromTrueAnomaly(float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float trueAnomaly)
            {
                return new KeplerCoordinates(eccentricity, semiMajorRadius, inclinationAngle, periapseAngle, periapseAngle, trueAnomaly);
            }

            public static KeplerCoordinates fromEccentricAnomaly(float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float eccentricAnomaly)
            {
                var trueAnomaly = Angle.toDegrees(getTrueAnomalyFromEsscentricAnomaly(eccentricity, Angle.toRadian(eccentricAnomaly)));

                return fromTrueAnomaly(eccentricity, semiMajorRadius, inclinationAngle, periapseAngle, ascendingAngle, trueAnomaly);
            }

            public static KeplerCoordinates fromMeanAnomaly(float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float meanAnomaly)
            {
                var eccentricAnomaly = Angle.toDegrees(getEsscentricAnomalyFromMeanAnomaly(eccentricity, Angle.toRadian(meanAnomaly)));

                return fromTrueAnomaly(eccentricity, semiMajorRadius, inclinationAngle, periapseAngle, ascendingAngle, eccentricAnomaly);
            }

            public static KeplerCoordinates fromTimeSincePeriapsis(Gravity gravity, float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float timeSincePeriapsis)
            {
                var meanAnomaly = Angle.toDegrees((twoPi * timeSincePeriapsis) / gravity.getPeriod(semiMajorRadius));

                return fromMeanAnomaly(eccentricity, semiMajorRadius, inclinationAngle, periapseAngle, ascendingAngle, meanAnomaly);
            }

            public float getTimeSincePeriapsis(Gravity gravity)
            {
                return (Angle.toRadian(meanAnomaly) * gravity.getPeriod(semiMajorRadius)) / twoPi;
            }

            public override string ToString()
            {
                return $"({Math.Round(eccentricity, 4).ToString("G9")}, {Math.Round(semiMajorRadius, 7).ToString("G9")}, {Math.Round(inclinationAngle, 4).ToString("G9")}, {Math.Round(periapseAngle, 4).ToString("G9")}, {Math.Round(ascendingAngle, 4).ToString("G9")}, {Math.Round(trueAnomaly, 4).ToString("G9")})";
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
                        && meanAnomaly == obj.meanAnomaly;
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
                    + (meanAnomaly.GetHashCode() * 16384);
            }

            public bool isOrbit()
            {
                return
                    !float.IsNaN(eccentricity) && !float.IsInfinity(eccentricity) && 0f <= eccentricity && eccentricity <= 1f
                        && !float.IsNaN(semiMajorRadius) && !float.IsInfinity(semiMajorRadius)
                        && !float.IsNaN(inclinationAngle) && !float.IsInfinity(inclinationAngle) && 0f <= inclinationAngle && inclinationAngle <= 180f
                        && !float.IsNaN(periapseAngle) && !float.IsInfinity(periapseAngle) && 0f <= periapseAngle && periapseAngle <= 360f
                        && !float.IsNaN(ascendingAngle) && !float.IsInfinity(ascendingAngle) && 0f <= ascendingAngle && ascendingAngle <= 360f
                        && !float.IsNaN(meanAnomaly) && !float.IsInfinity(meanAnomaly) && 0f <= meanAnomaly && meanAnomaly <= 360f;
            }


            private static float getTrueAnomalyFromEsscentricAnomaly(double eccentricity, double esscentricAnomaly)
            {
                if (0.9999f < eccentricity && eccentricity < 1.0001f)
                    return (float)Angle.normalizeRadian(2d * Math.Atan2(Math.Sin(esscentricAnomaly) * Math.Sqrt(1d - (eccentricity * eccentricity)), Math.Cos(esscentricAnomaly) - eccentricity));
                return (float)Angle.normalizeRadian(2d * Math.Atan(Math.Sqrt(Math.Abs((1d + eccentricity) / (1d - eccentricity))) * Math.Tan(esscentricAnomaly / 2d)));

                //https://downloads.rene-schwarz.com/download/M002-Cartesian_State_Vectors_to_Keplerian_Orbit_Elements.pdf
                //return normalizeRadian(2d * Math.Atan2(Math.Sqrt(Math.Abs(1d + eccentricity)) * (Math.Sin(esscentricAnomaly) / 2d), Math.Sqrt(Math.Abs(1d - eccentricity)) * (Math.Cos(esscentricAnomaly) / 2d)));
            }

            private static float getEsscentricAnomalyFromTrueAnomaly(double eccentricity, double trueAnomaly)
            {
                return Angle.normalizeRadian((float)(2d * Math.Atan(Math.Sqrt(Math.Abs((1d - eccentricity) / (1d + eccentricity))) * Math.Tan(trueAnomaly / 2d))));
            }

            private static float getMeanAnomalyFromEsscentricAnomaly(double eccentricity, double esscentricAnomaly)
            {
                return Angle.normalizeRadian((float)(esscentricAnomaly - eccentricity * Math.Sin(esscentricAnomaly)));
            }

            private static float getEsscentricAnomalyFromMeanAnomaly(double eccentricity, double meanAnomaly)
            {
                double estimate = Angle.normalizeRadian((float)meanAnomaly);
                for (int i = 0; i < 10; i++)
                {
                    estimate += (meanAnomaly - getMeanAnomalyFromEsscentricAnomaly(eccentricity, estimate)) / 1.2;

                    //https://downloads.rene-schwarz.com/download/M002-Cartesian_State_Vectors_to_Keplerian_Orbit_Elements.pdf
                    //estimate = normalizeRadian(estimate - (((estimate - (eccentricity * Math.Sin(estimate))) - meanAnomaly) / (estimate - (eccentricity * Math.Cos(estimate)))));
                }
                return Angle.normalizeRadian((float)estimate);
            }
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
                eccentricity = Math.Abs(Math.Sqrt(((2d * specificEnergy) / mu) + 2d) - 1d); // Incorrect for exponent 1
                //eccentricity = Math.Abs(Math.Sqrt(1d + ((2d * specificEnergy) / mu)) - 1d);
                //eccentricity = Math.Sqrt(1d + Math.Abs(((2d * specificEnergy) / mu)));
                //semiMajorRadius = radius * Math.Abs(mu / (2d * specificEnergy));
                semiMajorRadius = angularMomemntum / (Math.Sqrt(mu) * (1d - (eccentricity * eccentricity)));
            }
            else
            {
                //https://web.archive.org/web/20160418175843/https://ccar.colorado.edu/asen5070/handouts/cart2kep2002.pdf
                specificEnergy = (speed * speed) / 2d - (mu / radius);
                semiMajorRadius = Math.Abs(mu / (2d * specificEnergy));
                eccentricity = Math.Sqrt(Math.Abs(1d + (2d * specificEnergy * angularMomemntum * angularMomemntum) / (mu * mu)));
            }
            var inclinationAngle = Angle.normalizeRadian(Math.Acos(orbitalMomentum.z/angularMomemntum)) % Math.PI;
            var ascendingAngle = Angle.normalizeRadian(Math.Atan2(orbitalMomentum.x, -1f * orbitalMomentum.y));
            var latitudeAngle = Angle.normalizeRadian(Math.Atan2(startPosition.z / Math.Sin(inclinationAngle), (startPosition.x * Math.Cos(ascendingAngle)) + (startPosition.y * Math.Sin(ascendingAngle))));

            var semiAxisRectum = Ellipse.getAxisRectum((float)semiMajorRadius, (float)eccentricity);
            double trueAnomaly;
            if (parent.exponent < 1.5d)
            {
                trueAnomaly = Angle.normalizeRadian(Math.Atan2(Math.Sqrt(1d / mu) * product, semiAxisRectum - radius));
            }
            else
            {
                trueAnomaly = Angle.normalizeRadian(Math.Atan2(Math.Sqrt(semiAxisRectum / mu) * product, semiAxisRectum - radius));
            }
            var periapseAngle = Angle.normalizeRadian(latitudeAngle - trueAnomaly);

            return KeplerCoordinates.fromTrueAnomaly((float)eccentricity, (float)semiMajorRadius, Angle.toDegrees((float)inclinationAngle), Angle.toDegrees((float)periapseAngle), Angle.toDegrees((float)ascendingAngle), Angle.toDegrees((float)trueAnomaly));
        }

        //https://space.stackexchange.com/questions/19322/converting-orbital-elements-to-cartesian-state-vectors
        //https://web.archive.org/web/20170810015111/http://ccar.colorado.edu/asen5070/handouts/kep2cart_2002.doc
        //https://downloads.rene-schwarz.com/download/M001-Keplerian_Orbit_Elements_to_Cartesian_State_Vectors.pdf
        public static Tuple<Vector3, Vector3> toCartesian(Gravity parent, float timeSinceStart, KeplerCoordinates keplerCoordinates)
        {
            keplerCoordinates = KeplerCoordinates.fromTimeSincePeriapsis(parent, keplerCoordinates.eccentricity, keplerCoordinates.semiMajorRadius, keplerCoordinates.inclinationAngle, keplerCoordinates.periapseAngle, keplerCoordinates.ascendingAngle, keplerCoordinates.getTimeSincePeriapsis(parent) + timeSinceStart);
            return toCartesianTrueAnomaly(parent, keplerCoordinates.eccentricity, keplerCoordinates.semiMajorRadius, keplerCoordinates.inclinationAngle, keplerCoordinates.periapseAngle, keplerCoordinates.ascendingAngle, keplerCoordinates.trueAnomaly); 
        }

        public static Tuple<Vector3, Vector3> toCartesianEsscentricAnomaly(Gravity parent, float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float esscentricAnomaly)
        {
            var keplerCoordinates = KeplerCoordinates.fromEccentricAnomaly(eccentricity, semiMajorRadius, inclinationAngle, periapseAngle, ascendingAngle, esscentricAnomaly);
            return toCartesianTrueAnomaly(parent, keplerCoordinates.eccentricity, keplerCoordinates.semiMajorRadius, keplerCoordinates.inclinationAngle, keplerCoordinates.periapseAngle, keplerCoordinates.ascendingAngle, keplerCoordinates.trueAnomaly);
        }

        public static Tuple<Vector3, Vector3> toCartesianTrueAnomaly(Gravity parent, float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float trueAnomaly)
        {
            inclinationAngle = Angle.toRadian(inclinationAngle);
            periapseAngle = Angle.toRadian(periapseAngle);
            ascendingAngle = Angle.toRadian(ascendingAngle);
            trueAnomaly = Angle.toRadian(trueAnomaly);

            var semiAxisRectum = Ellipse.getAxisRectum(semiMajorRadius, eccentricity);
            var radius = semiAxisRectum / (1.0 + eccentricity * Math.Cos(trueAnomaly));

            var sinsAscend = Math.Sin(ascendingAngle); // sun O
            var cosAscend = Math.Cos(ascendingAngle); // cos O
            var sinPeriapse = Math.Sin(periapseAngle); // sin w
            var cosPeriapse = Math.Cos(periapseAngle); // cos w
            var sinInclination = Math.Sin(inclinationAngle); // sin i
            var cosInclination = Math.Cos(inclinationAngle); // cos i

            var oX = radius * Math.Cos(trueAnomaly);
            var oY = radius * Math.Sin(trueAnomaly);

            double od;
            if (parent.exponent < 1.5f)
            {
                od = Math.Sqrt(parent.mu); // TODO: Figure out why exponent 1 doesn't work
            }
            else
            {
                od = Math.Sqrt(parent.mu / (semiAxisRectum));
            }
            var odX = od * -1d * Math.Sin(trueAnomaly);
            var odY = od * (eccentricity + Math.Cos(trueAnomaly));

            var X = (oX * ((cosPeriapse * cosAscend) - (sinPeriapse * cosInclination * sinsAscend)) - oY * ((sinPeriapse * cosAscend) + (cosPeriapse * cosInclination * sinsAscend)));
            var Y = (oX * ((cosPeriapse * sinsAscend) + (sinPeriapse * cosInclination * cosAscend)) - oY * ((sinPeriapse * sinsAscend) - (cosPeriapse * cosInclination * cosAscend)));
            var Z = ((oX * sinPeriapse * sinInclination) + (oY * cosPeriapse * sinInclination));

            var dX = (odX * ((cosPeriapse * cosAscend) - (sinPeriapse * cosInclination * sinsAscend)) - odY * ((sinPeriapse * cosAscend) + (cosPeriapse * cosInclination * sinsAscend)));
            var dY = (odX * ((cosPeriapse * sinsAscend) + (sinPeriapse * cosInclination * cosAscend)) - odY * ((sinPeriapse * sinsAscend) - (cosPeriapse * cosInclination * cosAscend)));
            var dZ = ((odX * sinPeriapse * sinInclination) + (odY * cosPeriapse * sinInclination));

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
    }
}
