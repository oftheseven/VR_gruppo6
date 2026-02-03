using UnityEngine;

public class ArmSnapshot
{
    public float timestamp; // tempo in cui è stato preso lo snapshot

    // rotazioni dei pivot del braccio
    public Quaternion mechanicalArmRotation;
    public Quaternion pivot1Rotation;
    public Quaternion pivot2Rotation;

    public Vector3 tipPosition; // posizione della punta del braccio

    public ArmSnapshot(float timestamp, Quaternion mechanicalArmRotation, Quaternion pivot1Rotation, Quaternion pivot2Rotation, Vector3 tipPosition)
    {
        this.timestamp = timestamp;
        this.mechanicalArmRotation = mechanicalArmRotation;
        this.pivot1Rotation = pivot1Rotation;
        this.pivot2Rotation = pivot2Rotation;
        this.tipPosition = tipPosition;
    }
}