using UnityEngine;

public class SimulatedIMU : MonoBehaviour, IIMUInput
{
    [Range(-90f, 90f)]
    public float pitch;

    [Range(-90f, 90f)]
    public float roll;

    public float GetPitch() => pitch;
    public float GetRoll() => roll;
}