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
    public class OrientationState
    {
        public Quaternion rotation { get; }
        public Vector3 angularVelocity { get; }
        public Vector3 angularAcceleration { get; }

        public OrientationState(Quaternion rotation, Vector3 angularVelocity, Vector3 angularAcceleration)
        {
            this.rotation = rotation;
            this.angularVelocity = angularVelocity;
            this.angularAcceleration = angularAcceleration;
        }

        public static OrientationState fromCurrentState(Position.HeavenlyBodies target)
        {
            return fromCurrentState(Position.getBody(target));
        }

        public static OrientationState fromCurrentState(OWRigidbody target)
        {
            if (target == null || target.GetRigidbody() == null)
            {
                return null;
            }
            return new OrientationState(target.GetRotation(), target.GetAngularVelocity(), target.GetAngularAcceleration());
        }

        public override string ToString()
        {
            var orientation = this.rotation == null ? "" : DisplayConsole.logQuaternion(this.rotation);
            var angularVelocity = this.angularVelocity == null ? "" : DisplayConsole.logVector(this.angularVelocity);
            var angularAcceleration = this.angularAcceleration == null ? "" : DisplayConsole.logVector(this.angularAcceleration);
            return $"({orientation}, {angularVelocity}, {angularAcceleration})";
        }

        public override bool Equals(System.Object other)
        {
            if (other != null && other is OrientationState)
            {
                var obj = other as OrientationState;
                return rotation == obj.rotation
                    && angularVelocity == obj.angularVelocity
                    && angularAcceleration == obj.angularAcceleration;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (rotation.GetHashCode() * 4)
                + (angularVelocity.GetHashCode() * 16)
                + (angularAcceleration.GetHashCode() * 64);
        }

        public void applyRotation(OWRigidbody target)
        {
            target.SetRotation(new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w));
            target.SetAngularVelocity(new Vector3(angularVelocity.x, angularVelocity.y, angularVelocity.z));

            target.SetValue("_currentAngularVelocity", new Vector3(angularVelocity.x, angularVelocity.y, angularVelocity.z));
            target.SetValue("_lastAngularVelocity", new Vector3(angularVelocity.x, angularVelocity.y, angularVelocity.z));
        }
    }

    public class PositionState
    {
        public Vector3 position { get; }
        public Vector3 velocity { get; }
        public Vector3 acceleration { get; }
        public Vector3 jerk { get; }

        public PositionState(Vector3 position, Vector3 velocity, Vector3 acceleration, Vector3 jerk)
        {
            this.position = position;
            this.velocity = velocity;
            this.acceleration = acceleration;
            this.jerk = jerk;
        }

        public static PositionState fromCurrentState(Position.HeavenlyBodies target)
        {
            return fromCurrentState(Position.getBody(target));
        }

        public static PositionState fromCurrentState(OWRigidbody target)
        {
            if (target == null || target.GetRigidbody() == null)
            {
                return null;
            }
            return new PositionState(target.GetPosition(), target.GetVelocity(), target.GetAcceleration(), target.GetJerk());
        }

        public override string ToString()
        {
            var position = this.position == null ? "" : DisplayConsole.logVector(this.position);
            var velocity = this.velocity == null ? "" : DisplayConsole.logVector(this.velocity);
            var acceleration = this.acceleration == null ? "" : DisplayConsole.logVector(this.acceleration);
            return $"({position}, {velocity}, {acceleration})";
        }

        public override bool Equals(System.Object other)
        {
            if (other != null && other is PositionState)
            {
                var obj = other as PositionState;
                return position == obj.position
                   && velocity == obj.velocity
                   && acceleration == obj.acceleration;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (position.GetHashCode() * 4)
                + (velocity.GetHashCode() * 16)
                + (acceleration.GetHashCode() * 64);
        }

        public void applyMovement(OWRigidbody target)
        {
            target.SetPosition(new Vector3(position.x, position.y, position.z));
            target.SetVelocity(new Vector3(velocity.x, velocity.y, velocity.z));

            target.SetValue("_lastPosition", new Vector3(position.x, position.y, position.z));
            target.SetValue("_currentVelocity", new Vector3(velocity.x, velocity.y, velocity.z));
            target.SetValue("_lastVelocity", new Vector3(velocity.x, velocity.y, velocity.z));
            target.SetValue("_currentAccel", new Vector3(acceleration.x, acceleration.y, acceleration.z));
            target.SetValue("_lastAccel", new Vector3(acceleration.x, acceleration.y, acceleration.z));
        }
    }

    public class KeplerState
    {
        public Orbit.KeplerCoordinates coordinates { get; }
        public OrientationState orientation { get; }

        public Quaternion rotation { get { return orientation?.rotation ?? Quaternion.identity; } }
        public Vector3 angularVelocity { get { return orientation?.angularVelocity ?? Vector3.zero; } }
        public Vector3 angularAcceleration { get { return orientation?.angularAcceleration ?? Vector3.zero; } }


        public KeplerState(Orbit.KeplerCoordinates coordinates, OrientationState orientation)
        {
            this.coordinates = coordinates;
            this.orientation = orientation;
        }

        public override string ToString()
        {
            return $"({coordinates}, {orientation})";
        }

        public override bool Equals(System.Object other)
        {
            if (other != null && other is KeplerState)
            {
                var obj = other as KeplerState;
                return coordinates == obj.coordinates
                    && orientation == obj.orientation;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (coordinates.GetHashCode())
                + (orientation.GetHashCode() * 16);
        }
    }

    public class MovementState
    {
        public PositionState coordinates { get; }
        public OrientationState orientation { get; }

        public Vector3 position { get { return coordinates?.position ?? Vector3.zero; } }
        public Vector3 velocity { get { return coordinates?.velocity ?? Vector3.zero; } }
        public Vector3 acceleration { get { return coordinates?.acceleration ?? Vector3.zero; } }
        public Vector3 jerk { get { return coordinates?.jerk ?? Vector3.zero; } }

        public Quaternion rotation { get { return orientation?.rotation ?? Quaternion.identity; } }
        public Vector3 angularVelocity { get { return orientation?.angularVelocity ?? Vector3.zero; } }
        public Vector3 angularAcceleration { get { return orientation?.angularAcceleration ?? Vector3.zero; } }

        public MovementState(PositionState coordinates, OrientationState orientation)
        {
            this.coordinates = coordinates ?? new PositionState(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
            this.orientation = orientation ?? new OrientationState(Quaternion.identity, Vector3.zero, Vector3.zero);
        }

        public override string ToString()
        {
            return $"({coordinates}, {orientation})";
        }

        public override bool Equals(System.Object other)
        {
            if (other != null && other is MovementState)
            {
                var obj = other as MovementState;
                return coordinates == obj.coordinates
                    && orientation == obj.orientation;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (coordinates.GetHashCode())
                + (orientation.GetHashCode() * 16);
        }
    }

    public class AbsoluteState : MovementState
    {
        public AbsoluteState(PositionState coordinates, OrientationState orientation) : base(coordinates, orientation)
        {
        }

        public static AbsoluteState fromCurrentState(Position.HeavenlyBodies target)
        {
            return fromCurrentState(Position.getBody(target));
        }

        public static AbsoluteState fromCurrentState(OWRigidbody target)
        {
            if (target == null || target.GetRigidbody() == null)
            {
                return null;
            }
            return new AbsoluteState(PositionState.fromCurrentState(target), OrientationState.fromCurrentState(target));
        }

        public override string ToString()
        {
            return $"({coordinates}, {orientation})";
        }

        public override bool Equals(System.Object other)
        {
            if (other != null && other is AbsoluteState)
            {
                var obj = other as AbsoluteState;
                return position == obj.position
                    && orientation == obj.orientation;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (coordinates.GetHashCode())
                + (orientation.GetHashCode() * 16);
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
            return rotation * localDirection;
        }

        public Vector3 InverseTransformDirection(Vector3 worldDirection)
        {
            return Quaternion.Inverse(rotation) * worldDirection;
        }

        public Vector3 TransformPoint(Vector3 localPoint)
        {
            return rotation * localPoint + position;
        }

        public Vector3 InverseTransformPoint(Vector3 worldPoint)
        {
            return Quaternion.Inverse(rotation) * (worldPoint - position);
        }

        public Quaternion TransformRotation(Quaternion localPoint)
        {
            return localPoint * rotation;
        }

        public Quaternion InverseTransformRotation(Quaternion worldPoint)
        {
            return Quaternion.Inverse(rotation) * worldPoint;
        }

        public void apply(OWRigidbody target)
        {
            coordinates.applyMovement(target);
            orientation.applyRotation(target);
        }

        public void apply(Position.HeavenlyBodies parent, AbsoluteState parentState, OWRigidbody target)
        {
            coordinates.applyMovement(target);
            orientation.applyRotation(target);
            applyCachedState(parent, parentState, target);
        }

        private void applyCachedState(Position.HeavenlyBodies parent, AbsoluteState parentState, OWRigidbody target)
        {
            if (target.IsSuspended())
            {
                var parentBody = Position.getBody(parent);
                target.SetValue("_suspensionBody", parentBody);
                target.SetValue("_cachedRelativeVelocity", (parentBody == null || parentState == null) ? Vector3.zero : parentState.InverseTransformDirection(velocity - parentState.GetPointVelocity(position)));
                target.SetValue("_cachedAngularVelocity", angularVelocity);
            }
        }
    }

    public class RelativeState
    {
        public Position.HeavenlyBodies parent { get; }
        public MovementState relative { get; }
        public MovementState surface { get; }
        public KeplerState orbit { get; }


        private RelativeState(Position.HeavenlyBodies parent, MovementState relative, MovementState surface, KeplerState orbit)
        {
            this.parent = parent;
            this.relative = relative;
            this.surface = surface;
            this.orbit = orbit;
        }

        public override string ToString()
        {
            var orbit = (this.orbit?.ToString() ?? "");
            return $"({parent}, {relative}, {surface}, {orbit})";
        }

        public override bool Equals(System.Object other)
        {
            if (other != null && other is RelativeState)
            {
                var obj = other as RelativeState;
                return parent == obj.parent
                    && relative == obj.relative
                    && surface == obj.surface
                    && orbit == obj.orbit;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (parent.GetHashCode())
                + (relative.GetHashCode() >> 2)
                + (surface.GetHashCode() >> 4)
                + (orbit.GetHashCode() >> 6);
        }

        public AbsoluteState apply(OWRigidbody body)
        {
            var parentState = AbsoluteState.fromCurrentState(parent);
            var parentGravity = Position.getGravity(parent);

            return apply(body, parentState, parentGravity);
        }

        public AbsoluteState apply(OWRigidbody body, AbsoluteState parentState, Orbit.Gravity gravity)
        {
            var state = getAbsoluteState(parentState, gravity);
            if (state != null)
            {
                state.apply(parent, parentState, body);
            }
            return state;
        }

        public AbsoluteState getAbsoluteState()
        {
            var parentState = AbsoluteState.fromCurrentState(parent);
            var parentGravity = Position.getGravity(parent);

            return getAbsoluteState(parentState, parentGravity);
        }

        public AbsoluteState getAbsoluteState(AbsoluteState parentState, Orbit.Gravity gravity)
        {
            var movement = getAbsoluteMovement(parentState, gravity);
            if (movement == null)
            {
                return null;
            }

            var orientation = getAbsoluteOrientation(parentState, gravity, movement.position);
            if (orientation == null)
            {
                return null;
            }

            return new AbsoluteState(movement, orientation);
        }

        private PositionState getAbsoluteMovement(AbsoluteState parentState, Orbit.Gravity gravity)
        {
            if (orbit != null && orbit.coordinates != null && orbit.coordinates.isOrbit() && gravity != null)
            {
                return getAbsoluteFromKeplerState(parentState, gravity);
            }
            else if (parentState != null
                && surface != null)
            {
                return getAbsoluteFromSurfaceState(parentState);
            }
            else if (relative != null)
            {
                return getAbsoluteFromRelativeState(parentState);
            }

            return null;
        }

        private PositionState getAbsoluteFromKeplerState(AbsoluteState parentState, Orbit.Gravity gravity)
        {
            var cartesian = Orbit.toCartesian(gravity, Time.timeSinceLevelLoad, orbit.coordinates);
            var position = cartesian.Item1;
            var velocity = cartesian.Item2;
            var acceleration = Vector3.zero;
            var jerk = Vector3.zero;

            if (parentState != null && surface != null)
            {
                position = surface.position.normalized * position.magnitude;
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

            return new PositionState(position, velocity, acceleration, jerk);
        }

        private PositionState getAbsoluteFromSurfaceState(AbsoluteState parentState)
        {
            var position = this.surface.position;
            var velocity = this.surface.velocity;
            var acceleration = this.surface.acceleration;
            var jerk = this.surface.jerk;

            position = parentState.TransformPoint(position);
            velocity += parentState.GetPointVelocity(position);
            acceleration += parentState.GetPointAcceleration(position);

            return new PositionState(position, velocity, acceleration, jerk);
        }

        private PositionState getAbsoluteFromRelativeState(AbsoluteState parentState)
        {
            var position = this.relative.position;
            var velocity = this.relative.velocity;
            var acceleration = this.relative.acceleration;
            var jerk = this.relative.jerk;

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

            return new PositionState(position, velocity, acceleration, jerk);
        }

        private OrientationState getAbsoluteOrientation(AbsoluteState parentState, Orbit.Gravity gravity, Vector3 worldPosition)
        {
            if (orbit != null && orbit.coordinates != null && orbit.coordinates.isOrbit() && gravity != null)
            {
                var rotation = orbit.rotation; // TODO
                var velocity = orbit.angularVelocity; // TODO

                return new OrientationState(rotation, velocity, Vector3.zero);
            }
            if (parentState != null && surface != null)
            {
                var rotation = surface.rotation * parentState.rotation;
                var velocity = surface.angularVelocity; // TODO

                return new OrientationState(rotation, velocity, Vector3.zero);
            }
            else if (relative != null)
            {
                var rotation = relative.rotation;
                var velocity = relative.angularVelocity;

                return new OrientationState(rotation, velocity, Vector3.zero);
            }

            return new OrientationState(Quaternion.identity, Vector3.zero, Vector3.zero);
        }

        public static RelativeState fromGlobal(Position.HeavenlyBodies parent, Vector3 worldPosition, Vector3 worldVelocity, Vector3 worldAcceleration, Vector3 worldJerk, Quaternion worldOrientation, Vector3 worldAngularVelocity, Vector3 worldAngularAcceleration)
        {
            var state = AbsoluteState.fromCurrentState(parent);
            var gravity = Position.getGravity(parent);
            var size = Position.getSize(parent);

            return fromGlobal(parent, state, gravity, size, worldPosition, worldVelocity, worldAcceleration, worldJerk, worldOrientation, worldAngularVelocity, worldAngularAcceleration);
        }

        public static RelativeState fromGlobal(Position.HeavenlyBodies parent, AbsoluteState parentState, Orbit.Gravity parentGravity, Position.Size parentSize, Vector3 worldPosition, Vector3 worldVelocity, Vector3 worldAcceleration, Vector3 worldJerk, Quaternion worldOrientation, Vector3 worldAngularVelocity, Vector3 worldAngularAcceleration)
        {
            MovementState surfaceMovement = null;
            KeplerState orbit = null;

            parent = parentState == null ? Position.HeavenlyBodies.None : parent;
            var relativeMovement = getRelativeMovement(parentState, parentGravity, worldPosition, worldVelocity, worldAcceleration, worldJerk, worldOrientation, worldAngularVelocity, worldAngularAcceleration);
            if (parentState != null)
            {
                var distance = relativeMovement.position.sqrMagnitude;
                if (distance < (parentSize.influence * parentSize.influence))
                {
                    surfaceMovement = getSurfaceMovement(parentState, parentGravity, worldPosition, worldVelocity, worldAcceleration, worldJerk, worldOrientation, worldAngularVelocity, worldAngularAcceleration);
                }

                if ((parentSize.size * parentSize.size) < distance && distance < (parentSize.influence * parentSize.influence))
                {
                    var kepler = getKepler(parentState, parentGravity, worldPosition, worldVelocity, worldOrientation, worldAngularVelocity, worldAngularAcceleration);
                    if (kepler != null && kepler.coordinates != null && kepler.coordinates.isOrbit())
                    {
                        var apoapsis = kepler.coordinates.semiMajorRadius + kepler.coordinates.foci;
                        var periapsis = kepler.coordinates.semiMajorRadius - kepler.coordinates.foci;
                        if (parentSize.size < periapsis && apoapsis < parentSize.influence)
                        {
                            orbit = kepler;
                        }
                    }
                }
            }

            return new RelativeState(parent, relativeMovement, surfaceMovement, orbit);
        }

        public static MovementState getRelativeMovement(AbsoluteState parentState, Orbit.Gravity parentGravity, Vector3 worldPosition, Vector3 worldVelocity, Vector3 worldAcceleration, Vector3 worldJerk, Quaternion worldOrientation, Vector3 worldAngularVelocity, Vector3 worldAngularAcceleration)
        {
            var relativePosition = worldPosition - (parentState == null ? (Locator.GetCenterOfTheUniverse()?.GetOffsetPosition() ?? Vector3.zero) : parentState.position);
            var relativeVelocity = worldVelocity - (parentState == null ? (Locator.GetCenterOfTheUniverse()?.GetOffsetVelocity() ?? Vector3.zero) : parentState.velocity);
            var relativeAcceleration = worldAcceleration - (parentState == null ? Vector3.zero : parentState.acceleration);
            var relativeJerk = worldJerk - (parentState == null ? Vector3.zero : parentState.jerk);
            var relativeOrientation = worldOrientation;
            var relaitveAngularVelocity = worldAngularVelocity;
            var relativeAngularAcceleration = worldAngularAcceleration;

            return new MovementState(new PositionState(relativePosition, relativeVelocity, relativeAcceleration, relativeJerk), new OrientationState(relativeOrientation, relaitveAngularVelocity, relativeAngularAcceleration));
        }

        public static MovementState getSurfaceMovement(AbsoluteState parentState, Orbit.Gravity parentGravity, Vector3 worldPosition, Vector3 worldVelocity, Vector3 worldJerk, Vector3 worldAcceleration, Quaternion worldOrientation, Vector3 worldAngularVelocity, Vector3 worldAngularAcceleration)
        {
            var surfacePosition = parentState.InverseTransformPoint(worldPosition);
            var surfaceVelocity = worldVelocity - parentState.GetPointVelocity(worldPosition);
            var surfaceAcceleration = worldAcceleration - parentState.GetPointAcceleration(worldPosition);
            var surfaceJerk = worldJerk - (parentState == null ? Vector3.zero : parentState.jerk);
            var surfraceOrientation = parentState.InverseTransformRotation(worldOrientation);
            var surfaceAngularVelocity = Vector3.zero; // TODO
            var surfaceAngularAcceleration = Vector3.zero; // TODO

            return new MovementState(new PositionState(surfacePosition, surfaceVelocity, surfaceAcceleration, surfaceJerk), new OrientationState(surfraceOrientation, surfaceAngularVelocity, surfaceAngularAcceleration));
        }

        public static KeplerState getKepler(AbsoluteState parentState, Orbit.Gravity parentGravity, Vector3 worldPosition, Vector3 worldVelocity, Quaternion worldOrientation, Vector3 worldAngularVelocity, Vector3 worldAngularAcceleration)
        {
            return new KeplerState(Position.getKepler(parentState, parentGravity, worldPosition, worldVelocity), new OrientationState(worldOrientation, worldAngularVelocity, worldAngularAcceleration));
        }

        public static RelativeState fromRelative(Position.HeavenlyBodies parent, MovementState relative)
        {
            if (relative == null)
            {
                return null;
            }
            return new RelativeState(parent, relative, null, null);
        }

        public static RelativeState fromSurface(Position.HeavenlyBodies parent, MovementState surface)
        {
            if (surface == null)
            {
                return null;
            }
            return new RelativeState(parent, null, surface, null);
        }

        public static RelativeState fromKepler(Position.HeavenlyBodies parent, Orbit.KeplerCoordinates kepler, OrientationState orientation)
        {
            if (kepler != null && kepler.isOrbit())
            {
                return new RelativeState(parent, null, null, new KeplerState(kepler, orientation));
            }
            return null;
        }

        public static RelativeState fromCurrentState(Position.HeavenlyBodies parent, OWRigidbody target)
        {
            if (target == null || target.GetRigidbody() == null)
            {
                return null;
            }

            var position = target.GetPosition();
            var velocity = target.GetVelocity();
            var acceleration = target.GetAcceleration();
            var jerk = target.GetJerk();
            var orientation = target.GetRotation();
            var angularVelocity = target.GetAngularVelocity();
            var angularAcceleration = target.GetAngularAcceleration();

            return fromGlobal(parent, position, velocity, acceleration, jerk, orientation, angularVelocity, angularAcceleration);
        }

        public static RelativeState fromClosetInfluence(OWRigidbody target, params Position.HeavenlyBodies[] exclude)
        {
            if (target == null || target.GetRigidbody() == null)
            {
                return null;
            }

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
                if (parentBody == null || size == null || target == null)
                {
                    return false;
                }
                else if ((target.GetPosition() - parentBody.GetPosition()).sqrMagnitude < size.influence * size.influence)
                {
                    return false;
                }
                return true;
            }, includes);

            if (parent.Count < 1)
            {
                return null;
            }
            return fromCurrentState(parent[0].Item1, target);
        }
    }
}
