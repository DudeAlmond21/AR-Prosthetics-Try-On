using UnityEngine;

public class MotionInterpreter : MonoBehaviour
{
    [Header("IMU Source")]
    public MonoBehaviour imuSource;
    private IIMUInput imu;

    [Header("Threshold Settings")]
    public float forwardThreshold = 45f;
    public float sideThreshold = 45f;
    public float deadZone = 5f;

    [Header("Timing")]
    public float postReturnDelay = 2f;
    public float cooldownDuration = 1.5f;

    private float baselinePitch;
    private float baselineRoll;

    private ProstheticAnimationController animationController;

    private bool forwardRaised = false;
    private bool sideRaised = false;

    private bool waitingToTrigger = false;
    private GestureType pendingGesture = GestureType.None;
    private float delayTimer = 0f;

    private float cooldownTimer = 0f;

    void Start()
    {
        imu = imuSource as IIMUInput;
        animationController = GetComponent<ProstheticAnimationController>();

        if (imu != null)
            Calibrate();
    }

    public void Calibrate()
    {
        if (imu == null) return;

        baselinePitch = imu.GetPitch();
        baselineRoll = imu.GetRoll();
    }

    void Update()
    {
        if (imu == null || animationController == null)
            return;

        // -------------------------------------------------
        // MODE GUARD (Only block if ModeManager exists)
        // -------------------------------------------------
        if (TryOnModeManager.Instance != null)
        {
            if (TryOnModeManager.Instance.currentMode != TryOnMode.Gesture)
            {
                return; // Do nothing in Follow mode
            }
        }

        // -------------------------------------------------
        // Cooldown handling
        // -------------------------------------------------
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        // -------------------------------------------------
        // Delayed trigger handling
        // -------------------------------------------------
        if (waitingToTrigger)
        {
            delayTimer -= Time.deltaTime;

            if (delayTimer <= 0f)
            {
                animationController.PlayGesture(pendingGesture);
                waitingToTrigger = false;
                cooldownTimer = cooldownDuration;
            }

            return;
        }

        float deltaPitch = imu.GetPitch() - baselinePitch;
        float deltaRoll = imu.GetRoll() - baselineRoll;

        bool inDeadZone =
            Mathf.Abs(deltaPitch) < deadZone &&
            Mathf.Abs(deltaRoll) < deadZone;

        // -------------------------------------------------
        // FORWARD GESTURE (Raise → Return → Delay → Play)
        // -------------------------------------------------
        if (!forwardRaised && deltaPitch > forwardThreshold)
        {
            forwardRaised = true;
        }

        if (forwardRaised && inDeadZone)
        {
            forwardRaised = false;
            waitingToTrigger = true;
            pendingGesture = GestureType.Forward;
            delayTimer = postReturnDelay;
            return;
        }

        // -------------------------------------------------
        // SIDE GESTURE
        // -------------------------------------------------
        if (!sideRaised && deltaRoll > sideThreshold)
        {
            sideRaised = true;
        }

        if (sideRaised && inDeadZone)
        {
            sideRaised = false;
            waitingToTrigger = true;
            pendingGesture = GestureType.Side;
            delayTimer = postReturnDelay;
            return;
        }

        // -------------------------------------------------
        // Idle when neutral
        // -------------------------------------------------
        if (inDeadZone)
        {
            animationController.PlayGesture(GestureType.None);
        }
    }
}