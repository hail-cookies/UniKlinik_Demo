using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

[System.Flags]
public enum LockAxis
{
    All = ~0,
    X = 1,
    Y = 2,
    Z = 4
}

public enum TrackingMode { Velocity, Instant }
public enum EnumAxis { X, Y, Z }
public enum EnumCompare { Equal, Greater, GreaterOrEqual, Less, LessOrEqual }
public enum FilterType { Include, Exclude }
public enum Hand { left, right, both }

public class Helper
{
    public static float FloorOffset = 0;

    public static void MovePlaySpace(Transform playSpace, Transform camera, Vector3 targetPosition)
    {
        Vector3 offset = playSpace.position - camera.position;
        offset.y = FloorOffset;

        playSpace.position = targetPosition + offset;
    }

    public static void RotatePlaySpace(Transform playSpace, Transform camera, float targetRotation, Vector3 axis)
    {
        Vector3 fwd = Vector3.ProjectOnPlane(camera.forward, axis);
        Vector3 trg = Quaternion.AngleAxis(targetRotation, axis) * Vector3.forward;
        playSpace.RotateAround(camera.position, axis, Vector3.SignedAngle(fwd, trg, axis));
    }

    public static float GetOrientation(Transform camera, Vector3 axis)
    {
        return Vector3.SignedAngle(Vector3.forward, Vector3.ProjectOnPlane(camera.forward, axis), axis);
    }

    public static int ArcCast(Vector3[] arc, LayerMask layerMask, out RaycastHit hit)
    {
        hit = new RaycastHit();
        if (arc.Length <= 1)
            return -1;

        for (int i = 1; i < arc.Length; i++)
        {
            Vector3 org = arc[i - 1];
            Vector3 del = arc[i] - org;
            float dst = del.magnitude;
            Vector3 dir = del / dst;

            if (Physics.Raycast(
                org,
                dir,
                out hit,
                dst,
                layerMask))
            {
                return i;
            }
        }

        return -1;
    }

    public static float RandomRange(Vector2 range)
    {
        float min = Mathf.Min(range.x, range.y);
        float max = Mathf.Max(range.x, range.y);

        return Random.Range(min, max);
    }

