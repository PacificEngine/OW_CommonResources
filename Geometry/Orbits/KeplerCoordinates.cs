using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacificEngine.OW_CommonResources.Geometry.Orbits
{
    public class KeplerCoordinates
    {
        private const float twoPi = (float)(2d * Math.PI);

        public static KeplerCoordinates zero { get; } = new KeplerCoordinates(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

        private float _semiMinorRadius = float.NaN;
        private float _foci = float.NaN;
        private float _trueAnomaly = float.NaN;
        private float _esccentricAnomaly = float.NaN;
        private float _meanAnomaly = float.NaN;

        public float eccentricity { get; }
        public float semiMajorRadius { get; }
        public float inclinationAngle { get; }
        public float periapseAngle { get; }
        public float ascendingAngle { get; }

        public float semiMinorRadius { get { if (float.IsNaN(_semiMinorRadius)) _semiMinorRadius = Ellipse.getMinorRadius(semiMajorRadius, foci); return _semiMinorRadius; } }
        public float foci { get { if (float.IsNaN(_foci)) _foci = Ellipse.getFocus(semiMajorRadius, eccentricity); return _foci; } }
        public float apogee { get { return semiMajorRadius + foci; } }
        public float perigee { get { return semiMajorRadius - foci; } }
        public float trueAnomaly { get { 
                if (float.IsNaN(_trueAnomaly)) 
                    _trueAnomaly = Angle.toDegrees(getTrueAnomalyFromEsscentricAnomaly(eccentricity, Angle.toRadian(esccentricAnomaly)));
                return _trueAnomaly; } }
        public float esccentricAnomaly { get { 
                if (float.IsNaN(_esccentricAnomaly)) 
                    _esccentricAnomaly = Angle.toDegrees(!float.IsNaN(_trueAnomaly)
                        ? getEsscentricAnomalyFromTrueAnomaly(eccentricity, Angle.toRadian(_trueAnomaly))
                        : getEsscentricAnomalyFromMeanAnomaly(eccentricity, Angle.toRadian(_meanAnomaly))); 
                return _esccentricAnomaly; } }
        public float meanAnomaly { get { 
                if (float.IsNaN(_meanAnomaly))
                    _meanAnomaly = Angle.toDegrees(getMeanAnomalyFromEsscentricAnomaly(eccentricity, Angle.toRadian(esccentricAnomaly))); 
                return _meanAnomaly; } }

        private KeplerCoordinates(float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float trueAnomaly, float esccentricAnomaly, float meanAnomaly)
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
            if (!float.IsNaN(trueAnomaly))
            {
                _trueAnomaly = Angle.normalizeDegrees(trueAnomaly);
            }
            else if (!float.IsNaN(esccentricAnomaly))
            {
                _esccentricAnomaly = Angle.normalizeDegrees(esccentricAnomaly);
            }
            else
            {
                _meanAnomaly = Angle.normalizeDegrees(meanAnomaly);
            }
        }

        public static KeplerCoordinates fromTrueAnomaly(float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float trueAnomaly)
        {
            return new KeplerCoordinates(eccentricity, semiMajorRadius, inclinationAngle, periapseAngle, ascendingAngle, trueAnomaly, float.NaN, float.NaN);
        }

        public static KeplerCoordinates setTrueAnomaly(KeplerCoordinates coordinates, float trueAnomaly)
        {
            return fromMeanAnomaly(coordinates.eccentricity, coordinates.semiMajorRadius, coordinates.inclinationAngle, coordinates.periapseAngle, coordinates.ascendingAngle, trueAnomaly);
        }

        public static KeplerCoordinates fromEccentricAnomaly(float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float eccentricAnomaly)
        {
            return new KeplerCoordinates(eccentricity, semiMajorRadius, inclinationAngle, periapseAngle, ascendingAngle, float.NaN, eccentricAnomaly, float.NaN);
        }

        public static KeplerCoordinates setEccentricAnomaly(KeplerCoordinates coordinates, float eccentricAnomaly)
        {
            return fromMeanAnomaly(coordinates.eccentricity, coordinates.semiMajorRadius, coordinates.inclinationAngle, coordinates.periapseAngle, coordinates.ascendingAngle, eccentricAnomaly);
        }

        public static KeplerCoordinates fromMeanAnomaly(float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float meanAnomaly)
        {
            return new KeplerCoordinates(eccentricity, semiMajorRadius, inclinationAngle, periapseAngle, ascendingAngle, float.NaN, float.NaN, meanAnomaly);
        }

        public static KeplerCoordinates setMeanAnomaly(KeplerCoordinates coordinates, float meanAnomaly)
        {
            return fromMeanAnomaly(coordinates.eccentricity, coordinates.semiMajorRadius, coordinates.inclinationAngle, coordinates.periapseAngle, coordinates.ascendingAngle, meanAnomaly);
        }

        public static KeplerCoordinates fromTimeSincePeriapsis(Gravity gravity, float eccentricity, float semiMajorRadius, float inclinationAngle, float periapseAngle, float ascendingAngle, float timeSincePeriapsis)
        {
            var meanAnomaly = Angle.toDegrees((twoPi * timeSincePeriapsis) / gravity.getPeriod(semiMajorRadius));

            return fromMeanAnomaly(eccentricity, semiMajorRadius, inclinationAngle, periapseAngle, ascendingAngle, meanAnomaly);
        }

        public static KeplerCoordinates setTimeSincePeriapsis(Gravity gravity, KeplerCoordinates coordinates, float time)
        {
            return fromTimeSincePeriapsis(gravity, coordinates.eccentricity, coordinates.semiMajorRadius, coordinates.inclinationAngle, coordinates.periapseAngle, coordinates.ascendingAngle, time);
        }

        public static KeplerCoordinates shiftTimeSincePeriapsis(Gravity gravity, KeplerCoordinates coordinates, float timeToShift)
        {
            var time = coordinates.getTimeSincePeriapsis(gravity) + timeToShift;

            return setTimeSincePeriapsis(gravity, coordinates, time);
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
                    && trueAnomaly == obj.trueAnomaly;
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
                + (trueAnomaly.GetHashCode() * 16384);
        }

        public bool isOrbit()
        {
            return
                !float.IsNaN(eccentricity) && !float.IsInfinity(eccentricity) && 0f <= eccentricity && eccentricity <= 1f
                    && !float.IsNaN(semiMajorRadius) && !float.IsInfinity(semiMajorRadius)
                    && !float.IsNaN(inclinationAngle) && !float.IsInfinity(inclinationAngle) && 0f <= inclinationAngle && inclinationAngle <= 180f
                    && !float.IsNaN(periapseAngle) && !float.IsInfinity(periapseAngle) && 0f <= periapseAngle && periapseAngle <= 360f
                    && !float.IsNaN(ascendingAngle) && !float.IsInfinity(ascendingAngle) && 0f <= ascendingAngle && ascendingAngle <= 360f
                    && (
                        !float.IsNaN(_trueAnomaly) && !float.IsInfinity(_trueAnomaly) && 0f <= _trueAnomaly && _trueAnomaly <= 360f
                       || !float.IsNaN(_esccentricAnomaly) && !float.IsInfinity(_esccentricAnomaly) && 0f <= _esccentricAnomaly && _esccentricAnomaly <= 360f
                       || !float.IsNaN(_meanAnomaly) && !float.IsInfinity(_meanAnomaly) && 0f <= _meanAnomaly && _meanAnomaly <= 360f);
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
            double guess;
            for(int i = 0; i < 1000; i++)
            {
                guess = getMeanAnomalyFromEsscentricAnomaly(eccentricity, estimate);
                if (Math.Abs(meanAnomaly - guess) < 0.00000001d)
                {
                    break;
                }

                estimate += (meanAnomaly - guess) / 1.2;

                //https://downloads.rene-schwarz.com/download/M002-Cartesian_State_Vectors_to_Keplerian_Orbit_Elements.pdf
                //estimate = Angle.normalizeRadian(estimate - (((estimate - (eccentricity * Math.Sin(estimate))) - meanAnomaly) / (estimate - (eccentricity * Math.Cos(estimate)))));
            }
            return Angle.normalizeRadian((float)estimate);
        }

        public KeplerCoordinates getPeriapsis()
        {
            return KeplerCoordinates.setEccentricAnomaly(this, 0f);
        }

        public KeplerCoordinates getDecending()
        {
            return KeplerCoordinates.setTrueAnomaly(this, 90f);
        }

        public KeplerCoordinates getSemiMinorDecending()
        {
            return KeplerCoordinates.setEccentricAnomaly(this, 90f);
        }

        public KeplerCoordinates getApoapsis()
        {
            return KeplerCoordinates.setEccentricAnomaly(this, 180f);
        }

        public KeplerCoordinates getAscending()
        {
            return KeplerCoordinates.setTrueAnomaly(this, 270f);
        }

        public KeplerCoordinates getSemiMinorAscending()
        {
            return KeplerCoordinates.setEccentricAnomaly(this, 270f);
        }
    }
}
