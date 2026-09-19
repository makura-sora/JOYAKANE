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

        // 強い打撃ほど、大きく揺らす
        ShakeStrength = MaxShakeDistance * ratio;
        ShakeTimer = ShakeTime;
    }

    private void LateUpdate()
    {
        // 編集に戻ったら、揺れを終える
        if (!PlayArm.IsPlaying)
        {
            ShakeTimer = 0f;
        }

        if (ShakeTimer <= 0f)
        {
            GameCamera.transform.localPosition =
                StartCameraPosition;

            return;
        }

        ShakeTimer = Mathf.Max(
            0f, ShakeTimer - Time.deltaTime);

        // 時間が経つほど、揺れを小さくする
        float remainingRatio =
            ShakeTimer / Mathf.Max(0.01f, ShakeTime);

        // ランダムな方向へ、少しだけカメラをずらす
        Vector2 offset =
            Random.insideUnitCircle *
            ShakeStrength *
            remainingRatio;

        // 元の位置を基準にするので、カメラが流れていかない
        GameCamera.transform.localPosition =
            StartCameraPosition +
            new Vector3(offset.x, offset.y, 0f);
    }

    private void OnDisable()
    {
        // 揺れの残り時間を消す
        ShakeTimer = 0f;

        // 再生終了時など、カメラが先に破棄されていたら何もしない
        if (GameCamera == null) return;

        // カメラが残っている場合だけ、元の位置へ戻す
        GameCamera.transform.localPosition = StartCameraPosition;
    }
}