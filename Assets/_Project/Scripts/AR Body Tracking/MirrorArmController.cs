using UnityEngine;

/// <summary>
/// Mirrors RIGHT arm ARKit joints onto the prosthetic bones every frame.
/// Uses ARBodyJoint integer constants — no ARHumanBodyJointIndex needed.
///
/// Bone names match this specific model's Blender armature:
///   Bone.001 = shoulder
///   Bone.002 = upper arm
///   Bone.004 = forearm  (Bone.003 is passive elbow joint)
///   Bone.005 = wrist
/// </summary>
public class MirrorArmController : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [SerializeField] HumanBodyJointController bodyController;

    [Header("Smoothing — start at 12, tune on device")]
    [SerializeField] float smoothSpeed = 12f;

    [Header("Rotation offsets — tune if model looks twisted on device")]
    [SerializeField] Vector3 shoulderOffset = Vector3.zero;
    [SerializeField] Vector3 upperArmOffset = Vector3.zero;
    [SerializeField] Vector3 forearmOffset  = Vector3.zero;
    [SerializeField] Vector3 wristOffset    = Vector3.zero;

    // Right arm source joints from ARKit
    Transform srcShoulder, srcUpperArm, srcForearm, srcWrist;

    // Prosthetic target bones
    Transform boneShoulder, boneUpperArm, boneForearm, boneWrist;

    bool bonesReady  = false;
    bool jointsReady = false;

    // ── Lifecycle ─────────────────────────────────────────────────────────────
    void OnEnable()
    {
        if (bodyController == null) return;
        bodyController.OnBodyDetected += FindSourceJoints;
        if (bodyController.BodyTracked) FindSourceJoints();
    }

    void OnDisable()
    {
        if (bodyController != null)
            bodyController.OnBodyDetected -= FindSourceJoints;
    }

    // ── Called by AboveElbowProstheticAttacher after prefab is spawned ────────
    public void SetProstheticRoot(Transform root)
    {
        boneShoulder = DeepFind(root, "Bone.001");
        boneUpperArm = DeepFind(root, "Bone.002");
        boneForearm  = DeepFind(root, "Bone.004");
        boneWrist    = DeepFind(root, "Bone.005");

        bonesReady = boneShoulder != null && boneUpperArm != null
                  && boneForearm  != null && boneWrist    != null;

        if (bonesReady)
            Debug.Log("[MirrorArm] All 4 bones found successfully.");
        else
            Debug.LogError("[MirrorArm] One or more bones missing — check names match Blender exactly.");
    }

    public void RefreshJoints() => FindSourceJoints();

    void FindSourceJoints()
    {
        // Read RIGHT arm — the reference/mirror source
        srcShoulder = bodyController.GetJointTransform(ARBodyJoint.RightShoulder1);
        srcUpperArm = bodyController.GetJointTransform(ARBodyJoint.RightArm);
        srcForearm  = bodyController.GetJointTransform(ARBodyJoint.RightForearm);
        srcWrist    = bodyController.GetJointTransform(ARBodyJoint.RightHand);

        jointsReady = srcShoulder != null && srcUpperArm != null
                   && srcForearm  != null && srcWrist    != null;

        if (!jointsReady)
            Debug.LogWarning("[MirrorArm] Right arm joints not all found yet.");
    }

    // ── Per-frame mirroring ───────────────────────────────────────────────────
    void Update()
    {
        if (!bonesReady || !jointsReady) return;

        Mirror(srcShoulder, boneShoulder, shoulderOffset);
        Mirror(srcUpperArm, boneUpperArm, upperArmOffset);
        Mirror(srcForearm,  boneForearm,  forearmOffset);
        Mirror(srcWrist,    boneWrist,    wristOffset);
    }

    void Mirror(Transform src, Transform dst, Vector3 offset)
    {
        if (src == null || dst == null) return;

        Vector3 e = src.localRotation.eulerAngles;

        // Flip Y and Z to mirror across the body's sagittal plane
        Vector3 mirrored = new Vector3(
             e.x + offset.x,
            -e.y + offset.y,
            -e.z + offset.z
        );

        dst.localRotation = Quaternion.Lerp(
            dst.localRotation,
            Quaternion.Euler(mirrored),
            Time.deltaTime * smoothSpeed
        );
    }

    // ── Utility: search all children for bone by name ─────────────────────────
    Transform DeepFind(Transform parent, string boneName)
    {
        if (parent.name == boneName) return parent;
        foreach (Transform child in parent)
        {
            var hit = DeepFind(child, boneName);
            if (hit != null) return hit;
        }
        Debug.LogWarning($"[MirrorArm] Bone '{boneName}' not found under {parent.name}");
        return null;
    }
}
