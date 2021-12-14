using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacificEngine.OW_CommonResources.Geometry.Orbits
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
}