    public static float ClampCircle(float value, float min, float max)
    {
        float delta = -min + (min > max ? 360 : 0);

        value = LoopValue(value + delta, 0, 360);

        if (min == max)
            return value - delta;
        if (0 <= value && value <= max + delta)
            return value - delta;
        else
            return
                Mathf.Abs(LoopDelta(0, 360, 0, value).x) < 
                Mathf.Abs(LoopDelta(0, 360, max + delta, value).x) ?
                min : max;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="current"></param>
    /// <param name="target"></param>
    /// <returns>Vector2(short path, long path)</returns>
    public static Vector2 LoopDelta(float min, float max, float current, float target)
    {
        if (min > max)
        {
            float swap = min;
            min = max;
            max = swap;
        }

        float delta = max - min;
        if (delta == 0)
            return Vector2.zero;

        current = LoopValue(current, min, max);
        target = LoopValue(target, min, max);
        float sign = current < target ? 1 : -1;

        float direct = Mathf.Abs(target - current);
        float looped = Mathf.Abs(target - (delta * sign) - current);

        return direct < looped ?
            new Vector2(direct * sign, looped * -sign) :
            new Vector2(looped * -sign, direct * sign);
    }

    public static float LoopValue(float value, float min, float max)
    {
        if(min > max)
        {
            float swap = min;
            min = max;
            max = swap;
        }

        if (value >= min && value <= max)
            return value;

        float delta = max - min;
        if (delta == 0)
            return 0;

        int error = (int)((value - min) / delta);
        float corrected = value - (error * delta);
        return value < min ? corrected + delta : corrected;
    }

    public static Quaternion GetOffsetRotation(Quaternion from, Quaternion to) => Quaternion.Inverse(to) * from;
    public static Vector3 GetOffsetPosition(Vector3 from, Vector3 to, Quaternion rotation) => Quaternion.Inverse(rotation) * (to - from);

    public static Quaternion AngularToQuaternion(Vector3 angularVelocity)
    {
        return Quaternion.Euler(
            Mathf.Rad2Deg * angularVelocity.x,
            Mathf.Rad2Deg * angularVelocity.y,
            Mathf.Rad2Deg * angularVelocity.z);
    }

    public static Mesh BuildQuad(float width, float height)
    {
        Mesh mesh = new Mesh();

        // Setup vertices
        Vector3[] newVertices = new Vector3[4];
        float halfHeight = height * 0.5f;
        float halfWidth = width * 0.5f;
        newVertices[0] = new Vector3(-halfWidth, -halfHeight, 0);
        newVertices[1] = new Vector3(-halfWidth, halfHeight, 0);
        newVertices[2] = new Vector3(halfWidth, -halfHeight, 0);
        newVertices[3] = new Vector3(halfWidth, halfHeight, 0);

        // Setup UVs
        Vector2[] newUVs = new Vector2[newVertices.Length];
        newUVs[0] = new Vector2(0, 0);
        newUVs[1] = new Vector2(0, 1);
        newUVs[2] = new Vector2(1, 0);
        newUVs[3] = new Vector2(1, 1);

        // Setup triangles
        int[] newTriangles = new int[] { 0, 1, 2, 3, 2, 1 };

        // Setup normals
        Vector3[] newNormals = new Vector3[newVertices.Length];
        for (int i = 0; i < newNormals.Length; i++)
        {
            newNormals[i] = Vector3.forward;
        }

        // Create quad
        mesh.vertices = newVertices;
        mesh.uv = newUVs;
        mesh.triangles = newTriangles;
        mesh.normals = newNormals;

        return mesh;
    }

    public static Quaternion MirrorRotation(Quaternion rotation, Vector3 normal)
    {
        float angle;
        Vector3 axis;
        rotation.ToAngleAxis(out angle, out axis);
        axis = Vector3.Reflect(axis, normal);

        return Quaternion.AngleAxis(angle, axis);
    }

    /// <summary>
    /// Removes all empty layers from a mask
    /// </summary>
    /// <param name="label"></param>
    /// <param name="layerMask"></param>
    /// <returns></returns>
    public static int CompressLayerMask(LayerMask layerMask, out List<string> options, out List<int> layerNumbers)
    {
        options = new List<string>();
        layerNumbers = new List<int>();

        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            if (!string.IsNullOrEmpty(layerName))
            {
                options.Add(layerName);
                layerNumbers.Add(i);
            }
        }

        int maskWithoutEmpty = 0;
        for (int i = 0; i < layerNumbers.Count; i++)
        {
            if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                maskWithoutEmpty |= (1 << i);
        }

        return maskWithoutEmpty;
    }

    /// <summary>
    /// Restores a compressed mask, to a full mask
    /// </summary>
    /// <param name="mask"></param>
    /// <param name="layerNumbers"></param>
    /// <returns></returns>
    public static int RestoreLayerMask(int mask, List<int> layerNumbers)
    {
        int result = 0;
        for (int i = 0; i < layerNumbers.Count; i++)
        {
            if ((mask & (1 << i)) > 0)
                result |= (1 << layerNumbers[i]);
        }

        for (int i = 0; i < 32; i++)
            if (!layerNumbers.Contains(i))
                result |= (1 << i);

        return result;
    }

    /// <summary>
    /// Change the layer of the target object and all of its children
    /// </summary>
    /// <param name="target"></param>
    /// <param name="layer"></param>
    public static void SetLayerRecursively(Transform target, int layer, int ignore = -1)
    {
        if(target.gameObject.layer != ignore)
            target.gameObject.layer = layer;

        int count = target.transform.childCount;
        for (int i = 0; i < count; i++)
            SetLayerRecursively(target.transform.GetChild(i), layer);
    }

    /// <summary>
    /// Returns the simple average of the provided vectors component wise
    /// </summary>
    /// <param name="vectors"></param>
    /// <returns></returns>
    public static Vector3 AverageVector3(List<Vector3> vectors)
    {
        Vector3 result = Vector3.zero;
        int c = vectors.Count;

        if (c <= 0)
            return result;

        int a = 0;
        for (int i = 0; i < c; i++, a++)
        {
            result += vectors[i];
            if (i > c * 0.2f && i < c * 0.8f)
            {
                result += vectors[i];
                a++;
            }
        }
        return result / (vectors.Count + a);
    }

