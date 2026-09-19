using UnityEngine;

public class BellEffect : MonoBehaviour
{
    [Header("Inspectorで指定")]
    [SerializeField] private PlayArm PlayArm;
    [SerializeField] private GameTimer GameTimer;
    [SerializeField] private Camera GameCamera;
    [SerializeField] private RectTransform TextCanvas;
    [SerializeField] private HitText HitTextPrefab;

    [Header("画面揺れ")]
    [SerializeField] private float ShakeTime = 0.2f;

    // カメラをずらす最大距離。ワールドの大きさに合わせて調整する
    [SerializeField] private float MaxShakeDistance = 0.15f;

    // この威力以上で、揺れが最大になる
    [SerializeField] private float MaxShakePower = 30f;

    [Header("ご利益タイム開始時の揺れ")]
    [SerializeField] private float BonusShakeTime = 1.2f;
    [SerializeField] private float BonusShakeDistance = 0.6f;

    // 今回の揺れの全体時間
    private float CurrentShakeTime;

    // ご利益タイム用の強い揺れを再生中か
    private bool IsBonusShake;

    private Vector3 StartCameraPosition;

    private float ShakeTimer;
    private float ShakeStrength;

    private void Awake()
    {
        // 揺れが終わったときに戻す位置
        StartCameraPosition = GameCamera.transform.localPosition;
    }

    // 鐘に当たったとき、Bellから呼ぶ
    public void Show(int power, bool isBenefit, Vector3 hitPosition)
    {
        // 編集中・結果表示中は、新しい打撃演出を出さない
        if (!GameTimer.CanScore) return;

        // 打撃位置を、画面上の座標へ変換する
        Vector3 screenPosition =
            GameCamera.WorldToScreenPoint(hitPosition);

        // 文字をCanvas内に生成する
        HitText hitText = Instantiate(HitTextPrefab, TextCanvas);

        hitText.Show(
            power,
            isBenefit,
            screenPosition,
            PlayArm);

        // 威力を0～1の割合にする
        float ratio = Mathf.Clamp01(
            power / Mathf.Max(1f, MaxShakePower));

        // ご利益タイム用の揺れを、普通の打撃で上書きしない
        if (!IsBonusShake)
        {
            ShakeStrength = MaxShakeDistance * ratio;
            CurrentShakeTime = Mathf.Max(0.01f, ShakeTime);
            ShakeTimer = CurrentShakeTime;
        }
    }

    private void LateUpdate()
    {
        if (GameCamera == null) return;

        // 編集に戻った場合は、揺れだけ終了する。
        // 編集中なので、時間は止めたままにする
        if (!PlayArm.IsPlaying)
        {
            ShakeTimer = 0f;
            IsBonusShake = false;

            GameCamera.transform.localPosition = StartCameraPosition;
            return;
        }

        if (ShakeTimer <= 0f)
        {
            GameCamera.transform.localPosition = StartCameraPosition;
            return;
        }

        // ご利益タイムの揺れは、時間停止の影響を受けない時間で進める
        // 通常の打撃は、今までどおりゲーム内の時間を使う
        float deltaTime = IsBonusShake
            ? Time.unscaledDeltaTime
            : Time.deltaTime;

        ShakeTimer = Mathf.Max(0f, ShakeTimer - deltaTime);

        // 終わりに近づくほど揺れを弱める
        float remainingRatio =
            ShakeTimer / Mathf.Max(0.01f, CurrentShakeTime);

        Vector2 offset =
            Random.insideUnitCircle *
            ShakeStrength *
            remainingRatio;

        GameCamera.transform.localPosition =
            StartCameraPosition +
            new Vector3(offset.x, offset.y, 0f);

        if (ShakeTimer <= 0f)
        {
            // カメラを元の位置へ戻す
            GameCamera.transform.localPosition = StartCameraPosition;

            if (IsBonusShake)
            {
                IsBonusShake = false;

                // ご利益タイムの揺れが終わったので、ゲームを再開する
                Time.timeScale = 1f;
            }
        }
    }
    private void OnDisable()
    {
        // ご利益タイムの演出中に無効になった場合は、時間を戻す。
        // ただし、編集に戻っているなら停止したままにする
        if (IsBonusShake && PlayArm != null && PlayArm.IsPlaying)
        {
            Time.timeScale = 1f;
        }

        // 揺れの残り時間を消す
        ShakeTimer = 0f;
        IsBonusShake = false;

        // 再生終了時など、カメラが先に破棄されていたら何もしない
        if (GameCamera == null) return;

        // カメラが残っている場合だけ、元の位置へ戻す
        GameCamera.transform.localPosition = StartCameraPosition;
    }

    // ご利益タイムへ入った瞬間に呼ぶ
    public void PlayBonusShake()
    {
        IsBonusShake = true;

        ShakeStrength = BonusShakeDistance;
        CurrentShakeTime = Mathf.Max(0.01f, BonusShakeTime);
        ShakeTimer = CurrentShakeTime;

        // アームの物理計算と、制限時間のカウントを止める
        Time.timeScale = 0f;
    }
}