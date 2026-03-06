using UnityEngine;

/// <summary>
/// Maps IMU pitch/roll from the hidden left arm / residual stump
/// to grip pose animations on the prosthetic hand.
///
/// Finger bones in this model: Bone.006 through Bone.025
/// (5 fingers × 4 bones each, branching from Bone.005/wrist)
/// The Animator Controller drives these via clip-based animation.
///
/// ── Animator Controller setup ─────────────────────────────────────────────
/// 1. Create an Animator Controller asset (Assets → Create → Animator Controller)
/// 2. Open it (double-click)
/// 3. Add Integer parameter named: GripState
/// 4. Create 3 states:
///      Neutral (default) — hand open/resting
///      PowerGrip         — fist closed
///      Pinch             — thumb + index only
/// 5. Add transitions: Any State → each state, condition GripState equals 0/1/2
///    Uncheck Has Exit Time on all transitions, set transition duration to 0.15
/// 6. Assign this controller to the prosthetic prefab's Animator component
///
/// ── IMU → Grip mapping ────────────────────────────────────────────────────
///   Stump tilts forward  (pitch > threshold)  → Power grip (fist)
///   Stump tilts back     (pitch < -threshold) → Neutral (open)
///   Stump rotates CW     (roll  > threshold)  → Pinch
/// </summary>
public class IMUGripController : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [SerializeField] BLEManager bleManager;

    [Header("Thresholds (degrees from baseline)")]
    [SerializeField] float pitchForwardThreshold = 25f;
    [SerializeField] float pitchBackThreshold    = -25f;
    [SerializeField] float rollCWThreshold       = 25f;

    [Header("Deadzone — prevents flickering near threshold")]
    [SerializeField] float deadzone = 5f;

    [Header("Debug overlay in Game view")]
    [SerializeField] bool showDebug = true;

    // Animator is assigned at runtime by AboveElbowProstheticAttacher
    Animator prostheticAnimator;
    readonly int gripStateId = Animator.StringToHash("GripState");

    const int GRIP_NEUTRAL = 0;
    const int GRIP_POWER   = 1;
    const int GRIP_PINCH   = 2;

    float baselinePitch = 0f;
    float baselineRoll  = 0f;
    bool  baselineSet   = false;
    int   currentGrip   = GRIP_NEUTRAL;
    int   lastGrip      = -1;

    public float RelativePitch { get; private set; }
    public float RelativeRoll  { get; private set; }

    void Update()
    {
        if (bleManager == null) return;

        RelativePitch = bleManager.Pitch - (baselineSet ? baselinePitch : 0f);
        RelativeRoll  = bleManager.Roll  - (baselineSet ? baselineRoll  : 0f);

        // Determine grip with hysteresis
        if      (RelativePitch >  pitchForwardThreshold + deadzone) currentGrip = GRIP_POWER;
        else if (RelativePitch <  pitchBackThreshold    - deadzone) currentGrip = GRIP_NEUTRAL;
        else if (RelativeRoll  >  rollCWThreshold       + deadzone) currentGrip = GRIP_PINCH;
        else if (Mathf.Abs(RelativePitch) < pitchForwardThreshold - deadzone &&
                 Mathf.Abs(RelativeRoll)  < rollCWThreshold       - deadzone)
            currentGrip = GRIP_NEUTRAL;

        if (currentGrip != lastGrip)
        {
            lastGrip = currentGrip;
            if (prostheticAnimator != null)
                prostheticAnimator.SetInteger(gripStateId, currentGrip);

            Debug.Log($"[IMUGrip] → {GripName(currentGrip)}");
        }
    }

    // Called from Calibration scene button
    public void SetBaseline()
    {
        if (bleManager == null) return;
        baselinePitch = bleManager.Pitch;
        baselineRoll  = bleManager.Roll;
        baselineSet   = true;
        Debug.Log($"[IMUGrip] Baseline set pitch:{baselinePitch:F1} roll:{baselineRoll:F1}");
    }

    // Called by AboveElbowProstheticAttacher after prefab is spawned
    public void SetAnimator(Animator anim) => prostheticAnimator = anim;

    void OnGUI()
    {
        if (!showDebug) return;
        GUI.Box(new Rect(10, 10, 220, 90), "IMU Grip");
        GUI.Label(new Rect(20, 32, 200, 20), $"Pitch: {RelativePitch:F1}°");
        GUI.Label(new Rect(20, 52, 200, 20), $"Roll:  {RelativeRoll:F1}°");
        GUI.Label(new Rect(20, 72, 200, 20), $"Grip:  {GripName(currentGrip)}");
    }

    static string GripName(int id) => id switch
    {
        GRIP_POWER   => "Power (fist)",
        GRIP_PINCH   => "Pinch",
        _            => "Neutral (open)"
    };
}
