using UnityEngine;

public class ArmWaypoint
{
    public Vector3 position;
    public Quaternion rotation;
    public float timestamp;
    public Quaternion[] jointRotations;
    
    public ArmWaypoint(Transform transform, float time)
    {
        position = transform.position;
        rotation = transform.rotation;
        timestamp = time;
        jointRotations = null;
    }
    
    public ArmWaypoint(Transform transform, float time, Quaternion[] joints)
    {
        position = transform.position;
        rotation = transform.rotation;
        timestamp = time;
        jointRotations = joints;
    }
    
    public float DistanceTo(ArmWaypoint other)
    {
        return Vector3.Distance(position, other.position);
    }
}