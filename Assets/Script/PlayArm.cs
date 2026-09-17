using UnityEngine;

public class PlayArm : MonoBehaviour
{
    [Header("アームの設定")]
    [SerializeField] private ArticulationBody CraneRoot;
    [SerializeField] private ArticulationBody Arm1;
    [SerializeField] private ArticulationBody Arm2;

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

        // 時間を動かす直前の状態を記録する
        PrintArmState("再生開始：CraneRoot", CraneRoot);
        PrintArmState("再生開始：Arm1", Arm1);
        PrintArmState("再生開始：Arm2", Arm2);

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

        // 編集中は物理を無効にしたままにする
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


    // 再生開始時の物理状態をConsoleへ出す
    private void PrintArmState(string label, ArticulationBody body)
    {
        Debug.Log(
            $"{label}\n" +
            $"位置：{body.transform.position.ToString("F4")}\n" +
            $"角度：{body.transform.eulerAngles.ToString("F4")}\n" +
            $"質量：{body.mass}\n" +
            $"重心：{body.centerOfMass.ToString("F4")}\n" +
            $"慣性：{body.inertiaTensor.ToString("F4")}\n" +
            $"慣性の向き：{body.inertiaTensorRotation.eulerAngles.ToString("F4")}\n" +
            $"移動減衰：{body.linearDamping}\n" +
            $"回転減衰：{body.angularDamping}\n" +
            $"関節摩擦：{body.jointFriction}\n" +
            $"親側の接続位置：{body.parentAnchorPosition.ToString("F4")}\n" +
            $"親側の接続角度：{body.parentAnchorRotation.eulerAngles.ToString("F4")}\n" +
            $"Driveの減衰：{body.xDrive.damping}",
            body
        );
    }

    private void FixedUpdate()
    {
        if (!IsPlaying) return;

        PhysicsStep++;

        // 最初の更新と、その後100更新ごとに記録する
        // 通常設定なら約2秒間隔。最初の約20秒間だけ記録する
        if (PhysicsStep > 1001) return;
        if ((PhysicsStep - 1) % 100 != 0) return;

        Debug.Log(
            $"物理更新：{PhysicsStep}回目\n" +
            $"Arm1 関節角度：{Arm1.jointPosition[0]:F4}" +
            $" ／ 回転速度：{Arm1.jointVelocity[0]:F4}\n" +
            $"Arm2 関節角度：{Arm2.jointPosition[0]:F4}" +
            $" ／ 回転速度：{Arm2.jointVelocity[0]:F4}\n" +
            $"休止状態：{CraneRoot.IsSleeping()}"
        );
    }
}