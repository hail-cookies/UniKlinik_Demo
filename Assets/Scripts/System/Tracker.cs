using System.Collections.Generic;
using UnityEngine;

public class Tracker
{
    [System.Serializable]
    public class TrackedRigidBody
    {
        public Rigidbody body;
        public Vector3 offsetPosition;
        public Quaternion offsetRotation;

        public TrackedRigidBody(
            Rigidbody body,
            Vector3 offsetPosition,
            Quaternion offsetRotation)
        {
            this.body = body;
            this.offsetPosition = offsetPosition;
            this.offsetRotation = offsetRotation;
        }
    }

    public Rigidbody body;
    public TrackingSystem.UpdateCall updateCall;
    public TrackingMode positionTracking = TrackingMode.Velocity;
    public TrackingMode rotationTracking = TrackingMode.Velocity;
    public LockAxis positionLock, rotationLock;
    public float linearForceLimit = 10;
    public float angularForceLimit = 20;
    public int maxTrackedBodies = 1;

    List<TrackedRigidBody> trackedRigidBodies = new List<TrackedRigidBody>();

    public Tracker(
        Rigidbody body,
        TrackingSystem.UpdateCall updateCall,
        TrackingMode positionTracking,
        TrackingMode rotationTracking,
        LockAxis positionLock,
        LockAxis rotationLock,
        float linearForceLimit,
        float angularForceLimit,
        int maxTrackedBodies)
    {
        this.body = body;
        this.positionTracking = positionTracking;
        this.rotationTracking = rotationTracking;
        this.positionLock = positionLock;
        this.rotationLock = rotationLock;
        this.linearForceLimit = linearForceLimit;
        this.angularForceLimit = angularForceLimit;
        this.maxTrackedBodies = maxTrackedBodies;
        this.updateCall = updateCall;
    }

    public bool IsTracking => trackedRigidBodies.Count > 0;
    public int Count => trackedRigidBodies.Count;
    public TrackedRigidBody GetTrackedRigidBody(int index) => trackedRigidBodies[index];

    public bool StartTracking(Rigidbody target) => StartTracking(
        target,
        Helper.GetOffsetPosition(target.position, body.position, target.rotation),
        Helper.GetOffsetRotation(body.rotation, target.rotation));

    public bool StartTracking(Rigidbody target, Vector3 offsetPosition) => StartTracking(
        target,
        offsetPosition,
        Helper.GetOffsetRotation(body.rotation, target.rotation));

    public bool StartTracking(Rigidbody target, Quaternion offsetRotation) => StartTracking(
        target,
        Helper.GetOffsetPosition(target.position, body.position, target.rotation),
        offsetRotation);

    public bool StartTracking(Rigidbody target, Vector3 offsetPosition, Quaternion offsetRotation)
    {
        if (trackedRigidBodies == null)
            trackedRigidBodies = new List<TrackedRigidBody>();

        //If already tracked, update values and return true
        for (int i = 0; i < trackedRigidBodies.Count; i++)
        {
            if (target == trackedRigidBodies[i].body)
            {
                trackedRigidBodies[i] = new TrackedRigidBody(target, offsetPosition, offsetRotation);
                return true;
            }
        }

        //New target, but at full capacity
        if (trackedRigidBodies.Count >= maxTrackedBodies)
            return false;
        //Initialise Tracker
        if (trackedRigidBodies.Count == 0)
            TrackingSystem.AddTracker(this, updateCall);
        //Add target
        trackedRigidBodies.Add(new TrackedRigidBody(target, offsetPosition, offsetRotation));
        return true;
    }

    public void StopTracking(Rigidbody target)
    {
        if (trackedRigidBodies == null)
            trackedRigidBodies = new List<TrackedRigidBody>();

        for (int i = 0; i < trackedRigidBodies.Count; i++)
        {
            if (target == trackedRigidBodies[i].body)
            {
                trackedRigidBodies.RemoveAt(i);
                i--;
            }
        }

        if (trackedRigidBodies.Count == 0)
            TrackingSystem.RemoveTracker(this);
    }

    public void StopAllTracking()
    {
        while (trackedRigidBodies.Count > 0)
            StopTracking(trackedRigidBodies[0].body);
    }

    public void Track(float dt)
    {
        int count = trackedRigidBodies.Count;
        if (count == 0)
            return;

        Vector3 avgPos = Vector3.zero;
        Vector3 delRot = Vector3.zero;
        foreach (var trg in trackedRigidBodies)
        {
            Rigidbody trgBdy = trg.body;

            //Get Position
            Vector3 trgPos =
                trgBdy.position +                       //Current position
                (trgBdy.velocity * dt) +                //Future position
                (trgBdy.rotation * trg.offsetPosition); //Offset

            //Get Rotation
            Quaternion trgRot =
                Helper.AngularToQuaternion(trgBdy.angularVelocity) *    //Future Rotation
                trgBdy.rotation *                                       //Current Rotation
                trg.offsetRotation;                                     //Offset

            delRot += TrackRotation(dt, trgRot);
            avgPos += trgPos;
        }

        TrackPosition(dt, avgPos / count);

        if (rotationTracking == TrackingMode.Velocity)
        {
            body.maxAngularVelocity = angularForceLimit;
            body.angularVelocity = delRot;
        }
        else
            body.rotation = Quaternion.Euler(delRot) * body.rotation;
    }

    void TrackPosition(float dt, Vector3 trgPos)
    {
        Vector3 delta;

        //Calculate delta
        if (positionTracking == TrackingMode.Velocity)
        {
            delta = Helper.TrackLinearVelocity(body.position, trgPos, dt, linearForceLimit);
            delta += body.useGravity ? -Physics.gravity * dt * dt : Vector3.zero;
        }
        else
            delta = trgPos - body.position;

        //Ensure locks
        if ((positionLock & LockAxis.X) == LockAxis.X)
            delta.x = 0;
        if ((positionLock & LockAxis.Y) == LockAxis.Y)
            delta.y = 0;
        if ((positionLock & LockAxis.Z) == LockAxis.Z)
            delta.z = 0;

        //Apply changes
        if (positionTracking == TrackingMode.Velocity)
            body.velocity = delta;
        else
            body.position += delta;
    }

    Vector3 TrackRotation(float dt, Quaternion trgRot)
    {
        //Calculate delta
        Vector3 delta = rotationTracking == TrackingMode.Velocity ?
            Helper.TrackAngularVelocity(body.rotation, trgRot, dt) :
            (trgRot * Quaternion.Inverse(body.rotation)).eulerAngles;

        //Ensure Locks
        if ((rotationLock & LockAxis.X) == LockAxis.X)
            delta.x = 0;
        if ((rotationLock & LockAxis.Y) == LockAxis.Y)
            delta.y = 0;
        if ((rotationLock & LockAxis.Z) == LockAxis.Z)
            delta.z = 0;

        return delta;

        //if (rotationTracking == TrackingMode.Velocity)
        //{
        //    body.maxAngularVelocity = angularForceLimit;
        //    body.angularVelocity = delta;
        //}
        //else
        //    body.rotation = Quaternion.Euler(delta) * body.rotation;
    }
}
