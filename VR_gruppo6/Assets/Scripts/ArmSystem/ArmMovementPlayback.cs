using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArmMovementPlayback : MonoBehaviour
{
    // singleton
    private static ArmMovementPlayback _instance;
    public static ArmMovementPlayback instance => _instance;
}