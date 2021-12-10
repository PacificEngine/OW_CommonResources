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
    public class MovementState
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


        private MovementState(Position.HeavenlyBodies parent,
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
            if (other != null && other is MovementState)
            {
                var obj = other as MovementState;
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

        public void applyState(OWRigidbody target)
        {
            applyState(Position.getBody(parent), Position.getGravity(parent), target);
        }

        private void applyState(OWRigidbody parentBody, Orbit.Gravity gravity, OWRigidbody target)
        {
            if (orbit != null && orbit.isOrbit() && gravity != null)
            {
                applyKeplerState(parentBody, gravity, target);
            }
            else if (parentBody != null
                && surfacePosition.HasValue
                && surfaceVelocity.HasValue
                && surfaceAcceleration.HasValue)
            {
                applySurfaceState(parentBody, target);
            }
            else if (position.HasValue
                && velocity.HasValue
                && acceleration.HasValue)
            {
                applyRelativeState(parentBody, target);
            }
        }

        private void applyKeplerState(OWRigidbody parentBody, Orbit.Gravity gravity, OWRigidbody target)
        {
            var cartesian = Orbit.toCartesian(gravity, Time.timeSinceLevelLoad, orbit);
            var position = cartesian.Item1;
            var velocity = cartesian.Item2;
            var acceleration = Vector3.zero;

            Helper.helper.Console.WriteLine($"{target}: Kepler: {DisplayConsole.logVector(position)} {DisplayConsole.logVector(velocity)} {DisplayConsole.logVector(acceleration)}");

            if (parentBody != null && surfacePosition.HasValue)
            {
                Helper.helper.Console.WriteLine($"{target}: Kepler1: {parentBody} {DisplayConsole.logVector(surfacePosition.Value)} {DisplayConsole.logVector(surfacePosition.Value.normalized * position.magnitude)} {DisplayConsole.logVector(parentBody.GetVelocity())}");
                position = surfacePosition.Value.normalized * position.magnitude;
                position = parentBody.transform.TransformPoint(position);
                velocity += parentBody.GetVelocity();
                // TODO: Fix velocity to point in correct direction
            }
            else if (parentBody != null)
            {
                Helper.helper.Console.WriteLine($"{target}: Kepler2: {parentBody} {DisplayConsole.logVector(parentBody.GetPosition())} {DisplayConsole.logVector(parentBody.GetVelocity())}");
                position += parentBody.GetPosition();
                velocity += parentBody.GetVelocity();
            }
            else
            {
                Helper.helper.Console.WriteLine($"{target}: Kepler3");
                position += Locator.GetCenterOfTheUniverse()?.GetOffsetPosition() ?? Vector3.zero;
                velocity += Locator.GetCenterOfTheUniverse()?.GetOffsetVelocity() ?? Vector3.zero;
            }

            applyMovement(parentBody, target, position, velocity, acceleration);
            applyOrientation(parentBody, target, position);
        }

        private void applySurfaceState(OWRigidbody parentBody, OWRigidbody target)
        {
            var position = this.surfacePosition.Value;
            var velocity = this.surfaceVelocity.Value;
            var acceleration = this.surfaceAcceleration.Value;

            Helper.helper.Console.WriteLine($"{target}: Surface: {DisplayConsole.logVector(position)} {DisplayConsole.logVector(velocity)} {DisplayConsole.logVector(acceleration)}");

            position = parentBody.transform.TransformPoint(position);
            velocity += parentBody.GetPointVelocity(position);
            acceleration += parentBody.GetPointAcceleration(position);

            applyMovement(parentBody, target, position, velocity, acceleration);
            applyOrientation(parentBody, target, position);
        }

        private void applyRelativeState(OWRigidbody parentBody, OWRigidbody target)
        {
            var position = this.position.Value;
            var velocity = this.velocity.Value;
            var acceleration = this.acceleration.Value;

            Helper.helper.Console.WriteLine($"{target}: Relative: {DisplayConsole.logVector(position)} {DisplayConsole.logVector(velocity)} {DisplayConsole.logVector(acceleration)}");

            if (parentBody != null)
            {
                position += parentBody.GetPosition();
                velocity += parentBody.GetVelocity();
                acceleration += parentBody.GetAcceleration();
            }
            else
            {
                position += Locator.GetCenterOfTheUniverse()?.GetOffsetPosition() ?? Vector3.zero;
                velocity += Locator.GetCenterOfTheUniverse()?.GetOffsetVelocity() ?? Vector3.zero;
            }

            applyMovement(parentBody, target, position, velocity, acceleration);
            applyOrientation(parentBody, target, position);
        }


        private void applyOrientation(OWRigidbody parentBody, OWRigidbody target, Vector3 worldPosition)
        {
            if (parentBody != null
                && surfraceOrientation.HasValue
                && surfaceAngularVelocity.HasValue
                && surfaceAngularAcceleration.HasValue)
            {
                // TODO: this
                var rotation = surfraceOrientation.Value * parentBody.GetRotation();
                var velocity = surfaceAngularVelocity.Value;

                applyRotation(parentBody, target, rotation, velocity, Vector3.zero);
            }
            else if (orientation.HasValue
                && angularVelocity.HasValue
                && angularAcceleration.HasValue)
            {
                var rotation = orientation.Value;
                var velocity = angularVelocity.Value;

                applyRotation(parentBody, target, rotation, velocity, Vector3.zero);
            }
        }

        private void applyMovement(OWRigidbody parentBody, OWRigidbody target, Vector3 position, Vector3 velocity, Vector3 acceleration)
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
                target.SetValue("_suspensionBody", parentBody);
                target.SetValue("_cachedRelativeVelocity", parentBody == null ? Vector3.zero : parentBody.transform.InverseTransformDirection(velocity - parentBody.GetPointVelocity(position)));
            }
        }

        private void applyRotation(OWRigidbody parentBody, OWRigidbody target, Quaternion orientation, Vector3 angularVelocity, Vector3 angularAcceleration)
        {
            target.SetRotation(new Quaternion(orientation.x, orientation.y, orientation.z, orientation.w));
            target.SetAngularVelocity(new Vector3(angularVelocity.x, angularVelocity.y, angularVelocity.z));

            target.SetValue("_currentAngularVelocity", new Vector3(angularVelocity.x, angularVelocity.y, angularVelocity.z));
            target.SetValue("_lastAngularVelocity", new Vector3(angularVelocity.x, angularVelocity.y, angularVelocity.z));

            if (target.IsSuspended())
            {
                target.SetValue("_cachedAngularVelocity", angularVelocity);
            }
        }

        public static MovementState fromGlobal(Position.HeavenlyBodies parent, Vector3 position, Vector3 velocity, Vector3 acceleration, Quaternion orientation, Vector3 angularVelocity, Vector3 angularAcceleration)
        {
            Vector3? surfacePosition = null;
            Vector3? surfaceVelocity = null;
            Vector3? surfaceAcceleration = null;
            Quaternion? surfraceOrientation = null;
            Vector3? surfaceAngularVelocity = null;
            Vector3? surfaceAngularAcceleration = null;
            Orbit.KeplerCoordinates orbit = null;

            var parentBody = Position.getBody(parent);
            if (parentBody == null)
            {
                parent = Position.HeavenlyBodies.None;
                position -= Locator.GetCenterOfTheUniverse()?.GetOffsetPosition() ?? Vector3.zero;
                velocity -= Locator.GetCenterOfTheUniverse()?.GetOffsetVelocity() ?? Vector3.zero;
            }
            else
            {
                var gravity = Position.getGravity(parent);
                var size = Position.getSize(parent);

                var worldPosition = position;

                position -= parentBody.GetPosition();
                velocity -= parentBody.GetVelocity();
                acceleration -= parentBody.GetAcceleration();

                var distance = position.sqrMagnitude;

                if (distance < (size.influence * size.influence))
                {
                    surfacePosition = parentBody.transform.InverseTransformPoint(worldPosition);
                    surfaceVelocity = velocity - parentBody.GetPointVelocity(worldPosition);
                    surfaceAcceleration = acceleration - parentBody.GetPointAcceleration(worldPosition);
                    surfraceOrientation = parentBody.transform.InverseTransformRotation(orientation);
                    surfaceAngularVelocity = Vector3.zero; // TODO
                    surfaceAngularAcceleration = Vector3.zero; // TODO
                }

                if ((size.size * size.size) < distance && distance < (size.influence * size.influence))
                {
                    var kepler = Orbit.toKeplerCoordinates(gravity, Time.timeSinceLevelLoad, position, velocity);
                    if (kepler != null && kepler.isOrbit())
                    {
                        var apoapsis = kepler.semiMajorRadius + kepler.foci;
                        var periapsis = kepler.semiMajorRadius - kepler.foci;
                        if (size.size < periapsis && apoapsis < size.influence)
                        {
                            orbit = kepler;
                        }
                    }
                }
            }

            return new MovementState(parent, position, velocity, acceleration, orientation, angularVelocity, angularAcceleration, surfaceAcceleration, surfaceVelocity, surfaceAcceleration, surfraceOrientation, surfaceAngularVelocity, surfaceAngularAcceleration, orbit);
        }

        public static MovementState fromRelative(Position.HeavenlyBodies parent, Vector3 position, Vector3 velocity, Vector3 acceleration, Quaternion orientation, Vector3 angularVelocity, Vector3 angularAcceleration)
        {
            return new MovementState(parent, position, velocity, acceleration, orientation, angularVelocity, angularAcceleration, null, null, null, null, null, null, null);
        }

        public static MovementState fromSurface(Position.HeavenlyBodies parent, Vector3 surfacePosition, Vector3 surfaceVelocity, Vector3 surfaceAcceleration, Quaternion surfraceOrientation, Vector3 surfaceAngularVelocity, Vector3 surfaceAngularAcceleration)
        {
            return new MovementState(parent, null, null, null, null, null, null, surfacePosition, surfaceVelocity, surfaceAcceleration, surfraceOrientation, surfaceAngularVelocity, surfaceAngularAcceleration, null);
        }

        public static MovementState fromKepler(Position.HeavenlyBodies parent, Orbit.KeplerCoordinates kepler, Quaternion orientation, Vector3 angularVelocity, Vector3 angularAcceleration)
        {
            if (kepler != null && kepler.isOrbit())
            {
                return new MovementState(parent, null, null, null, orientation, angularVelocity, angularAcceleration, null, null, null, null, null, null, kepler);
            }
            return null;
        }

        public static MovementState fromCurrentState(Position.HeavenlyBodies parent, OWRigidbody target)
        {
            var position = target.GetPosition();
            var velocity = target.GetVelocity();
            var acceleration = target.GetAcceleration();
            var orientation = target.GetRotation();
            var angularVelocity = target.GetAngularVelocity();
            var angularAcceleration = target.GetAngularAcceleration();

            return fromGlobal(parent, position, velocity, acceleration, orientation, angularVelocity, angularAcceleration);
        }

        public static MovementState fromClosetInfluence(OWRigidbody target, params Position.HeavenlyBodies[] exclude)
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