    /// <summary>
    /// Reduces all NaN components of the provided vector to the fallback value
    /// </summary>
    /// <param name="vector">Input vector</param>
    /// <param name="fallback">All NaN values will be replaced with this</param>
    /// <returns></returns>
    public static Vector3 Vector3ReduceNaN(Vector3 vector, float fallback)
    {
        if (float.IsNaN(vector.x))
            vector.x = fallback;
        if (float.IsNaN(vector.y))
            vector.y = fallback;
        if (float.IsNaN(vector.z))
            vector.z = fallback;

        return vector;
    }

    public static Vector3 TrackLinearVelocity(Vector3 position, Vector3 targetPosition, float dt, float forceLimit)
    {
        return Vector3.ClampMagnitude((targetPosition - position) / dt, forceLimit);
    }

    public static Vector3 TrackAngularVelocity(Quaternion rotation, Quaternion targetRotation, float dt)
    {
        //Track angular velocity and compensate
        // Rotations stack right to left,
        // so first we undo our rotation, then apply the target.
        Quaternion rot = targetRotation * Quaternion.Inverse(rotation);

        float angle; Vector3 axis;
        rot.ToAngleAxis(out angle, out axis);

        // We get an infinite axis in the event that our rotation is already aligned.
        if (!float.IsInfinity(axis.x))
        {
            if (angle > 180f)
                angle -= 360f;

            return (0.9f * Mathf.Deg2Rad * angle / dt) * axis.normalized;
        }

        return Vector3.zero;
    }

    public static List<InputDevice> GetController(Hand hand)
    {
        InputDeviceCharacteristics characteristics =
                    InputDeviceCharacteristics.HeldInHand |
                    InputDeviceCharacteristics.TrackedDevice |
                    InputDeviceCharacteristics.Controller |
                    (hand == Hand.left ? InputDeviceCharacteristics.Left : 
                    hand == Hand.right ? InputDeviceCharacteristics.Right : InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Right);

        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(characteristics, devices);

        return devices;
    }

    public static Vector3 MultiplyVector3(Vector3 a, Vector3 b)
    {
        return new Vector3(
            a.x * b.x,
            a.y * b.y,
            a.z * b.z);
    }

    public static bool InsideEllipse(Vector2 ellipse, Vector2 center, Vector2 point) =>
        CheckEllipse(ellipse, center, point) <= 1;

    public static float CheckEllipse(Vector2 ellipse, Vector2 center, Vector2 point)
    {
        float left = Mathf.Pow(point.x - center.x, 2) / (ellipse.x * ellipse.x);
        float right = Mathf.Pow(point.y - center.y, 2) / (ellipse.y * ellipse.y);

        return left + right;
    }

    public static Vector2[] DrawEllipse(Vector2 ellipse, Vector2 center, int resolution)
    {
        resolution += 1;
        Vector2[] result = new Vector2[resolution * 4];

        int sec2 = resolution * 2, sec4 = resolution * 4;
        float step = 0.5f * Mathf.PI / (resolution - 1);
        float r = ellipse.x > ellipse.y ? ellipse.x : ellipse.y;
        float rx = r == ellipse.x ? 1 : ellipse.x / ellipse.y;
        float ry = r == ellipse.y ? 1 : ellipse.y / ellipse.x;

        for(int index = 0; index < resolution; index++)
        {
            float angle = step * index;
            Vector2 point = new Vector2(
                r * rx * Mathf.Cos(angle),
                r * ry * Mathf.Sin(angle));

            //Top right
            result[index] = point + center;
            //Top left
            point.x *= -1;
            result[sec2 - 1 - index] = point + center;
            //Bottom left
            point.y *= -1;
            result[index + sec2] = point + center;
            //Bottom right
            point.x *= -1;
            result[sec4 - 1 - index] = point + center;
        }

        return result;
    }

    public static Vector2 GetPointOnEllipse(Vector2 ellipse, Vector2 center, float rad)
    {
        float r = ellipse.x > ellipse.y ? ellipse.x : ellipse.y;
        float rx = r == ellipse.x ? 1 : ellipse.x / ellipse.y;
        float ry = r == ellipse.y ? 1 : ellipse.y / ellipse.x;

        return
            new Vector2(
                r * rx * Mathf.Cos(rad),
                r * ry * Mathf.Sin(rad)) + center;
    }
}
