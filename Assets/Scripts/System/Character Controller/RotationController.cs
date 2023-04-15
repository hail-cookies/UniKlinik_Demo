using UnityEngine;

public class RotationController : MonoBehaviour
{
    public Vector2 LimitsX, LimitsY, LimitsZ;

    float _x, _y, _z;
    public float X
    {
        get => _x;
        set 
        {
            LimitsX = new Vector2(
                Helper.LoopValue(LimitsX.x, 0, 360), 
                Helper.LoopValue(LimitsX.y, 0, 360));
            
            _x = Helper.ClampCircle(value, LimitsX.x, LimitsX.y);

            ApplyRotation();
        }
    }

    public float Y
    {
        get => _y;
        set 
        {
            LimitsY = new Vector2(
                Helper.LoopValue(LimitsY.x, 0, 360),
                Helper.LoopValue(LimitsY.y, 0, 360));

            _y = Helper.ClampCircle(value, LimitsY.x, LimitsY.y);

            ApplyRotation();
        }
    }

    public float Z
    {
        get => _z;
        set 
        {
            LimitsZ = new Vector2(
                Helper.LoopValue(LimitsZ.x, 0, 360),
                Helper.LoopValue(LimitsZ.y, 0, 360));

            _z = Helper.ClampCircle(value, LimitsZ.x, LimitsZ.y);

            ApplyRotation();
        }
    }

    public Vector3 EulerAngles
    {
        get { return new Vector3(_x, _y, _z); }
        set
        {
            X = value.x;
            Y = value.y;
            Z = value.z;
        }
    }

    void ApplyRotation()
    {
        transform.eulerAngles = EulerAngles;
    }
}
