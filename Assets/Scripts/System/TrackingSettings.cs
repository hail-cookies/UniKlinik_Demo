using UnityEngine;

[CreateAssetMenu(fileName = "new TrackingSettings", menuName = "ScriptableObjects/Tracking Settings")]
public class TrackingSettings : ScriptableObject
{
    public TrackingSystem.UpdateCall updateCall = TrackingSystem.UpdateCall.FixedUpdate;
    public TrackingMode positionTracking = TrackingMode.Velocity;
    public TrackingMode rotationTracking = TrackingMode.Velocity;
    public LockAxis positionLock, rotationLock;
    public float linearForceLimit = 10;
    public float angularForceLimit = 20;
    public int maxTrackedBodies = 1;

    public Tracker Apply(Rigidbody body)
    {
        return new Tracker(
            body,
            updateCall,
            positionTracking,
            rotationTracking,
            positionLock,
            rotationLock,
            linearForceLimit,
            angularForceLimit,
            maxTrackedBodies);
    }
}
