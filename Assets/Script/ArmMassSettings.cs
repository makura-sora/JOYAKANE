using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HingeJoint2D))]
public class ArmMassSettings : MonoBehaviour
{
    // 0：棒の中央、1：肘の位置
    [SerializeField, Range(0f, 1f)]
    private float _elbowWeight = 1f;

    private void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        HingeJoint2D joint = GetComponent<HingeJoint2D>();

        // 今のアーム①では、Anchorが肘の位置になっている
        rb.centerOfMass =
            Vector2.Lerp(Vector2.zero, joint.anchor, _elbowWeight);
    }
}