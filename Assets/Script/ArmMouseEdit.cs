using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class ArmMouseEdit : MonoBehaviour
{
    [Header("参照をInspectorで指定")]
    [SerializeField] private PlayArm PlayArm;
    [SerializeField] private Camera GameCamera;

    [SerializeField] private ArticulationBody CraneRoot;
    [SerializeField] private ArticulationBody Arm1;
    [SerializeField] private ArticulationBody Arm2;

    [SerializeField] private Transform Arm1Visual;
    [SerializeField] private Transform Arm2Visual;
    [SerializeField] private Transform Hammer;

    [Header("操作設定")]
    [SerializeField] private float GrabRadius = 35f; // つかめる距離：画面のピクセル

    [Header("クレーンの回転中心")]
    [SerializeField] private Transform CranePivot;

    [Header("クレーンの回転範囲")]
    [SerializeField] private float MinCraneAngle = -45f;
    [SerializeField] private float MaxCraneAngle = 45f;

    // 起動時の角度を0度として、現在どれだけ回したか
    private float CurrentCraneAngle = 0f;

    // 何をつかんでいるか
    private enum DragTarget { None, Elbow, Hammer }
    private DragTarget Target;

    // クリックした瞬間にパーツが飛ばないよう、マウスとのずれを保存する
    private Vector3 GrabOffset;

    // Arm2の白い棒が、ローカルXのどちら側へ伸びているか
    private float TipSign;

    private void Start()
    {
        // 木槌を、伸縮する白い棒の子から外す
        // true：現在の見た目の位置・角度・大きさを保って親を変更する
        Hammer.SetParent(Arm2.transform, true);

        FixParentAnchor(Arm1);
        FixParentAnchor(Arm2);

        Vector3 toCenter =
            Arm2Visual.position - GetPivot(Arm2);

        TipSign = Vector3.Dot(Arm2Visual.right, toCenter) >= 0f ? 1f : -1f;
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;

        // 編集は、物理を止めているときだけ
        if (PlayArm.IsPlaying || mouse == null)
        {
            Target = DragTarget.None;
            return;
        }

        if (!mouse.leftButton.isPressed)
        {
            Target = DragTarget.None;
            return;
        }

        Vector2 screenPosition = mouse.position.ReadValue();

        // マウス位置を、アームと同じ平面上の座標に変換する
        Plane plane = new Plane(Vector3.forward, GetPivot(Arm1));
        Ray ray = GameCamera.ScreenPointToRay(screenPosition);

        if (!plane.Raycast(ray, out float distance)) return;

        Vector3 mousePosition = ray.GetPoint(distance);

        if (mouse.leftButton.wasPressedThisFrame)
        {
            Target = DragTarget.None;

            // 再生ボタンなど、UIを押したときはつかまない
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
                return;

            // 一番近い操作点を選ぶ
            float nearest = GrabRadius;
            PickTarget(GetPivot(Arm2), DragTarget.Elbow,
                screenPosition, ref nearest);
            PickTarget(Hammer.position, DragTarget.Hammer,
                screenPosition, ref nearest);

            // 何もつかめなかった場合は終了
            if (Target == DragTarget.None) return;

            // 選んだ操作点と、マウスとのずれを保存する
            Vector3 point = Target == DragTarget.Elbow
                ? GetPivot(Arm2)
                : GetTip();

            GrabOffset = point - mousePosition;
        }

        Vector3 destination = mousePosition + GrabOffset;

        switch (Target)
        {
            case DragTarget.Elbow:
                ChangeArm(
                    Arm1, Arm1Visual, GetPivot(Arm2),
                    destination, true);
                break;

            case DragTarget.Hammer:
                ChangeArm(
                    Arm2, Arm2Visual, GetTip(),
                    destination, false);
                break;
        }
    }

    // 棒の長さと角度を、マウス位置に合わせる
    private void ChangeArm(
    ArticulationBody body,
    Transform visual,
    Vector3 oldEnd,
    Vector3 destination,
    bool firstArm)
    {
        Vector3 pivot = GetPivot(body);

        // 現在の支点→先端と、支点→マウスの方向
        Vector3 oldDirection = oldEnd - pivot;
        Vector3 newDirection = destination - pivot;

        float oldLength = oldDirection.magnitude;
        float newLength = newDirection.magnitude;

        // ゼロで割ることと、方向が決まらない状態だけ避ける
        if (oldLength < 0.0001f || newLength < 0.0001f)
            return;

        // 長さを制限せず、マウスまでの距離をそのまま使う
        float ratio = newLength / oldLength;

        // 伸縮によって先端が移動する量
        Vector3 movement = oldDirection * (ratio - 1f);

        // 白い棒の位置と長さを変更する
        visual.position =
            pivot + (visual.position - pivot) * ratio;

        Vector3 scale = visual.localScale;
        scale.x *= ratio;
        visual.localScale = scale;

        if (firstArm)
        {
            // Arm1を伸ばしたら、Arm2全体を先端へ移動する
            Arm2.transform.position += movement;

            // 関節の接続位置も合わせる
            Arm2.parentAnchorPosition =
                body.transform.InverseTransformPoint(oldEnd + movement);
        }
        else
        {
            // Arm2を伸ばしたら、木槌の位置だけ移動する
            // 木槌のScaleには触らない
            Hammer.position += movement;
        }

        // マウスの方向へ、支点を中心にアームを回す
        float angle = Vector3.SignedAngle(
            oldDirection, newDirection, Vector3.forward);

        body.transform.RotateAround(
            pivot, Vector3.forward, angle);
    }

    private Vector3 GetPivot(ArticulationBody body)
    {
        return body.transform.TransformPoint(body.anchorPosition);
    }

    private Vector3 GetTip()
    {
        // プロジェクト内のSquareは、横幅1の画像をScaleで伸ばしている
        return Arm2Visual.TransformPoint(
            new Vector3(TipSign * 0.5f, 0f, 0f));
    }

    private void PickTarget(
        Vector3 worldPosition, DragTarget candidate,
        Vector2 mousePosition, ref float nearest)
    {
        Vector3 screen = GameCamera.WorldToScreenPoint(worldPosition);
        if (screen.z <= 0f) return;

        float distance = Vector2.Distance(mousePosition, screen);

        if (distance < nearest)
        {
            nearest = distance;
            Target = candidate;
        }
    }

    private void FixParentAnchor(ArticulationBody body)
    {
        Vector3 position = body.parentAnchorPosition;
        Quaternion rotation = body.parentAnchorRotation;

        body.matchAnchors = false;
        body.parentAnchorPosition = position;
        body.parentAnchorRotation = rotation;
    }

    // 矢印ボタンから呼ぶ
    // angleは「今回、何度回転させるか」
    public void RotateCrane(float angle)
    {
        if (PlayArm.IsPlaying) return;

        // 回転後の角度を、指定した範囲内に収める
        float nextAngle = Mathf.Clamp(
            CurrentCraneAngle + angle,
            MinCraneAngle,
            MaxCraneAngle);

        // 実際に回してよい角度だけ求める
        // 限界に達していれば0になる
        float moveAngle = nextAngle - CurrentCraneAngle;

        CraneRoot.transform.RotateAround(
            CranePivot.position,
            Vector3.forward,
            moveAngle);

        // 現在の角度を更新する
        CurrentCraneAngle = nextAngle;
    }
}


