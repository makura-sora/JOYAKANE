using UnityEngine;

public class PlayArm : MonoBehaviour
{
    [Header("アームの設定")]
    [SerializeField] private ArticulationBody CraneRoot;
    [SerializeField] private ArticulationBody Arm1;
    [SerializeField] private ArticulationBody Arm2;

    [Header("停止時にリセットするもの")]
    [SerializeField] private BonnoCounter BonnoCounter;
    [SerializeField] private Bell Bell;

    [Header("制限時間")]
    [SerializeField] private GameTimer GameTimer;

    public bool IsPlaying { get; private set; }

    // 再生してから何回、物理更新が行われたか
    private int PhysicsStep;
    // 再生直前の支点の位置・角度
    private Vector3 SavedRootPosition;
    private Quaternion SavedRootRotation;

    // 再生直前のアームの位置・角度
    private Vector3 SavedArm1Position;
    private Quaternion SavedArm1Rotation;

    private Vector3 SavedArm2Position;
    private Quaternion SavedArm2Rotation;

    private void Awake()
    {
        // Inspectorでチェックを外している場合は何もしない
        if (!enabled) return;

        Time.timeScale = 0f;
        IsPlaying = false;

        // 最初から「物理が無効の編集状態」にする
        DisablePhysics();
    }

    // ボタンから呼ぶ
    public void Play()
    {
        if (IsPlaying)
        {
            ReturnToEdit();
        }
        else
        {
            PhysicsStep = 0;
            StartSimulation();
        }
    }

    private void StartSimulation()
    {
        // 物理を開始する前に、現在の配置を保存する
        SavedRootPosition = CraneRoot.transform.position;
        SavedRootRotation = CraneRoot.transform.rotation;

        SavedArm1Position = Arm1.transform.localPosition;
        SavedArm1Rotation = Arm1.transform.localRotation;

        SavedArm2Position = Arm2.transform.localPosition;
        SavedArm2Rotation = Arm2.transform.localRotation;

        // 物理が無効なうちに、編集後の配置に接続設定を合わせる
        AlignJoint(Arm1, CraneRoot);
        AlignJoint(Arm2, Arm1);

        // ここからは既存の処理
        CraneRoot.enabled = true;
        Arm1.enabled = true;
        Arm2.enabled = true;

        // 今の配置を基準の0度にしたので、物理側の角度もそろえる
        Arm1.jointPosition = new ArticulationReducedSpace(0f);
        Arm2.jointPosition = new ArticulationReducedSpace(0f);

        // 支点 → Arm1 → Arm2の順に物理を有効にする
        // 1回目も2回目も、ここから開始する
        CraneRoot.enabled = true;
        Arm1.enabled = true;
        Arm2.enabled = true;

        // 静止した状態から開始する
        CraneRoot.linearVelocity = Vector3.zero;
        CraneRoot.angularVelocity = Vector3.zero;

        Arm1.jointVelocity = new ArticulationReducedSpace(0f);
        Arm2.jointVelocity = new ArticulationReducedSpace(0f);

        CraneRoot.WakeUp();
        Arm1.WakeUp();
        Arm2.WakeUp();
        IsPlaying = true;
        Time.timeScale = 1f;
    }

    private void ReturnToEdit()
    {
        Time.timeScale = 0f;

        // 位置を戻す前に、3つとも物理を無効にする
        DisablePhysics();

        // 物理が無効なので、Transformで配置を戻せる
        CraneRoot.transform.SetPositionAndRotation(
            SavedRootPosition, SavedRootRotation);

        Arm1.transform.localPosition = SavedArm1Position;
        Arm1.transform.localRotation = SavedArm1Rotation;

        Arm2.transform.localPosition = SavedArm2Position;
        Arm2.transform.localRotation = SavedArm2Rotation;


        // 煩悩と鐘を戻す
        BonnoCounter.ResetCount();
        Bell.ResetBell();

        // 残り時間も戻す
        GameTimer.ResetTimer();

        // 編集状態に戻る
        IsPlaying = false;
    }

    private void DisablePhysics()
    {
        // 先端 → 支点の順に物理を無効にする
        // 画像の表示は消えない
        Arm2.enabled = false;
        Arm1.enabled = false;
        CraneRoot.enabled = false;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    // 編集した配置を、関節の基準となる姿勢にする
    private void AlignJoint(
        ArticulationBody child,
        ArticulationBody parent)
    {
        // 子側の接続位置・向きを、ワールド座標で取得する
        Vector3 worldPosition =
            child.transform.TransformPoint(child.anchorPosition);

        Quaternion worldRotation =
            child.transform.rotation * child.anchorRotation;

        // 親側の接続位置・向きを、子側にそろえる
        child.matchAnchors = false;

        child.parentAnchorPosition =
            parent.transform.InverseTransformPoint(worldPosition);

        child.parentAnchorRotation =
            Quaternion.Inverse(parent.transform.rotation) * worldRotation;
    }
}