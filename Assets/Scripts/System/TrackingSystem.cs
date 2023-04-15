using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1900)]
public class TrackingSystem : MonoBehaviour
{
    public enum UpdateCall { Update, FixedUpdate }

    static bool _hasInstance = false;
    static TrackingSystem _instance;
    static TrackingSystem CreateInstance()
    {
        if (!_hasInstance)
        {
            GameObject go = new GameObject("TrackingSystem");
            _instance = go.AddComponent<TrackingSystem>();
            _hasInstance = true;
        }

        return _instance;
    }

    static List<Tracker> update = new List<Tracker>();
    static List<Tracker> fixedUpdate = new List<Tracker>();
    public static void AddTracker(Tracker tracker, UpdateCall updateCall)
    {
        CreateInstance();

        int index = TryGetTracker(tracker.body, updateCall, out var result);
        if (index != -1)
            TrySetTracker(index, updateCall, tracker);
        else
            (updateCall == UpdateCall.Update ? update : fixedUpdate).Add(tracker);
    }

    public static int TryGetTracker(Tracker tracker, out UpdateCall updateCall)
    {
        updateCall = UpdateCall.Update;
        int index = TryGetTracker(tracker.body, updateCall, out var result);
        if(index != -1)
            return index;

        updateCall = UpdateCall.FixedUpdate;
        index = TryGetTracker(tracker.body, updateCall, out result);
        if (index != -1)
            return index;

        return -1;
    }

    public static int TryGetTracker(Rigidbody body, UpdateCall updateCall, out Tracker tracker)
    {
        List<Tracker> list = updateCall == UpdateCall.Update ? update : fixedUpdate;
        for(int i = 0; i < list.Count; i++)
        {
            tracker = list[i];
            if (tracker.body == body)
                return i;
        }

        tracker = null;
        return -1;
    }

    public static bool TrySetTracker(int index, UpdateCall updateCall, Tracker value)
    {
        List<Tracker> list = updateCall == UpdateCall.Update ? update : fixedUpdate;
        if (index < 0 || index >= list.Count)
            return false;
        list[index] = value;

        return true;
    }

    public static void RemoveTracker(Tracker tracker) =>
        RemoveTracker(TryGetTracker(tracker, out var updateCall), updateCall);

    public static void RemoveTracker(int index, UpdateCall updateCall)
    {
        List<Tracker> list = updateCall == UpdateCall.Update ? update : fixedUpdate;
        if (index < 0 || index >= list.Count)
            return;
        list.RemoveAt(index);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        foreach (var tracker in update)
            tracker.Track(dt);
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        foreach (var tracker in fixedUpdate)
            tracker.Track(dt);
    }
}
