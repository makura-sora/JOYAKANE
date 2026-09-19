using UnityEngine;
using TMPro;

public class BonusText : MonoBehaviour
{
    [Header("Inspectorで指定")]
    [SerializeField] private TMP_Text Label;
    [SerializeField] private RectTransform CanvasRect;

    [Header("演出の時間")]
    [SerializeField] private float EnterTime = 0.25f;
    [SerializeField] private float HoldTime = 1.4f;
    [SerializeField] private float ExitTime = 0.25f;

    private float Timer;
    private bool IsShowing;

    // 中央から、画面外までの移動距離
    private float MoveDistance;

    private void Awake()
    {
        ResetEffect();
    }

    // ご利益タイムに入った瞬間に呼ぶ
    public void Show()
    {
        Timer = 0f;
        IsShowing = true;

        Label.text = "スーパーご利益タイム";
        Label.enabled = true;

        // 文字全体が画面外へ隠れる距離を求める
        // Canvas直下・中央Anchor・Scale 1の配置を前提にする
        MoveDistance =
            2000f;

        // 右の画面外から始める
        SetPosition(MoveDistance);
    }

    private void Update()
    {
        if (!IsShowing) return;

        // 画面揺れで時間を止めている間は、文字を動かさない
        if (Time.timeScale == 0f) return;

        Timer += Time.deltaTime;

        // 0秒に設定しても、0で割らないようにする
        float enter = Mathf.Max(0.01f, EnterTime);
        float hold = Mathf.Max(0f, HoldTime);
        float exit = Mathf.Max(0.01f, ExitTime);

        if (Timer < enter)
        {
            // ---------- 右から中央へ ----------

            // 進み具合を0～1にする
            float progress = Timer / enter;

            // 最初は速く、中央に近づくほどゆっくりにする
            float ease = 1f - (1f - progress) * (1f - progress);

            float x = Mathf.Lerp(MoveDistance, 0f, ease);
            SetPosition(x);
        }
        else if (Timer < enter + hold)
        {
            // ---------- 中央で止まる ----------

            SetPosition(0f);
        }
        else if (Timer < enter + hold + exit)
        {
            // ---------- 中央から左へ ----------

            float progress = (Timer - enter - hold) / exit;

            // 徐々に加速して、左へ抜ける
            float ease = progress * progress;

            float x = Mathf.Lerp(0f, -MoveDistance, ease);
            SetPosition(x);
        }
        else
        {
            // 画面外へ抜けたら非表示にする
            ResetEffect();
        }
    }

    // 横位置だけを変更する
    private void SetPosition(float x)
    {
        Vector2 position = Label.rectTransform.anchoredPosition;
        position.x = x;
        Label.rectTransform.anchoredPosition = position;
    }

    // 編集へ戻ったときにも呼ぶ
    public void ResetEffect()
    {
        IsShowing = false;
        Timer = 0f;
        Label.enabled = false;

        SetPosition(0f);
    }
}