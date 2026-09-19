using UnityEngine;
using TMPro;

public class HitText : MonoBehaviour
{
    [Header("このオブジェクトのTextを指定")]
    [SerializeField] private TMP_Text Label;

    [Header("表示の設定")]
    [SerializeField] private float DisplayTime = 0.8f;
    [SerializeField] private float RiseSpeed = 50f;

    private PlayArm PlayArm;
    private float Timer;

    // 文字を生成した直後に呼ぶ
    public void Show(
        int power,
        bool isBenefit,
        Vector3 screenPosition,
        PlayArm playArm)
    {
        PlayArm = playArm;
        Timer = 0f;

        // カメラからの距離はUIの配置には使わない
        screenPosition.z = 0f;
        transform.position = screenPosition;

        // 単位だけ少し小さく表示する
        string unit = isBenefit ? "ご利益" : "煩悩";
        Label.text = $"{power}<size=60%>{unit}</size>";

        // 煩悩は白、ご利益は緑
        Label.color = isBenefit ? Color.green : Color.white;

        // 威力30以上で最大サイズになる
        float ratio = Mathf.Clamp01(power / 30f);

        // 弱い打撃は48、強い打撃は160まで大きくする
        Label.fontSize = Mathf.Lerp(48f, 160f, ratio);
    }

    private void Update()
    {
        // 編集に戻ったら、残っている文字も消す
        if (!PlayArm.IsPlaying)
        {
            Destroy(gameObject);
            return;
        }

        Timer += Time.deltaTime;

        // 少しずつ上へ移動する
        Label.rectTransform.anchoredPosition +=
            Vector2.up * RiseSpeed * Time.deltaTime;

        // 表示時間の後半で、徐々に透明にする
        float progress = Timer / Mathf.Max(0.01f, DisplayTime);

        Color color = Label.color;
        color.a = 1f - Mathf.InverseLerp(0.5f, 1f, progress);
        Label.color = color;

        if (Timer >= DisplayTime)
        {
            Destroy(gameObject);
        }
    }
}