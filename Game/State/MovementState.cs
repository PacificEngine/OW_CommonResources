using OWML.Utils;
using PacificEngine.OW_CommonResources.Game.Display;
using PacificEngine.OW_CommonResources.Game.Resource;
using PacificEngine.OW_CommonResources.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PacificEngine.OW_CommonResources.Game.State
{
    public class AbsoluteState
    {
        public Vector3 position { get; }
        public Vector3 velocity { get; }
        public Vector3 acceleration { get; }
        public Quaternion orientation { get; }
        public Vector3 angularVelocity { get; }
        public Vector3 angularAcceleration { get; }

        public AbsoluteState(Vector3 position, Vector3 velocity, Vector3 acceleration,
           Quaternion orientation, Vector3 angularVelocity, Vector3 angularAcceleration)
        {
            this.position = position;
            this.velocity = velocity;
            this.acceleration = acceleration;
            this.orientation = orientation;
            this.angularVelocity = angularVelocity;
            this.angularAcceleration = angularAcceleration;
        }

        public static AbsoluteState fromCurrentState(Position.HeavenlyBodies target)
        {
            return fromCurrentState(Position.getBody(target));
        }

        public static AbsoluteState fromCurrentState(OWRigidbody target)
        {
            if (target == null)
            {
                return null;
            }
            return new AbsoluteState(target.GetPosition(), target.GetVelocity(), target.GetAcceleration(), target.GetRotation(), target.GetAngularVelocity(), target.GetAngularAcceleration());
        }

        public override string ToString()
        {
            var position = this.position == null ? "" : DisplayConsole.logVector(this.position);
            var velocity = this.velocity == null ? "" : DisplayConsole.logVector(this.velocity);
            var acceleration = this.acceleration == null ? "" : DisplayConsole.logVector(this.acceleration);
            var orientation = this.orientation == null ? "" : DisplayConsole.logQuaternion(this.orientation);
            var angularVelocity = this.angularVelocity == null ? "" : DisplayConsole.logVector(this.angularVelocity);
            var angularAcceleration = this.angularAcceleration == null ? "" : DisplayConsole.logVector(this.angularAcceleration);
            return $"({position}, {velocity}, {acceleration}, {orientation}, {angularVelocity}, {angularAcceleration})";
        }

        public override bool Equals(System.Object other)
        {
            if (other != null && other is RelativeState)
            {
                var obj = other as RelativeState;
                return position == obj.position
                    && velocity == obj.velocity
                    && acceleration == obj.acceleration
                    && orientation == obj.orientation
                    && angularVelocity == obj.angularVelocity
                    && angularAcceleration == obj.angularAcceleration;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (position.GetHashCode() >> 2)
                + (velocity.GetHashCode() >> 4)
                + (acceleration.GetHashCode() >> 6)
                + (orientation.GetHashCode() >> 8)
                + (angularVelocity.GetHashCode() >> 10)
                + (angularAcceleration.GetHashCode() >> 12);
        }

        public Vector3 GetPointVelocity(Vector3 worldPoint) => this.GetPointTangentialVelocity(worldPoint) + velocity;

        public Vector3 GetPointTangentialVelocity(Vector3 worldPoint) => Vector3.Cross(angularVelocity, worldPoint - position);

        public Vector3 GetPointAcceleration(Vector3 worldPoint)
        {
            var rhs = worldPoint - position;
            return acceleration + Vector3.Cross(angularVelocity, Vector3.Cross(angularVelocity, rhs)) + Vector3.Cross(angularAcceleration, rhs);
        }

        public Vector3 GetPointTangentialAcceleration(Vector3 worldPoint)
        {
            var rhs = worldPoint - position;
            return Vector3.Cross(angularAcceleration, rhs);
        }

        public Vector3 GetPointCentripetalAcceleration(Vector3 worldPoint)
        {
            return Vector3.Cross(angularVelocity, Vector3.Cross(angularVelocity, worldPoint - position));
        }

        public Vector3 TransformDirection(Vector3 localDirection)
        {
            return orientation * localDirection;
        }

        public Vector3 InverseTransformDirection(Vector3 worldDirection)
        {
            return Quaternion.Inverse(orientation) * worldDirection;
        }

        public Vector3 TransformPoint(Vector3 localPoint)
        {
            return orientation * localPoint + position;
        }

        public Vector3 InverseTransformPoint(Vector3 worldPoint)
        {
            return Quaternion.Inverse(orientation) * (worldPoint - position);
        }

        public Quaternion TransformRotation(Quaternion localPoint)
        {
            return orientation * localPoint;
        }

        public Quaternion InverseTransformRotation(Quaternion worldPoint)
        {
            return Quaternion.Inverse(orientation) * worldPoint;
        }
    }

    public class RelativeState
    {
        public Position.HeavenlyBodies parent { get; }
        public Vector3? position { get; }
        public Vector3? velocity { get; }
        public Vector3? acceleration { get; }
        public Quaternion? orientation { get; }
        public Vector3? angularVelocity { get; }
        public Vector3? angularAcceleration { get; }
        public Vector3? surfacePosition { get; }
        public Vector3? surfaceVelocity { get; }
        public Vector3? surfaceAcceleration { get; }
        public Quaternion? surfraceOrientation { get; }
        public Vector3? surfaceAngularVelocity { get; }
        public Vector3? surfaceAngularAcceleration { get; }
        public Orbit.KeplerCoordinates orbit { get; }


        private RelativeState(Position.HeavenlyBodies parent,
            Vector3? position, Vector3? velocity, Vector3? acceleration,
            Quaternion? orientation, Vector3? angularVelocity, Vector3? angularAcceleration,
            Vector3? surfacePosition, Vector3? surfaceVelocity, Vector3? surfaceAcceleration,
            Quaternion? surfraceOrientation, Vector3? surfaceAngularVelocity, Vector3? surfaceAngularAcceleration,
            Orbit.KeplerCoordinates orbit)
        {
            this.parent = parent;
            this.position = position;
            this.velocity = velocity;
            this.acceleration = acceleration;
            this.orientation = orientation;
            this.angularVelocity = angularVelocity;
            this.angularAcceleration = angularAcceleration;
            this.surfacePosition = surfacePosition;
            this.surfaceVelocity = surfaceVelocity;
            this.surfaceAcceleration = surfaceAcceleration;
            this.surfraceOrientation = surfraceOrientation;
            this.surfaceAngularVelocity = surfaceAngularVelocity;
            this.surfaceAngularAcceleration = surfaceAngularAcceleration;
            this.orbit = orbit;
        }

        public override string ToString()
        {
            var position = this.position == null ? "" : DisplayConsole.logVector(this.position.Value);
            var velocity = this.velocity == null ? "" : DisplayConsole.logVector(this.velocity.Value);
            var acceleration = this.acceleration == null ? "" : DisplayConsole.logVector(this.acceleration.Value);
            var orientation = this.orientation == null ? "" : DisplayConsole.logQuaternion(this.orientation.Value);
            var angularVelocity = this.angularVelocity == null ? "" : DisplayConsole.logVector(this.angularVelocity.Value);
            var angularAcceleration = this.angularAcceleration == null ? "" : DisplayConsole.logVector(this.angularAcceleration.Value);
            var surfacePosition = this.surfacePosition == null ? "" : DisplayConsole.logVector(this.surfacePosition.Value);
            var surfaceVelocity = this.surfaceVelocity == null ? "" : DisplayConsole.logVector(this.surfaceVelocity.Value);
            var surfaceAcceleration = this.surfaceAcceleration == null ? "" : DisplayConsole.logVector(this.surfaceAcceleration.Value);
            var surfraceOrientation = this.surfraceOrientation == null ? "" : DisplayConsole.logQuaternion(this.surfraceOrientation.Value);
            var surfaceAngularVelocity = this.surfaceAngularVelocity == null ? "" : DisplayConsole.logVector(this.surfaceAngularVelocity.Value);
            var surfaceAngularAcceleration = this.surfaceAngularAcceleration == null ? "" : DisplayConsole.logVector(this.surfaceAngularAcceleration.Value);
            var orbit = (this.orbit?.ToString() ?? "");
            return $"({parent}, {position}, {velocity}, {acceleration}, {orientation}, {angularVelocity}, {angularAcceleration}, {surfacePosition}, {surfaceVelocity}, {surfaceAcceleration}, {surfraceOrientation}, {surfaceAngularVelocity}, {surfaceAngularAcceleration}, {orbit})";
        }

        public override bool Equals(System.Object other)
        {
            if (other != null && other is RelativeState)
            {
                var obj = other as RelativeState;
                return parent == obj.parent
                    && position == obj.position
                    && velocity == obj.velocity
                    && acceleration == obj.acceleration
                    && orientation == obj.orientation
                    && angularVelocity == obj.angularVelocity
                    && angularAcceleration == obj.angularAcceleration
                    && surfacePosition == obj.surfacePosition
                    && surfaceVelocity == obj.surfaceVelocity
                    && surfaceAcceleration == obj.surfaceAcceleration
                    && surfraceOrientation == obj.surfraceOrientation
                    && surfaceAngularVelocity == obj.surfaceAngularVelocity
                    && surfaceAngularAcceleration == obj.surfaceAngularAcceleration
                    && orbit == obj.orbit;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (parent.GetHashCode())
                + (position.GetHashCode() >> 2)
                + (velocity.GetHashCode() >> 4)
                + (acceleration.GetHashCode() >> 6)
                + (orientation.GetHashCode() >> 8)
                + (angularVelocity.GetHashCode() >> 10)
                + (angularAcceleration.GetHashCode() >> 12)
                + (surfacePosition.GetHashCode() >> 14)
                + (surfaceVelocity.GetHashCode() >> 16)
                + (surfaceAcceleration.GetHashCode() >> 18)
                + (surfraceOrientation.GetHashCode() >> 20)
                + (surfaceAngularVelocity.GetHashCode() >> 22)
                + (surfaceAngularAcceleration.GetHashCode() >> 24)
                + (orbit.GetHashCode() >> 26);
        }

        public AbsoluteState applyState(OWRigidbody target)
        {
            return applyState(AbsoluteState.fromCurrentState(parent), Position.getGravity(parent), target);
        }

        public AbsoluteState applyState(AbsoluteState parentState, Orbit.Gravity gravity, OWRigidbody target)
        {
            if (orbit != null && orbit.isOrbit() && gravity != null)
            {
                return applyKeplerState(parentState, gravity, target);
            }
            else if (parentState != null
                && surfacePosition.HasValue
                && surfaceVelocity.HasValue
                && surfaceAcceleration.HasValue)
            {
                return applySurfaceState(parentState, target);
            }
            else if (position.HasValue
                && velocity.HasValue
                && acceleration.HasValue)
            {
                return applyRelativeState(parentState, target);
            }

            return null;
        }

        private AbsoluteState applyKeplerState(AbsoluteState parentState, Orbit.Gravity gravity, OWRigidbody target)
        {
            var cartesian = Orbit.toCartesian(gravity, Time.timeSinceLevelLoad, orbit);
            var position = cartesian.Item1;
            var velocity = cartesian.Item2;
            var acceleration = Vector3.zero;

            if (parentState != null && surfacePosition.HasValue)
            {
                position = surfacePosition.Value.normalized * position.magnitude;
                position = parentState.TransformPoint(position);
                velocity += parentState.velocity;
                // TODO: Fix velocity to point in correct direction
            }
            else if (parentState != null)
            {
                position += parentState.position;
                velocity += parentState.velocity;
            }
            else
            {
                position += Locator.GetCenterOfTheUniverse()?.GetOffsetPosition() ?? Vector3.zero;
                velocity += Locator.GetCenterOfTheUniverse()?.GetOffsetVelocity() ?? Vector3.zero;
            }

            return applyMovement(parentState, target, position, velocity, acceleration);
        }

        private AbsoluteState applySurfaceState(AbsoluteState parentState, OWRigidbody target)
        {
            var position = this.surfacePosition.Value;
            var velocity = this.surfaceVelocity.Value;
            var acceleration = this.surfaceAcceleration.Value;

            Helper.helper.Console.WriteLine($"{target}: Surface: {DisplayConsole.logVector(position)} {DisplayConsole.logVector(velocity)} {DisplayConsole.logVector(acceleration)}");

            position = parentState.TransformPoint(position);
            velocity += parentState.GetPointVelocity(position);
            acceleration += parentState.GetPointAcceleration(position);

            return applyMovement(parentState, target, position, velocity, acceleration);
        }

        private AbsoluteState applyRelativeState(AbsoluteState parentState, OWRigidbody target)
        {
            var position = this.position.Value;
            var velocity = this.velocity.Value;
            var acceleration = this.acceleration.Value;

            Helper.helper.Console.WriteLine($"{target}: Relative: {DisplayConsole.logVector(position)} {DisplayConsole.logVector(velocity)} {DisplayConsole.logVector(acceleration)}");

            if (parentState != null)
            {
                position += parentState.position;
                velocity += parentState.velocity;
                acceleration += parentState.acceleration;
            }
            else
            {
                position += Locator.GetCenterOfTheUniverse()?.GetOffsetPosition() ?? Vector3.zero;
                velocity += Locator.GetCenterOfTheUniverse()?.GetOffsetVelocity() ?? Vector3.zero;
            }

            return applyMovement(parentState, target, position, velocity, acceleration);
        }

        private AbsoluteState applyMovement(AbsoluteState parentState, OWRigidbody target, Vector3 position, Vector3 velocity, Vector3 acceleration)
        {
            Helper.helper.Console.WriteLine($"{target}: Apply: {DisplayConsole.logVector(position)} {DisplayConsole.logVector(velocity)} {DisplayConsole.logVector(acceleration)}");

            target.SetPosition(new Vector3(position.x, position.y, position.z));
            target.SetVelocity(new Vector3(velocity.x, velocity.y, velocity.z));

            target.SetValue("_lastPosition", new Vector3(position.x, position.y, position.z));
            target.SetValue("_currentVelocity", new Vector3(velocity.x, velocity.y, velocity.z));
            target.SetValue("_lastVelocity", new Vector3(velocity.x, velocity.y, velocity.z));
            target.SetValue("_currentAccel", new Vector3(acceleration.x, acceleration.y, acceleration.z));
            target.SetValue("_lastAccel", new Vector3(acceleration.x, acceleration.y, acceleration.z));

            if (target.IsSuspended())
            {
                target.SetValue("_suspensionBody", Position.getBody(parent));
                target.SetValue("_cachedRelativeVelocity", parentState == null ? Vector3.zero : parentState.InverseTransformDirection(velocity - parentState.GetPointVelocity(position)));
            }

            var orientation = applyOrientation(parentState, target, position);
            return new AbsoluteState(position, velocity, acceleration, orientation.Item1, orientation.Item2, orientation.Item3);
        }

        private Tuple<Quaternion, Vector3, Vector3> applyOrientation(AbsoluteState parentState, OWRigidbody target, Vector3 worldPosition)
        {
            if (parentState != null
                && surfraceOrientation.HasValue
                && surfaceAngularVelocity.HasValue
                && surfaceAngularAcceleration.HasValue)
            {
                // TODO: this
                var rotation = surfraceOrientation.Value * parentState.orientation;
                var velocity = surfaceAngularVelocity.Value;

                return applyRotation(parentState, target, rotation, velocity, Vector3.zero);
            }
            else if (orientation.HasValue
                && angularVelocity.HasValue
                && angularAcceleration.HasValue)
            {
                var rotation = orientation.Value;
                var velocity = angularVelocity.Value;

                return applyRotation(parentState, target, rotation, velocity, Vector3.zero);
            }

            return Tuple.Create(Quaternion.identity, Vector3.zero, Vector3.zero);
        }

        private Tuple<Quaternion, Vector3, Vector3> applyRotation(AbsoluteState parentState, OWRigidbody target, Quaternion orientation, Vector3 angularVelocity, Vector3 angularAcceleration)
        {
            target.SetRotation(new Quaternion(orientation.x, orientation.y, orientation.z, orientation.w));
            target.SetAngularVelocity(new Vector3(angularVelocity.x, angularVelocity.y, angularVelocity.z));

            target.SetValue("_currentAngularVelocity", new Vector3(angularVelocity.x, angularVelocity.y, angularVelocity.z));
            target.SetValue("_lastAngularVelocity", new Vector3(angularVelocity.x, angularVelocity.y, angularVelocity.z));

            if (target.IsSuspended())
            {
                target.SetValue("_cachedAngularVelocity", angularVelocity);
            }

            return Tuple.Create(orientation, angularVelocity, Vector3.zero);
        }

        public static RelativeState fromGlobal(Position.HeavenlyBodies parent, Vector3 position, Vector3 velocity, Vector3 acceleration, Quaternion orientation, Vector3 angularVelocity, Vector3 angularAcceleration)
        {
            var state = AbsoluteState.fromCurrentState(parent);
            var gravity = Position.getGravity(parent);
            var size = Position.getSize(parent);

            return fromGlobal(parent, state, gravity, size, position, velocity, acceleration, orientation, angularVelocity, angularAcceleration);
        }

        public static RelativeState fromGlobal(Position.HeavenlyBodies parent, AbsoluteState parentState, Orbit.Gravity parentGravity, Position.Size parentSize, Vector3 position, Vector3 velocity, Vector3 acceleration, Quaternion orientation, Vector3 angularVelocity, Vector3 angularAcceleration)
        {
            Vector3? surfacePosition = null;
            Vector3? surfaceVelocity = null;
            Vector3? surfaceAcceleration = null;
            Quaternion? surfraceOrientation = null;
            Vector3? surfaceAngularVelocity = null;
            Vector3? surfaceAngularAcceleration = null;
            Orbit.KeplerCoordinates orbit = null;

            if (parentState == null)
            {
                parent = Position.HeavenlyBodies.None;
                position -= Locator.GetCenterOfTheUniverse()?.GetOffsetPosition() ?? Vector3.zero;
                velocity -= Locator.GetCenterOfTheUniverse()?.GetOffsetVelocity() ?? Vector3.zero;
            }
            else
            {
                var worldPosition = position;

                position -= parentState.position;
                velocity -= parentState.velocity;
                acceleration -= parentState.acceleration;

                var distance = position.sqrMagnitude;

                if (distance < (parentSize.influence * parentSize.influence))
                {
                    surfacePosition = parentState.InverseTransformPoint(worldPosition);
                    surfaceVelocity = velocity - parentState.GetPointVelocity(worldPosition);
                    surfaceAcceleration = acceleration - parentState.GetPointAcceleration(worldPosition);
                    surfraceOrientation = parentState.InverseTransformRotation(orientation);
                    surfaceAngularVelocity = Vector3.zero; // TODO
                    surfaceAngularAcceleration = Vector3.zero; // TODO
                }

                if ((parentSize.size * parentSize.size) < distance && distance < (parentSize.influence * parentSize.influence))
                {
                    var kepler = Orbit.toKeplerCoordinates(parentGravity, Time.timeSinceLevelLoad, position, velocity);
                    if (kepler != null && kepler.isOrbit())
                    {
                        var apoapsis = kepler.semiMajorRadius + kepler.foci;
                        var periapsis = kepler.semiMajorRadius - kepler.foci;
                        if (parentSize.size < periapsis && apoapsis < parentSize.influence)
                        {
                            orbit = kepler;
                        }
                    }
                }
            }

            return new RelativeState(parent, position, velocity, acceleration, orientation, angularVelocity, angularAcceleration, surfaceAcceleration, surfaceVelocity, surfaceAcceleration, surfraceOrientation, surfaceAngularVelocity, surfaceAngularAcceleration, orbit);
        }

        public static RelativeState fromRelative(Position.HeavenlyBodies parent, Vector3 position, Vector3 velocity, Vector3 acceleration, Quaternion orientation, Vector3 angularVelocity, Vector3 angularAcceleration)
        {
            return new RelativeState(parent, position, velocity, acceleration, orientation, angularVelocity, angularAcceleration, null, null, null, null, null, null, null);
        }

        public static RelativeState fromSurface(Position.HeavenlyBodies parent, Vector3 surfacePosition, Vector3 surfaceVelocity, Vector3 surfaceAcceleration, Quaternion surfraceOrientation, Vector3 surfaceAngularVelocity, Vector3 surfaceAngularAcceleration)
        {
            return new RelativeState(parent, null, null, null, null, null, null, surfacePosition, surfaceVelocity, surfaceAcceleration, surfraceOrientation, surfaceAngularVelocity, surfaceAngularAcceleration, null);
        }

        public static RelativeState fromKepler(Position.HeavenlyBodies parent, Orbit.KeplerCoordinates kepler, Quaternion orientation, Vector3 angularVelocity, Vector3 angularAcceleration)
        {
            if (kepler != null && kepler.isOrbit())
            {
                return new RelativeState(parent, null, null, null, orientation, angularVelocity, angularAcceleration, null, null, null, null, null, null, kepler);
            }
            return null;
        }

        public static RelativeState fromCurrentState(Position.HeavenlyBodies parent, OWRigidbody target)
        {
            var position = target.GetPosition();
            var velocity = target.GetVelocity();
            var acceleration = target.GetAcceleration();
            var orientation = target.GetRotation();
            var angularVelocity = target.GetAngularVelocity();
            var angularAcceleration = target.GetAngularAcceleration();

            return fromGlobal(parent, position, velocity, acceleration, orientation, angularVelocity, angularAcceleration);
        }

        public static RelativeState fromClosetInfluence(OWRigidbody target, params Position.HeavenlyBodies[] exclude)
        {
            var includes = (Position.HeavenlyBodies[])Enum.GetValues(typeof(Position.HeavenlyBodies));
            var excl = new HashSet<Position.HeavenlyBodies>(exclude);
            var parent = Position.getClosest(target.GetPosition(), (body) =>
            {
                if (excl.Contains(body))
                {
                    return true;
                }

                var parentBody = Position.getBody(body);
                var size = Position.getSize(body);
                if (parentBody == null || size == null)
                {
                    return false;
                }
                if ((target.GetPosition() - parentBody.GetPosition()).sqrMagnitude < size.influence * size.influence)
                {
                    return false;
                }
                return true;
            }, includes);

            return fromCurrentState(parent[0].Item1, target);
        }
    }
}
