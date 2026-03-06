using UnityEngine;

/// <summary>
/// Attaches the rigged prosthetic full arm to the LEFT shoulder joint,
/// then hands off to MirrorArmController and IMUGripController.
///
/// Uses ARBodyJoint integer constants — no ARHumanBodyJointIndex needed.
/// </summary>
public class AboveElbowProstheticAttacher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] HumanBodyJointController bodyController;
    [SerializeField] MirrorArmController mirrorArmController;
    [SerializeField] IMUGripController imuGripController;

    [Header("Prosthetic Prefab")]
    [SerializeField] GameObject prostheticPrefab0;
    [SerializeField] GameObject prostheticPrefab1;

    [Header("Attachment Offset (tune in Editor with mock skeleton)")]
    [SerializeField] Vector3 positionOffset = Vector3.zero;
    [SerializeField] Vector3 rotationOffset = Vector3.zero;
    [SerializeField] float scaleMultiplier = 1f;

    [Header("Debug")]
    [SerializeField] bool showDebugGizmo = true;
    [SerializeField] bool printBoneNames = true;

    GameObject activeProsthetic;
    Transform shoulderJoint;
    int currentModelIndex = 0;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void OnEnable()
    {
        if (bodyController == null) return;
        bodyController.OnBodyDetected += Attach;
        bodyController.OnBodyLost += Detach;
        if (bodyController.BodyTracked) Attach();
    }

    void OnDisable()
    {
        if (bodyController == null) return;
        bodyController.OnBodyDetected -= Attach;
        bodyController.OnBodyLost -= Detach;
    }

    // ── Public ────────────────────────────────────────────────────────────────
    public void SetModelIndex(int index)
    {
        currentModelIndex = Mathf.Clamp(index, 0, 1);
        if (shoulderJoint != null) Attach();
    }

    /// <summary>
    /// Called by RuntimeOffsetCalibration sliders to update offsets live.
    /// Updates the active prosthetic immediately without re-instantiating.
    /// </summary>
    public void SetOffsets(Vector3 posOffset, Vector3 rotOffset, float scale)
    {
        positionOffset = posOffset;
        rotationOffset = rotOffset;
        scaleMultiplier = scale;

        // Apply immediately to the active prosthetic if it exists
        if (activeProsthetic != null)
        {
            activeProsthetic.transform.localPosition = positionOffset;
            activeProsthetic.transform.localRotation = Quaternion.Euler(rotationOffset);
            activeProsthetic.transform.localScale = Vector3.one * scaleMultiplier;
        }
    }

    // ── Core ──────────────────────────────────────────────────────────────────
    void Attach()
    {
        // Attach to LEFT shoulder — the amputated/hidden side
        shoulderJoint = bodyController.GetJointTransform(ARBodyJoint.LeftShoulder1);

        if (shoulderJoint == null)
        {
            Debug.LogWarning("[AboveElbow] Left shoulder joint not found yet.");
            return;
        }

        if (activeProsthetic != null) Destroy(activeProsthetic);

        var prefab = currentModelIndex == 0 ? prostheticPrefab0 : prostheticPrefab1;
        if (prefab == null) { Debug.LogError("[AboveElbow] No prefab assigned."); return; }

        // Instantiate and parent to shoulder joint
        activeProsthetic = Instantiate(prefab);
        activeProsthetic.transform.SetParent(shoulderJoint, worldPositionStays: false);
        activeProsthetic.transform.localPosition = positionOffset;
        activeProsthetic.transform.localRotation = Quaternion.Euler(rotationOffset);
        activeProsthetic.transform.localScale = Vector3.one * scaleMultiplier;

        // Print bone names once so you can verify them in Console
        if (printBoneNames)
        {
            Debug.Log("[AboveElbow] Bone hierarchy:");
            PrintBones(activeProsthetic.transform, 0);
        }

        // Give bones to MirrorArmController
        mirrorArmController?.SetProstheticRoot(activeProsthetic.transform);

        // Give Animator to IMUGripController
        var anim = activeProsthetic.GetComponentInChildren<Animator>();
        if (anim != null)
            imuGripController?.SetAnimator(anim);
        else
            Debug.LogWarning("[AboveElbow] No Animator on prefab — add one for grip animations.");

        // Refresh ARKit source joints
        mirrorArmController?.RefreshJoints();
    }

    void Detach()
    {
        if (activeProsthetic != null) { Destroy(activeProsthetic); activeProsthetic = null; }
        shoulderJoint = null;
    }

    void PrintBones(Transform t, int depth)
    {
        Debug.Log($"{new string(' ', depth * 2)}└─ {t.name}");
        foreach (Transform child in t) PrintBones(child, depth + 1);
    }

    void OnDrawGizmos()
    {
        if (!showDebugGizmo || shoulderJoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(shoulderJoint.position, 0.06f);
    }
}