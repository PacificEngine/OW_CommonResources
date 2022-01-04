using PacificEngine.OW_CommonResources.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PacificEngine.OW_CommonResources.Geometry
{
    public class Ellipse
    {
        private float _semiMajorRadius = float.NaN; // a
        private float _semiMinorRadius = float.NaN; // b
        private float _semiLatusRectum = float.NaN; // l
        private float _eccentricityRoot = float.NaN; // (1 - e^2)^(1/2)
        private float _eccentricity = float.NaN; // e
        private float _foci = float.NaN; // f
        private float _apogee = float.NaN; // r_max
        private float _perigee = float.NaN; // r_min

        private float eccentricityRoot { get { if (float.IsNaN(_eccentricityRoot)) _eccentricityRoot = (float)Math.Abs(Math.Sqrt(Math.Abs(1 - (eccentricity * eccentricity)))); return _eccentricityRoot; } }

        public float semiMajorRadius { get { if (float.IsNaN(_semiMajorRadius)) _semiMajorRadius = Math.Abs(getSemiMajorRadius()); return _semiMajorRadius; } }
        public float semiMinorRadius { get { if (float.IsNaN(_semiMinorRadius)) _semiMinorRadius = Math.Abs(getSemiMinorRadius()); return _semiMinorRadius; } }
        public float semiLatusRectum { get { if (float.IsNaN(_semiLatusRectum)) _semiLatusRectum = Math.Abs(getSemiLatusRectum()); return _semiLatusRectum; } }
        public float eccentricity { get { if (float.IsNaN(_eccentricity)) _eccentricity = Math.Abs(getEccentricity()); return _eccentricity; } }
        public float foci { get { if (float.IsNaN(_foci)) _foci = Math.Abs(getFoci()); return _foci; } }
        public float apogee { get { if (float.IsNaN(_apogee)) _apogee = Math.Abs(getApogee()); return _apogee; } }
        public float perigee { get { if (float.IsNaN(_perigee)) _perigee = Math.Abs(getPerigee()); return _perigee; } }

        private Ellipse(float semiMajorRadius, float semiMinorRadius, float semiLatusRectum, float eccentricity, float foci, float apogee, float perigee)
        {
            this._semiMajorRadius = float.IsNaN(semiMajorRadius) ? float.NaN : Math.Abs(semiMajorRadius);
            this._semiMinorRadius = float.IsNaN(semiMinorRadius) ? float.NaN : Math.Abs(semiMinorRadius);
            this._semiLatusRectum = float.IsNaN(semiLatusRectum) ? float.NaN : Math.Abs(semiLatusRectum);
            this._eccentricity = float.IsNaN(eccentricity) ? float.NaN : Math.Abs(eccentricity);
            this._foci = float.IsNaN(foci) ? float.NaN : Math.Abs(foci);
            this._apogee = float.IsNaN(apogee) ? float.NaN : Math.Abs(apogee);
            this._perigee = float.IsNaN(perigee) ? float.NaN : Math.Abs(perigee);

            if (!float.IsNaN(_semiMajorRadius) 
                && !float.IsNaN(_semiMinorRadius)
                && _semiMajorRadius < _semiMinorRadius)
            {
                _semiMajorRadius = Math.Abs(semiMinorRadius);
                _semiMinorRadius = Math.Abs(semiMajorRadius);
            }

            if (!float.IsNaN(_apogee)
                && !float.IsNaN(_perigee)
                && _apogee < _perigee)
            {
                _apogee = Math.Abs(apogee);
                _perigee = Math.Abs(perigee);
            }
        }

        public static Ellipse fromMajorRadiusAndMinorRadius(float semiMajorRadius, float semiMinorRadius)
        {
            return new Ellipse(semiMajorRadius, semiMinorRadius, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN);
        }

        public static Ellipse fromMajorRadiusAndLatusRectum(float semiMajorRadius, float semiLatusRectum)
        {
            return new Ellipse(semiMajorRadius, float.NaN, semiLatusRectum, float.NaN, float.NaN, float.NaN, float.NaN);
        }

        public static Ellipse fromMajorRadiusAndEccentricity(float semiMajorRadius, float eccentricity)
        {
            return new Ellipse(semiMajorRadius, float.NaN, float.NaN, eccentricity, float.NaN, float.NaN, float.NaN);
        }

        public static Ellipse fromMajorRadiusAndFoci(float semiMajorRadius, float foci)
        {
            return new Ellipse(semiMajorRadius, float.NaN, float.NaN, float.NaN, foci, float.NaN, float.NaN);
        }

        public static Ellipse fromMajorRadiusAndApogee(float semiMajorRadius, float apogee)
        {
            return new Ellipse(semiMajorRadius, float.NaN, float.NaN, float.NaN, float.NaN, apogee, float.NaN);
        }

        public static Ellipse fromMajorRadiusAndPerigee(float semiMajorRadius, float perigee)
        {
            return new Ellipse(semiMajorRadius, float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, perigee);
        }

        public static Ellipse fromMinorRadiusAndLatusRectum(float semiMinorRadius, float semiLatusRectum)
        {
            return new Ellipse(float.NaN, semiMinorRadius, semiLatusRectum, float.NaN, float.NaN, float.NaN, float.NaN);
        }

        public static Ellipse fromMinorRadiusAndEccentricity(float semiMinorRadius, float eccentricity)
        {
            return new Ellipse(float.NaN, semiMinorRadius, float.NaN, eccentricity, float.NaN, float.NaN, float.NaN);
        }

        public static Ellipse fromMinorRadiusAndFoci(float semiMinorRadius, float foci)
        {
            return new Ellipse(float.NaN, semiMinorRadius, float.NaN, float.NaN, foci, float.NaN, float.NaN);
        }

        /* TODO
        public static Ellipse fromMinorRadiusAndApogee(float semiMinorRadius, float apogee)
        {
            return new Ellipse(float.NaN, semiMinorRadius, float.NaN, float.NaN, float.NaN, apogee, float.NaN);
        }

        public static Ellipse fromMinorRadiusAndPerigee(float semiMinorRadius, float perigee)
        {
            return new Ellipse(float.NaN, semiMinorRadius, float.NaN, float.NaN, float.NaN, float.NaN, perigee);
        }
        */

        public static Ellipse fromLatusRectumAndEccentricity(float semiLatusRectum, float eccentricity)
        {
            return new Ellipse(float.NaN, float.NaN, semiLatusRectum, eccentricity, float.NaN, float.NaN, float.NaN);
        }


        /* TODO
        public static Ellipse fromLatusRectumAndFoci(float semiLatusRectum, float foci)
        {
            return new Ellipse(float.NaN, float.NaN, semiLatusRectum, float.NaN, foci, float.NaN, float.NaN);
        }
        */

        public static Ellipse fromLatusRectumAndApogee(float semiLatusRectum, float apogee)
        {
            return new Ellipse(float.NaN, float.NaN, semiLatusRectum, float.NaN, float.NaN, apogee, float.NaN);
        }

        public static Ellipse fromLatusRectumAndPerigee(float semiLatusRectum, float perigee)
        {
            return new Ellipse(float.NaN, float.NaN, semiLatusRectum, float.NaN, float.NaN, float.NaN, perigee);
        }

        public static Ellipse fromEccentricityAndFoci(float eccentricity, float foci)
        {
            if (eccentricity <= 0f && foci <= 0f)
            {
                throw new ArgumentException("No valid ellipse with both eccentricity and foci set as zero");
            }

            return new Ellipse(float.NaN, float.NaN, float.NaN, eccentricity, foci, float.NaN, float.NaN);
        }

        public static Ellipse fromEccentricityAndApogee(float eccentricity, float apogee)
        {
            return new Ellipse(float.NaN, float.NaN, float.NaN, eccentricity, float.NaN, apogee, float.NaN);
        }

        public static Ellipse fromEccentricityAndPerigee(float eccentricity, float perigee)
        {
            return new Ellipse(float.NaN, float.NaN, float.NaN, eccentricity, float.NaN, float.NaN, perigee);
        }

        public static Ellipse fromFociAndApogee(float foci, float apogee)
        {
            return new Ellipse(float.NaN, float.NaN, float.NaN, float.NaN, foci, apogee, float.NaN);
        }

        public static Ellipse fromFociAndPerigee(float foci, float perigee)
        {
            return new Ellipse(float.NaN, float.NaN, float.NaN, float.NaN, foci, float.NaN, perigee);
        }

        public static Ellipse fromApogeeAndPerigee(float apogee, float perigee)
        {
            return new Ellipse(float.NaN, float.NaN, float.NaN, float.NaN, float.NaN, apogee, perigee);
        }

        private float getSemiMajorRadius()
        {
            // TODO
            // _foci & _semiLatusRectum (Could use pathaogrian)
            // _apogee & _semiMinorRadius
            // _perigee & _semiMinorRadius

            if (!float.IsNaN(_semiLatusRectum) && !float.IsNaN(_semiMinorRadius))
            {
                // a = bb / l
                return (_semiMinorRadius * _semiMinorRadius) / _semiLatusRectum;
            }
            else if (!float.IsNaN(_eccentricity) && !float.IsNaN(_semiLatusRectum))
            {
                // a = l / (1 - ee)
                return _semiLatusRectum / (1 - (_eccentricity * _eccentricity));
            }
            else if (!float.IsNaN(_foci) && !float.IsNaN(_eccentricity))
            {
                // a = f / e
                return _foci / _eccentricity;
            }
            else if (!float.IsNaN(_apogee) && !float.IsNaN(_eccentricity))
            {
                // a = r_max / (1+e)
                return _apogee / (1f + _eccentricity);
            }
            else if (!float.IsNaN(_apogee) && !float.IsNaN(_foci))
            {
                // a = r_max - f
                return _apogee - _foci;
            }
            else if (!float.IsNaN(_perigee) && !float.IsNaN(_eccentricity))
            {
                // a = r_min / (1-e)
                return _perigee / (1f - _eccentricity);
            }
            else if (!float.IsNaN(_perigee) && !float.IsNaN(_foci))
            {
                // a = f + r_min
                return _foci + _perigee;
            }
            else if (!float.IsNaN(_perigee) && !float.IsNaN(_apogee))
            {
                // a = (r_min + r_max) / 2
                return (_perigee + _apogee) / 2f;
            }

            // Force Solve of other variables
            if (!float.IsNaN(_apogee) && !float.IsNaN(_semiLatusRectum))
            {
                return _apogee - foci;
            }
            else if (!float.IsNaN(_perigee) && !float.IsNaN(_semiLatusRectum))
            {
                return foci + _perigee;
            }

            // Requires eccentricityRoot
            if (!float.IsNaN(_eccentricity) && !float.IsNaN(_semiMinorRadius))
            {
                return _semiMinorRadius / eccentricityRoot;
            }

            // Requires Math.Sqrt
            if (!float.IsNaN(_foci) && !float.IsNaN(_semiMinorRadius))
            {
                return (float)Math.Sqrt(Math.Abs((_semiMinorRadius * _semiMinorRadius) + (_foci * _foci)));
            }

            Helper.helper.Console.WriteLine($"Unable to Solve Semi-Major Radius for Ellipse {this.ToString()}", OWML.Common.MessageType.Warning);
            return 0f;
        }

        private float getSemiMinorRadius()
        {
            // TODO
            // _foci & _semiLatusRectum 

            // _apogee & _semiMajorRadius
            // _apogee & _semiLatusRectum
            // _apogee & _eccentricity
            // _apogee & _foci

            // _perigee & _semiMajorRadius
            // _perigee & _semiLatusRectum
            // _perigee & _eccentricity
            // _perigee & _foci


            // Requires eccentricityRoot
            if (!float.IsNaN(_eccentricity) && !float.IsNaN(_semiMajorRadius))
            {
                // b = a * (1 - ee)^(1/2)
                return _semiMajorRadius * eccentricityRoot;
            }
            else if (!float.IsNaN(_eccentricity) && !float.IsNaN(_semiLatusRectum))
            {
                // b = l / (1 - ee)^(1/2)
                return _semiLatusRectum / eccentricityRoot;
            }

            // Force Solve of other variables (eccentricityRoot)
            if (!float.IsNaN(_foci) && !float.IsNaN(_eccentricity))
            {
                return semiMajorRadius * eccentricityRoot;
            }

            // Requires Math.Sqrt
            if (!float.IsNaN(_semiLatusRectum) && !float.IsNaN(_semiMajorRadius))
            {
                // b = (la)^(1/2)
                return (float)Math.Sqrt(Math.Abs((_semiLatusRectum * _semiMajorRadius)));
            }
            else if (!float.IsNaN(_foci) && !float.IsNaN(_semiMajorRadius))
            {
                // b = (aa - ff)^(1/2)
                return (float)Math.Sqrt(Math.Abs((_semiMajorRadius * _semiMajorRadius) - (foci * foci)));
            }
            else if (!float.IsNaN(_perigee) && !float.IsNaN(_apogee))
            {
                // b = (r_min * r_max)^(1/2)
                return (float)Math.Sqrt(Math.Abs(_perigee * _apogee));
            }

            return semiMajorRadius * eccentricityRoot;
        }

        private float getSemiLatusRectum()
        {
            // TODO
            // _foci & _semiMinorRadius
            // _apogee & _semiMajorRadius
            // _apogee & _semiMinorRadius
            // _apogee & _foci
            // _perigee & _semiMajorRadius
            // _perigee & _semiMinorRadius
            // _perigee & _foci

            if (!float.IsNaN(_semiMinorRadius) && !float.IsNaN(_semiMajorRadius))
            {
                // l = bb / a
                return (_semiMinorRadius * _semiMinorRadius) / _semiMajorRadius;
            }
            else if (!float.IsNaN(_eccentricity) && !float.IsNaN(_semiMajorRadius))
            {
                // l = a * (1 - ee)
                return _semiMajorRadius * (1f - _eccentricity * _eccentricity);
            }
            else if (!float.IsNaN(_apogee) && !float.IsNaN(_eccentricity))
            {
                // l = r_max (1 - e)
                return _apogee * (1f - _eccentricity);
            }
            else if (!float.IsNaN(_perigee) && !float.IsNaN(_eccentricity))
            {
                // l = r_min (1 + e)
                return _perigee * (1f + _eccentricity);
            }
            else if (!float.IsNaN(_perigee) && !float.IsNaN(_apogee))
            {
                // l = 2 * r_min * r_max / (r_max + r_min)
                return (2f * _perigee * _apogee) / (_perigee + _apogee);
            }

            // Force Solve of other variables
            if (!float.IsNaN(_foci) && !float.IsNaN(_semiMajorRadius))
            {
                return _semiMajorRadius * (1f - (eccentricity * eccentricity));
            }
            else if (!float.IsNaN(_foci) && !float.IsNaN(_eccentricity))
            {
                return semiMajorRadius * (1f - (_eccentricity * _eccentricity));
            }

            // Requires eccentricityRoot
            if (!float.IsNaN(_eccentricity) && !float.IsNaN(_semiMinorRadius))
            {
                // l = b * (1 - ee) ^ (1/2)
                return _semiMinorRadius * eccentricityRoot;
            }

            return (semiMinorRadius * semiMinorRadius) / semiMajorRadius;
        }

        private float getEccentricity()
        {
            // TODO
            // _foci & _semiMinorRadius
            // _foci & _semiLatusRectum
            // _apogee & _semiMinorRadius
            // _perigee & _semiMinorRadius

            if (!float.IsNaN(_foci) && !float.IsNaN(_semiMajorRadius))
            {
                // e = f / a
                return _foci / _semiMajorRadius;
            }
            else if (!float.IsNaN(_apogee) && !float.IsNaN(_semiMajorRadius))
            {
                // e = r_max/a - 1
                return (_apogee / _semiMajorRadius) - 1f;
            }
            else if (!float.IsNaN(_apogee) && !float.IsNaN(_semiLatusRectum))
            {
                // e = 1 - l/r_max
                return 1f - (_semiLatusRectum / _apogee);
            }
            else if (!float.IsNaN(_perigee) && !float.IsNaN(_semiMajorRadius))
            {
                // e = 1 - r_min/a
                return 1f - (_perigee / _semiMajorRadius);
            }
            else if (!float.IsNaN(_perigee) && !float.IsNaN(_semiLatusRectum))
            {
                // e = l/r_min - 1
                return (_semiLatusRectum / _perigee) - 1f;
            }

            // Force Solve of other variables
            if (!float.IsNaN(_apogee) && !float.IsNaN(_foci))
            {
                return _foci / semiMajorRadius;
            }
            else if (!float.IsNaN(_perigee) && !float.IsNaN(_foci))
            {
                return _foci / semiMajorRadius;
            }
            else if (!float.IsNaN(_perigee) && !float.IsNaN(_apogee))
            {
                return foci / semiMajorRadius;
            }

            // Requires Math.Sqrt
            if (!float.IsNaN(_semiMinorRadius) && !float.IsNaN(_semiMajorRadius))
            {
                // e = (1 - bb/aa)^(1/2)
                return (float)Math.Sqrt(Math.Abs(1f - ((_semiMinorRadius * _semiMinorRadius) / (_semiMajorRadius * _semiMajorRadius))));
            }
            else if (!float.IsNaN(_semiLatusRectum) && !float.IsNaN(_semiMajorRadius))
            {
                // e = (1 - l/a)^(1/2)
                return (float)Math.Sqrt(Math.Abs(1f - (_semiLatusRectum / _semiMajorRadius)));
            }
            else if (!float.IsNaN(_semiLatusRectum) && !float.IsNaN(_semiMinorRadius))
            {
                // e = (1 - ll/bb)^(1/2)
                return (float)Math.Sqrt(Math.Abs(1f - ((_semiLatusRectum * _semiLatusRectum) / (_semiMinorRadius * _semiMinorRadius))));
            }

            return foci / semiMajorRadius;
        }

        private float getFoci()
        {
            if (!float.IsNaN(_eccentricity) && !float.IsNaN(_semiMajorRadius))
            {
                // f = ae
                return _semiMajorRadius * _eccentricity;
            }
            else if (!float.IsNaN(_eccentricity) && !float.IsNaN(_semiLatusRectum))
            {
                // f = le / (1 - ee)
                return (_semiLatusRectum * _eccentricity) / (1f - (_eccentricity * _eccentricity));
            }
            else if (!float.IsNaN(_apogee) && !float.IsNaN(_semiMajorRadius))
            {
                // f = r_max - a
                return _apogee - _semiMajorRadius;
            }
            else if (!float.IsNaN(_apogee) && !float.IsNaN(_semiMinorRadius))
            {
                // f = (r_max - (bb / r_max)) / 2
                return (_apogee - ((_semiMajorRadius * _semiMajorRadius) / _apogee)) / 2f;
            }
            else if (!float.IsNaN(_apogee) && !float.IsNaN(_eccentricity))
            {
                // f = (r_max * e) / (1 + e)
                return (_apogee * _eccentricity) / (1f + _eccentricity);
            }
            else if (!float.IsNaN(_perigee) && !float.IsNaN(_semiMajorRadius))
            {
                // f = a - r_min
                return _semiMajorRadius - _perigee;
            }
            else if (!float.IsNaN(_perigee) && !float.IsNaN(_semiMinorRadius))
            {
                // f = ((bb / r_min) - r_min) / 2
                return (((_semiMajorRadius * _semiMajorRadius) / _perigee) - _perigee) / 2f;
            }
            else if (!float.IsNaN(_perigee) && !float.IsNaN(_eccentricity))
            {
                // f = (r_min * e) / (1 - e)
                return (_perigee * _eccentricity) / (1f - _eccentricity);
            }
            else if (!float.IsNaN(_perigee) && !float.IsNaN(_apogee))
            {
                // f = (r_max - r_mix) / 2
                return (_apogee - _perigee) / 2f;
            }

            // Force Solve of other variables
            if (!float.IsNaN(_apogee) && !float.IsNaN(_semiLatusRectum))
            {
                return (_apogee * eccentricity) / (1f + eccentricity);
            }
            else if (!float.IsNaN(_perigee) && !float.IsNaN(_semiLatusRectum))
            {
                return (_perigee * eccentricity) / (1f - eccentricity);
            }

            // Requires eccentricityRoot
            if (!float.IsNaN(_eccentricity) && !float.IsNaN(_semiMinorRadius))
            {
                // f = be / (1 - ee)^(1/2)
                return (_semiMinorRadius * _eccentricity) / eccentricityRoot;
            }

            // Requires Math.Sqrt
            if (!float.IsNaN(_semiMinorRadius) && !float.IsNaN(_semiMajorRadius))
            {
                // f = (aa - bb)^(1/2)
                return (float)Math.Sqrt(Math.Abs((_semiMajorRadius * _semiMajorRadius) - (_semiMinorRadius * _semiMinorRadius)));
            }
            else if (!float.IsNaN(_semiLatusRectum) && !float.IsNaN(_semiMajorRadius))
            {
                // f = (aa - al)^(1/2)
                return (float)Math.Sqrt(Math.Abs((_semiMajorRadius * _semiMajorRadius) - (_semiMajorRadius * _semiLatusRectum)));
            }
            else if (!float.IsNaN(_semiLatusRectum) && !float.IsNaN(_semiMinorRadius))
            {
                // f = (bb/ll - 1)^(1/2)
                return _semiMinorRadius * (float)Math.Sqrt(Math.Abs(((_semiMinorRadius * _semiMinorRadius) / (_semiLatusRectum * _semiLatusRectum)) - 1f));
            }

            Helper.helper.Console.WriteLine($"Unable to Solve Foci for Ellipse {this.ToString()}", OWML.Common.MessageType.Warning);
            return 0f;
        }

        private float getApogee()
        {
            if (!float.IsNaN(_eccentricity) && !float.IsNaN(_semiMajorRadius))
            {
                // r_max = a(1 + e)
                return _semiMajorRadius * (1 + _eccentricity);
            }
            else if (!float.IsNaN(_eccentricity) && !float.IsNaN(_semiLatusRectum))
            {
                // r_max = l / (1 - e)
                return _semiLatusRectum / (1 - _eccentricity);
            }
            else if (!float.IsNaN(_foci) && !float.IsNaN(_semiMajorRadius))
            {
                // r_min = a + f
                return _semiMajorRadius + _foci;
            }
            else if (!float.IsNaN(_perigee) && !float.IsNaN(_semiMajorRadius))
            {
                // r_max = 2a - r_min
                return (2f * _semiMajorRadius) - _perigee;
            }
            else if (!float.IsNaN(_perigee) && !float.IsNaN(_semiMinorRadius))
            {
                // r_max = bb / r_min
                return (_semiMinorRadius * _semiMinorRadius) / _perigee;
            }
            else if (!float.IsNaN(_perigee) && !float.IsNaN(_foci))
            {
                // r_max = 2f + r_min
                return _perigee + 2f * _foci;
            }

            // Force Solve of other variables
            if (!float.IsNaN(_perigee) && !float.IsNaN(_semiLatusRectum))
            {
                return _perigee + 2f * foci;
            }
            else if (!float.IsNaN(_perigee) && !float.IsNaN(_eccentricity))
            {
                return _perigee + 2f * foci;
            }
            else if (!float.IsNaN(_foci) && !float.IsNaN(_eccentricity))
            {
                return semiMajorRadius + _foci;
            }

            // Requires eccentricityRoot
            if (!float.IsNaN(_eccentricity) && !float.IsNaN(_semiMinorRadius))
            {
                // r_max = (b(1 + e)) / (1 - ee)^(1/2)
                return (_semiMinorRadius * (1 + _eccentricity)) / eccentricityRoot;
            }

            // Force Solve of other variables (Math.Sqrt)
            if (!float.IsNaN(_semiMinorRadius) && !float.IsNaN(_semiMajorRadius))
            {
                return _semiMajorRadius + foci;
            }
            else if (!float.IsNaN(_semiLatusRectum) && !float.IsNaN(_semiMajorRadius))
            {
                return _semiMajorRadius + foci;
            }
            else if (!float.IsNaN(_semiLatusRectum) && !float.IsNaN(_semiMinorRadius))
            {
                return semiMajorRadius + foci;
            }
            else if (!float.IsNaN(_foci) && !float.IsNaN(_semiMinorRadius))
            {
                return semiMajorRadius + _foci;
            }
            else if (!float.IsNaN(_foci) && !float.IsNaN(_semiLatusRectum))
            {
                return _semiLatusRectum / (1 - eccentricity);
            }

            Helper.helper.Console.WriteLine($"Unable to Apogee Foci for Ellipse {this.ToString()}", OWML.Common.MessageType.Warning);
            return 0f;
        }

        private float getPerigee()
        {
            if (!float.IsNaN(_eccentricity) && !float.IsNaN(_semiMajorRadius))
            {
                // r_min = a(1 - e)
                return _semiMajorRadius * (1 - _eccentricity);
            }
            else if (!float.IsNaN(_eccentricity) && !float.IsNaN(_semiLatusRectum))
            {
                // r_min = l / (1 + e)
                return _semiLatusRectum / (1 + _eccentricity);
            }
            else if (!float.IsNaN(_foci) && !float.IsNaN(_semiMajorRadius))
            {
                // r_min = a - f
                return _semiMajorRadius - _foci;
            }
            else if (!float.IsNaN(_apogee) && !float.IsNaN(_semiMajorRadius))
            {
                // r_min = 2a - r_max
                return (2f * _semiMajorRadius) - _apogee;
            }
            else if (!float.IsNaN(_apogee) && !float.IsNaN(_semiMinorRadius))
            {
                // r_min = bb / r_max
                return (_semiMinorRadius * _semiMinorRadius) / _apogee;
            }
            else if (!float.IsNaN(_apogee) && !float.IsNaN(_foci))
            {
                // r_min = r_max - 2f
                return _apogee - 2f * _foci;
            }

            // Force Solve of other variables
            if (!float.IsNaN(_apogee) && !float.IsNaN(_semiLatusRectum))
            {
                return _apogee - 2f * foci;
            }
            else if (!float.IsNaN(_apogee) && !float.IsNaN(_eccentricity))
            {
                return _apogee - 2f * foci;
            }
            else if (!float.IsNaN(_foci) && !float.IsNaN(_eccentricity))
            {
                return semiMajorRadius - _foci;
            }

            // Requires eccentricityRoot
            if (!float.IsNaN(_eccentricity) && !float.IsNaN(_semiMinorRadius))
            {
                // r_min = (b(1 - e)) / (1 - ee)^(1/2)
                return (_semiMinorRadius * (1 - _eccentricity)) / eccentricityRoot;
            }

            // Force Solve of other variables (Math.Sqrt)
            if (!float.IsNaN(_semiMinorRadius) && !float.IsNaN(_semiMajorRadius))
            {
                return _semiMajorRadius - foci;
            }
            else if (!float.IsNaN(_semiLatusRectum) && !float.IsNaN(_semiMajorRadius))
            {
                return _semiMajorRadius - foci;
            }
            else if (!float.IsNaN(_semiLatusRectum) && !float.IsNaN(_semiMinorRadius))
            {
                return semiMajorRadius - foci;
            }
            else if (!float.IsNaN(_foci) && !float.IsNaN(_semiMinorRadius))
            {
                return semiMajorRadius - _foci;
            }
            else if (!float.IsNaN(_foci) && !float.IsNaN(_semiLatusRectum))
            {
                return _semiLatusRectum / (1 + eccentricity);
            }

            Helper.helper.Console.WriteLine($"Unable to Perigee Foci for Ellipse {this.ToString()}", OWML.Common.MessageType.Warning);
            return 0f;
        }

        // True Anomaly
        public float getRadiusFromFociAngle(float fociAngleFromPerigee)
        {
            return semiLatusRectum / (1f + eccentricity * (float)Math.Cos(Angle.toRadian(fociAngleFromPerigee)));
        }

        // Eccentric Anomaly
        public float getRadiusFromCenterAngle(float centerAngleFromPerigee)
        {
            return (semiMajorRadius * (1f - Math.Abs(eccentricity * (float)Math.Sin(Angle.toRadian(centerAngleFromPerigee)))));
        }

        // Eccentric Anomaly
        public float getCenterAngle(Vector2 coordinates)
        {
            var polar = 0f;
            if (coordinates.y > coordinates.x)
                polar = (float)Math.Asin(coordinates.y / semiMinorRadius);
            else
                polar = (float)Math.Acos(coordinates.x / semiMajorRadius);
            polar = Angle.toDegrees(polar) % (90f);
            if (coordinates.x > 0 && coordinates.y >= 0) // Right
                return polar;
            if (coordinates.x <= 0 && coordinates.y > 0) // Up
                return polar + 90f;
            if (coordinates.x < 0 && coordinates.y <= 0) // Left
                return polar + 180f;
            else if (coordinates.x >= 0 && coordinates.y < 0) // Down
                return polar + 270f;
            return 0f;
        }

        // Eccentric Anomaly
        public Vector2 getCoordinatesFromCenterAngle(float centerAngleFromPerigee)
        {
            centerAngleFromPerigee = Angle.toRadian(centerAngleFromPerigee);
            return new Vector2(semiMajorRadius * (float)Math.Cos(centerAngleFromPerigee), semiMinorRadius * (float)Math.Sin(centerAngleFromPerigee));
        }

        // Eccentric Anomaly
        public Vector2 getSlopeFromCenterAngle(float centerAngleFromPerigee)
        {
            centerAngleFromPerigee = Angle.toRadian(centerAngleFromPerigee);
            return new Vector2(-1f * semiMajorRadius * (float)Math.Sin(centerAngleFromPerigee), semiMinorRadius * (float)Math.Cos(centerAngleFromPerigee));
        }

        public override string ToString()
        { 
            return $"({Math.Round(_semiMajorRadius, 7).ToString("G9")}, {Math.Round(_semiMinorRadius, 7).ToString("G9")}, {Math.Round(_semiLatusRectum, 7).ToString("G9")}, {Math.Round(_eccentricity, 7).ToString("G9")}, {Math.Round(_foci, 7).ToString("G9")}, {Math.Round(_apogee, 7).ToString("G9")}, {Math.Round(_perigee, 7).ToString("G9")})";
        }

        public override bool Equals(System.Object other)
        {
            if (other != null && other is Ellipse)
            {
                var obj = other as Ellipse;
                return eccentricity == obj.eccentricity
                    && semiMajorRadius == obj.semiMajorRadius;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (eccentricity.GetHashCode() * 4)
                + (semiMajorRadius.GetHashCode() * 16);
        }
    }
}
