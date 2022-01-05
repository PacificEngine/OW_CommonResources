using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacificEngine.OW_CommonResources.Geometry.Orbits
{
    public class Gravity
    {
        public static float GRAVITATIONAL_CONSTANT { get; } = GravityVolume.GRAVITATIONAL_CONSTANT;

        private const float twoPi = (float)(2d * Math.PI);

        private float _mu = float.NaN;

        public float gravityConstant { get; }
        public float exponent { get; }
        public float mass { get; }
        public float mu { get { if (float.IsNaN(_mu)) _mu = Math.Abs(gravityConstant * mass); return _mu; } }
        public bool isStatic { get; }

        public Gravity(float gravityConstant, float exponent, float mass, bool isStatic = false)
        {
            this.gravityConstant = gravityConstant;
            this.exponent = exponent;
            this.mass = mass;
            this.isStatic = isStatic;
        }

        public static Gravity of(float exponent, float mass, bool isStatic = false)
        {
            return new Gravity(GravityVolume.GRAVITATIONAL_CONSTANT, exponent, mass, isStatic);
        }

        public float getPeriod(float semiMajorRadius)
        {
            return twoPi / (float)Math.Sqrt(Math.Abs(mu / ((float)Math.Pow(semiMajorRadius, exponent + 1f))));
        }

        public float getAngularMomentum(float semiAxisRectum)
        {
            return (float)Math.Sqrt(Math.Abs(mu * ((float)Math.Pow(semiAxisRectum, 3f - exponent))));
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
