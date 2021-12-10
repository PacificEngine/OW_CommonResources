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
        public Quaternion orientation { get; }
        public Vector3 angularVelocity { get; }
        public Vector3 angularAcceleration { get; }

        public OrientationState(Quaternion orientation, Vector3 angularVelocity, Vector3 angularAcceleration)
        {
            this.orientation = orientation;
            this.angularVelocity = angularVelocity;
            this.angularAcceleration = angularAcceleration;
        }

        public static OrientationState fromCurrentState(Position.HeavenlyBodies target)
        {
            return fromCurrentState(Position.getBody(target));
        }

        public static OrientationState fromCurrentState(OWRigidbody target)
        {
            if (target == null)
            {
                return null;
            }
            return new OrientationState(target.GetRotation(), target.GetAngularVelocity(), target.GetAngularAcceleration());
        }

        public override string ToString()
        {
            var orientation = this.orientation == null ? "" : DisplayConsole.logQuaternion(this.orientation);
            var angularVelocity = this.angularVelocity == null ? "" : DisplayConsole.logVector(this.angularVelocity);
            var angularAcceleration = this.angularAcceleration == null ? "" : DisplayConsole.logVector(this.angularAcceleration);
            return $"({orientation}, {angularVelocity}, {angularAcceleration})";
        }

        public override bool Equals(System.Object other)
        {
            if (other != null && other is OrientationState)
            {
                var obj = other as OrientationState;
                return orientation == obj.orientation
                    && angularVelocity == obj.angularVelocity
                    && angularAcceleration == obj.angularAcceleration;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (orientation.GetHashCode() * 4)
                + (angularVelocity.GetHashCode() * 16)
                + (angularAcceleration.GetHashCode() * 64);
        }

        public void applyRotation(OWRigidbody target)
        {
            target.SetRotation(new Quaternion(orientation.x, orientation.y, orientation.z, orientation.w));
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

        public PositionState(Vector3 position, Vector3 velocity, Vector3 acceleration)
        {
            this.position = position;
            this.velocity = velocity;
            this.acceleration = acceleration;
        }

        public static PositionState fromCurrentState(Position.HeavenlyBodies target)
        {
            return fromCurrentState(Position.getBody(target));
        }

        public static PositionState fromCurrentState(OWRigidbody target)
        {
            if (target == null)
            {
                return null;
            }
            return new PositionState(target.GetPosition(), target.GetVelocity(), target.GetAcceleration());
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
        public Orbit.KeplerCoordinates position { get; }
        public OrientationState orientation { get; }


        public KeplerState(Orbit.KeplerCoordinates position, OrientationState orientation)
        {
            this.position = position;
            this.orientation = orientation;
        }

        public override string ToString()
        {
            return $"({position}, {orientation})";
        }

        public override bool Equals(System.Object other)
        {
            if (other != null && other is KeplerState)
            {
                var obj = other as KeplerState;
                return position == obj.position
                    && orientation == obj.orientation;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (position.GetHashCode())
                + (orientation.GetHashCode() * 16);
        }
    }

    public class MovementState
    {
        public PositionState position { get; }
        public OrientationState orientation { get; }

        public MovementState(PositionState position, OrientationState orientation)
        {
            this.position = position;
            this.orientation = orientation;
        }

        public static MovementState fromCurrentState(Position.HeavenlyBodies target)
        {
            return fromCurrentState(Position.getBody(target));
        }

        public static MovementState fromCurrentState(OWRigidbody target)
        {
            if (target == null)
            {
                return null;
            }
            return new MovementState(PositionState.fromCurrentState(target), OrientationState.fromCurrentState(target));
        }

        public override string ToString()
        {
            return $"({position}, {orientation})";
        }

        public override bool Equals(System.Object other)
        {
            if (other != null && other is MovementState)
            {
                var obj = other as MovementState;
                return position == obj.position
                    && orientation == obj.orientation;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (position.GetHashCode())
                + (orientation.GetHashCode() * 16);
        }

        public Vector3 GetPointVelocity(Vector3 worldPoint) => this.GetPointTangentialVelocity(worldPoint) + position.velocity;

        public Vector3 GetPointTangentialVelocity(Vector3 worldPoint) => Vector3.Cross(orientation.angularVelocity, worldPoint - position.position);

        public Vector3 GetPointAcceleration(Vector3 worldPoint)
        {
            var rhs = worldPoint - position.position;
            return position.acceleration + Vector3.Cross(orientation.angularVelocity, Vector3.Cross(orientation.angularVelocity, rhs)) + Vector3.Cross(orientation.angularAcceleration, rhs);
        }

        public Vector3 GetPointTangentialAcceleration(Vector3 worldPoint)
        {
            var rhs = worldPoint - position.position;
            return Vector3.Cross(orientation.angularAcceleration, rhs);
        }

        public Vector3 GetPointCentripetalAcceleration(Vector3 worldPoint)
        {
            return Vector3.Cross(orientation.angularVelocity, Vector3.Cross(orientation.angularVelocity, worldPoint - position.position));
        }

        public Vector3 TransformDirection(Vector3 localDirection)
        {
            return orientation.orientation * localDirection;
        }

        public Vector3 InverseTransformDirection(Vector3 worldDirection)
        {
            return Quaternion.Inverse(orientation.orientation) * worldDirection;
        }

        public Vector3 TransformPoint(Vector3 localPoint)
        {
            return orientation.orientation * localPoint + position.position;
        }

        public Vector3 InverseTransformPoint(Vector3 worldPoint)
        {
            return Quaternion.Inverse(orientation.orientation) * (worldPoint - position.position);
        }

        public Quaternion TransformRotation(Quaternion localPoint)
        {
            return orientation.orientation * localPoint;
        }

        public Quaternion InverseTransformRotation(Quaternion worldPoint)
        {
            return Quaternion.Inverse(orientation.orientation) * worldPoint;
        }

        public void apply(OWRigidbody target)
        {
            position.applyMovement(target);
            orientation.applyRotation(target);
        }

        public void apply(Position.HeavenlyBodies parent, MovementState parentState, OWRigidbody target)
        {
            position.applyMovement(target);
            orientation.applyRotation(target);
            applyCachedState(parent, parentState, target);
        }

        private void applyCachedState(Position.HeavenlyBodies parent, MovementState parentState, OWRigidbody target)
        {
            if (target.IsSuspended())
            {
                var parentBody = Position.getBody(parent);
                target.SetValue("_suspensionBody", parentBody);
                target.SetValue("_cachedRelativeVelocity", (parentBody == null || parentState == null) ? Vector3.zero : parentState.InverseTransformDirection(position.velocity - parentState.GetPointVelocity(position.position)));
                target.SetValue("_cachedAngularVelocity", orientation.angularVelocity);
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

        public MovementState apply(OWRigidbody body)
        {
            var parentState = MovementState.fromCurrentState(parent);
            var parentGravity = Position.getGravity(parent);

            return apply(body, parentState, parentGravity);
        }

        public MovementState apply(OWRigidbody body, MovementState parentState, Orbit.Gravity gravity)
        {
            var state = getAbsoluteState(parentState, gravity);
            if (state != null)
            {
                state.apply(parent, parentState, body);
            }
            return state;
        }

        public MovementState getAbsoluteState()
        {
            var parentState = MovementState.fromCurrentState(parent);
            var parentGravity = Position.getGravity(parent);

            return getAbsoluteState(parentState, parentGravity);
        }

        public MovementState getAbsoluteState(MovementState parentState, Orbit.Gravity gravity)
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

            return new MovementState(movement, orientation);
        }

        private PositionState getAbsoluteMovement(MovementState parentState, Orbit.Gravity gravity)
        {
            if (orbit != null && orbit.position != null && orbit.position.isOrbit() && gravity != null)
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

        private PositionState getAbsoluteFromKeplerState(MovementState parentState, Orbit.Gravity gravity)
        {
            var cartesian = Orbit.toCartesian(gravity, Time.timeSinceLevelLoad, orbit.position);
            var position = cartesian.Item1;
            var velocity = cartesian.Item2;
            var acceleration = Vector3.zero;

            if (parentState != null && surface != null)
            {
                position = surface.position.position.normalized * position.magnitude;
                position = parentState.TransformPoint(position);
                velocity += parentState.position.velocity;
                // TODO: Fix velocity to point in correct direction
            }
            else if (parentState != null)
            {
                position += parentState.position.position;
                velocity += parentState.position.velocity;
            }
            else
            {
                position += Locator.GetCenterOfTheUniverse()?.GetOffsetPosition() ?? Vector3.zero;
                velocity += Locator.GetCenterOfTheUniverse()?.GetOffsetVelocity() ?? Vector3.zero;
            }

            return new PositionState(position, velocity, acceleration);
        }

        private PositionState getAbsoluteFromSurfaceState(MovementState parentState)
        {
            var position = this.surface.position.position;
            var velocity = this.surface.position.velocity;
            var acceleration = this.surface.position.acceleration;

            position = parentState.TransformPoint(position);
            velocity += parentState.GetPointVelocity(position);
            acceleration += parentState.GetPointAcceleration(position);

            return new PositionState(position, velocity, acceleration);
        }

        private PositionState getAbsoluteFromRelativeState(MovementState parentState)
        {
            var position = this.relative.position.position;
            var velocity = this.relative.position.velocity;
            var acceleration = this.relative.position.acceleration;

            if (parentState != null)
            {
                position += parentState.position.position;
                velocity += parentState.position.velocity;
                acceleration += parentState.position.acceleration;
            }
            else
            {
                position += Locator.GetCenterOfTheUniverse()?.GetOffsetPosition() ?? Vector3.zero;
                velocity += Locator.GetCenterOfTheUniverse()?.GetOffsetVelocity() ?? Vector3.zero;
            }

            return new PositionState(position, velocity, acceleration);
        }

        private OrientationState getAbsoluteOrientation(MovementState parentState, Orbit.Gravity gravity, Vector3 worldPosition)
        {
            if (orbit != null && orbit.position != null && orbit.position.isOrbit() && gravity != null)
            {
                var rotation = orbit.orientation.orientation; // TODO
                var velocity = orbit.orientation.angularVelocity; // TODO

                return new OrientationState(rotation, velocity, Vector3.zero);
            }
            if (parentState != null && surface != null)
            {
                var rotation = surface.orientation.orientation * parentState.orientation.orientation;
                var velocity = surface.orientation.angularVelocity; // TODO

                return new OrientationState(rotation, velocity, Vector3.zero);
            }
            else if (relative != null)
            {
                var rotation = relative.orientation.orientation;
                var velocity = relative.orientation.angularVelocity;

                return new OrientationState(rotation, velocity, Vector3.zero);
            }

            return new OrientationState(Quaternion.identity, Vector3.zero, Vector3.zero);
        }

        public static RelativeState fromGlobal(Position.HeavenlyBodies parent, Vector3 position, Vector3 velocity, Vector3 acceleration, Quaternion orientation, Vector3 angularVelocity, Vector3 angularAcceleration)
        {
            var state = MovementState.fromCurrentState(parent);
            var gravity = Position.getGravity(parent);
            var size = Position.getSize(parent);

            return fromGlobal(parent, state, gravity, size, position, velocity, acceleration, orientation, angularVelocity, angularAcceleration);
        }

        public static RelativeState fromGlobal(Position.HeavenlyBodies parent, MovementState parentState, Orbit.Gravity parentGravity, Position.Size parentSize, Vector3 worldPosition, Vector3 worldVelocity, Vector3 worldAcceleration, Quaternion worldOrientation, Vector3 worldAngularVelocity, Vector3 worldAngularAcceleration)
        {
            MovementState surfaceMovement = null;
            KeplerState orbit = null;

            parent = parentState == null ? Position.HeavenlyBodies.None : parent;
            var relativeMovement = getRelativeMovement(parentState, parentGravity, worldPosition, worldVelocity, worldAcceleration, worldOrientation, worldAngularVelocity, worldAngularAcceleration);
            if (parentState != null)
            {
                var distance = relativeMovement.position.position.sqrMagnitude;
                if (distance < (parentSize.influence * parentSize.influence))
                {
                    surfaceMovement = getSurfaceMovement(parentState, parentGravity, worldPosition, worldVelocity, worldAcceleration, worldOrientation, worldAngularVelocity, worldAngularAcceleration);
                }

                if ((parentSize.size * parentSize.size) < distance && distance < (parentSize.influence * parentSize.influence))
                {
                    var kepler = getKepler(parentState, parentGravity, worldPosition, worldVelocity, worldOrientation, worldAngularVelocity, worldAngularAcceleration);
                    if (kepler != null && kepler.position != null && kepler.position.isOrbit())
                    {
                        var apoapsis = kepler.position.semiMajorRadius + kepler.position.foci;
                        var periapsis = kepler.position.semiMajorRadius - kepler.position.foci;
                        if (parentSize.size < periapsis && apoapsis < parentSize.influence)
                        {
                            orbit = kepler;
                        }
                    }
                }
            }

            return new RelativeState(parent, relativeMovement, surfaceMovement, orbit);
        }

        public static MovementState getRelativeMovement(MovementState parentState, Orbit.Gravity parentGravity, Vector3 worldPosition, Vector3 worldVelocity, Vector3 worldAcceleration, Quaternion worldOrientation, Vector3 worldAngularVelocity, Vector3 worldAngularAcceleration)
        {
            var relativePosition = worldPosition - (parentState == null ? (Locator.GetCenterOfTheUniverse()?.GetOffsetPosition() ?? Vector3.zero) : parentState.position.position);
            var relativeVelocity = worldVelocity - (parentState == null ? (Locator.GetCenterOfTheUniverse()?.GetOffsetVelocity() ?? Vector3.zero) : parentState.position.velocity);
            var relativeAcceleration = worldAcceleration - (parentState == null ? Vector3.zero : parentState.position.acceleration);
            var relativeOrientation = worldOrientation;
            var relaitveAngularVelocity = worldAngularVelocity;
            var relativeAngularAcceleration = worldAngularAcceleration;

            return new MovementState(new PositionState(relativePosition, relativeVelocity, relativeAcceleration), new OrientationState(relativeOrientation, relaitveAngularVelocity, relativeAngularAcceleration));
        }

        public static MovementState getSurfaceMovement(MovementState parentState, Orbit.Gravity parentGravity, Vector3 worldPosition, Vector3 worldVelocity, Vector3 worldAcceleration, Quaternion worldOrientation, Vector3 worldAngularVelocity, Vector3 worldAngularAcceleration)
        {
            var surfacePosition = parentState.InverseTransformPoint(worldPosition);
            var surfaceVelocity = worldVelocity - parentState.GetPointVelocity(worldPosition);
            var surfaceAcceleration = worldAcceleration - parentState.GetPointAcceleration(worldPosition);
            var surfraceOrientation = parentState.InverseTransformRotation(worldOrientation);
            var surfaceAngularVelocity = Vector3.zero; // TODO
            var surfaceAngularAcceleration = Vector3.zero; // TODO

            return new MovementState(new PositionState(surfacePosition, surfaceVelocity, surfaceAcceleration), new OrientationState(surfraceOrientation, surfaceAngularVelocity, surfaceAngularAcceleration));
        }

        public static KeplerState getKepler(MovementState parentState, Orbit.Gravity parentGravity, Vector3 worldPosition, Vector3 worldVelocity, Quaternion worldOrientation, Vector3 worldAngularVelocity, Vector3 worldAngularAcceleration)
        {
            var position = worldPosition - parentState.position.position;
            var velocity = worldVelocity - parentState.position.velocity;

            return new KeplerState(Orbit.toKeplerCoordinates(parentGravity, Time.timeSinceLevelLoad, position, velocity), new OrientationState(worldOrientation, worldAngularVelocity, worldAngularAcceleration));
        }

        public static Orbit.KeplerCoordinates getKepler(MovementState parentState, Orbit.Gravity parentGravity, Vector3 worldPosition, Vector3 worldVelocity)
        {
            var position = worldPosition - parentState.position.position;
            var velocity = worldVelocity - parentState.position.velocity;

            return Orbit.toKeplerCoordinates(parentGravity, Time.timeSinceLevelLoad, position, velocity);
        }

        public static RelativeState fromRelative(Position.HeavenlyBodies parent, MovementState relative)
        {
            return new RelativeState(parent, relative, null, null);
        }

        public static RelativeState fromSurface(Position.HeavenlyBodies parent, MovementState surface)
        {
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
