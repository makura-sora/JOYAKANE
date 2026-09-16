using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class BellReaction : MonoBehaviour
{
    [Header("槌の当たり判定")]
    [SerializeField] private Collider Hammer;

    [Header("鐘の反応")]
    [SerializeField] private float HitPower = 0.5f;      // 槌の速度を受け取る割合
    [SerializeField] private float ReturnStrength = 20f; // 元へ戻す力の強さ
    [SerializeField] private float TiltAmount = 20f;     // 横のずれに対する傾き

    [Header("煩悩のカウント")]
    [SerializeField] private BonnoCounter BonnoCounter;
    [SerializeField] private float ScoreMultiplier = 0.5f; // 得点の倍率

    private Rigidbody Rb;
    private ArticulationBody HammerBody;

    private Vector3 StartPosition;    // 最初の位置
    private Quaternion StartRotation; // 最初の角度

    private Vector3 Offset;   // 最初の位置からのずれ
    private Vector3 Velocity; // 鐘が動いている速度


    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();

        // 鐘はこのスクリプトで動かすため、重力や衝突の力で動かさない
        Rb.isKinematic = true;
        Rb.useGravity = false;

        // 槌との重なりを検出する。槌を物理的に押し返さない
        GetComponent<BoxCollider>().isTrigger = true;

        // 鐘が戻る位置・角度を保存する
        StartPosition = Rb.position;
        StartRotation = Rb.rotation;

        // 槌の親にあるArticulation Bodyを取得する
        HammerBody = Hammer.GetComponentInParent<ArticulationBody>();
    }


    private void OnTriggerEnter(Collider other)
    {
        // 槌以外が触れた場合は、ここで処理を終える
        if (other != Hammer) return;

        // 槌の中心位置を取得する
        Vector3 hammerPosition = Hammer.bounds.center;

        // 槌の位置での速度を取得する
        Vector3 hammerVelocity = HammerBody.GetPointVelocity(hammerPosition);

        // 奥行き方向には動かさない
        hammerVelocity.z = 0f;

        // 速度ベクトルから速さを取得
        float hitSpeed = hammerVelocity.magnitude;

        // ほぼ止まっている槌には反応しない
        if (hitSpeed < 0.1f) return;

        // ---------- 煩悩を減らす ----------

        // 速い打撃ほど多く減らす
        int power = Mathf.Max(1, Mathf.RoundToInt(hitSpeed * ScoreMultiplier));

        BonnoCounter.Reduce(power);

        // ---------- 鐘を動かす ----------

        // 槌の速度の一部を、鐘へ与える速度にする
        Vector3 addedVelocity = hammerVelocity * HitPower;

        // 今の鐘の速度に加える
        Velocity += addedVelocity;

        // 連続で当たっても、速度の大きさは8を超えないようにする
        Velocity = Vector3.ClampMagnitude(Velocity, 8f);
    }


    private void FixedUpdate()
    {
        // 今回の物理更新で進む時間
        float deltaTime = Time.fixedDeltaTime;

        // 計算に使う強さを、正の値にしておく
        float strength = Mathf.Max(0.01f, ReturnStrength);


        // ---------- 元の位置へ戻す ----------

        // ずれと反対方向へ引き戻す
        Vector3 returnAcceleration = -Offset * strength;


        // ---------- ブレーキをかける ----------

        // 戻す力に合わせて、ブレーキの強さを決める
        float brakeStrength = 2f * Mathf.Sqrt(strength);

        // 今の移動方向と反対向きにブレーキをかける
        Vector3 brakeAcceleration = -Velocity * brakeStrength;


        // ---------- 速度と位置を更新する ----------

        // 戻す力とブレーキを合計する
        Vector3 acceleration = returnAcceleration + brakeAcceleration;

        // 加速度によって、速度を変える
        Velocity += acceleration * deltaTime;

        // 速度に応じて、最初の位置からのずれを変える
        Offset += Velocity * deltaTime;

        // 最初の位置にずれを足して、現在の位置を決める
        Vector3 nextPosition = StartPosition + Offset;

        // 見た目と当たり判定を一緒に移動する
        Rb.MovePosition(nextPosition);


        // ---------- 横のずれに合わせて傾ける ----------

        // 右へずれたらプラス、左へずれたらマイナスの角度になる
        float tiltAngle = Offset.x * TiltAmount;

        // 画面に垂直なZ軸を中心とした回転を作る
        Quaternion tiltRotation = Quaternion.Euler(0f, 0f, tiltAngle);

        // 元の角度に傾きを加える
        Rb.MoveRotation(StartRotation * tiltRotation);
    }
}