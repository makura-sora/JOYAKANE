using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BonnoCounter : MonoBehaviour
{
    [Header("煩悩の数")]
    [SerializeField] private int RemainingCount = 108;

    [Header("煩悩・ご利益の表示")]
    [SerializeField] private Image BonnoGauge;
    [SerializeField] private Image BonnoDamageGauge;
    [SerializeField] private TMP_Text BonnoText;

    [Header("減った部分を赤く表示する時間")]
    [SerializeField] private float DamageWaitTime = 0.3f;

    [Header("ご利益タイム")]
    [SerializeField] private GameTimer GameTimer;
    [SerializeField] private float BonusTime = 10f;

    [Header("クリア時の表示")]
    [SerializeField] private TMP_Text ClearTitleText;
    [SerializeField] private TMP_Text ClearScoreText;

    [Header("ゲームオーバー時の表示")]
    [SerializeField] private TMP_Text GameOverTitleText;
    [SerializeField] private TMP_Text GameOverScoreText;

    [Header("背景の切り替え")]
    [SerializeField] private BackgroundChange BackgroundChange;

    [Header("ご利益タイムの文字演出")]
    [SerializeField] private BonusText BonusText;

    [Header("ご利益タイム開始時の画面揺れ")]
    [SerializeField] private BellEffect BellEffect;

    [Header("ご利益タイムのBGM")]
    [SerializeField] private AudioSource BonusBGM;

    // 編集に戻ったときに復元する、最初の煩悩の数
    private int StartCount;

    // 煩悩をすべて消した後に獲得する得点
    private int BenefitCount;

    // 煩悩をすべて消したか
    public bool IsClear { get; private set; }

    // 赤い部分を表示しておく残り時間
    private float DamageTimer;

    // ご利益表示で変えた色を、リセット時に戻すために保存する
    private Color StartTextColor;

    private void Awake()
    {
        StartCount = RemainingCount;
        StartTextColor = BonnoText.color;

        // 数字・ゲージ・結果表示を最初の状態にする
        ResetCount();
    }

    private void Update()
    {
        // 赤い部分を少しの間だけ残す
        if (DamageTimer > 0f)
        {
            DamageTimer -= Time.deltaTime;

            // まだ表示時間が残っているなら、そのままにする
            if (DamageTimer > 0f) return;
        }

        // 赤を緑と同じ長さにして、減った部分の赤をパッと消す
        BonnoDamageGauge.fillAmount = BonnoGauge.fillAmount;
    }

    // 鐘に当たったとき、Bellから呼ばれる
    public void Reduce(int amount)
    {
        // 編集中・時間切れ後は、数字を変えない
        if (!GameTimer.CanScore) return;

        // 威力が0以下なら何もしない
        if (amount <= 0) return;

        if (IsClear)
        {
            // すでに煩悩を消していれば、ご利益を増やす
            BenefitCount += amount;
        }
        else
        {
            // まだ煩悩が残っていれば減らす
            RemainingCount -= amount;

            // 今回減った部分を、一定時間だけ赤く表示する
            DamageTimer = DamageWaitTime;

            if (RemainingCount <= 0)
            {
                // 例：残り28に威力30が当たると、残りは-2。
                // 超過した2を、ご利益として受け取る
                BenefitCount = -RemainingCount;

                // 煩悩の表示は0にする
                RemainingCount = 0;

                // 次の打撃からは、ご利益を加算する
                IsClear = true;

                // ご利益タイムに入った瞬間、BGMを最初から流す
                BonusBGM.Play();

                // 背景を切り替える
                BackgroundChange.ShowBenefit();

                // 「スーパーご利益タイム」を右から表示する
                BonusText.Show();

                // 通常の打撃より、大きく長く揺らす
                BellEffect.PlayBonusShake();

                // 残り時間を追加する
                GameTimer.AddTime(BonusTime);

                Debug.Log("スーパーご利益タイム！");
            }
        }

        // 変更した数字を画面に反映する
        UpdateDisplay();
    }

    // 現在の煩悩・ご利益を表示する
    private void UpdateDisplay()
    {
        if (IsClear)
        {
            BonnoText.text = $"ご利益 {BenefitCount}";
            BonnoText.color = Color.green;
        }
        else
        {
            BonnoText.text = $"残り煩悩 {RemainingCount}";
            BonnoText.color = StartTextColor;
        }

        // 残りの割合を、緑のゲージへ反映する
        // 例：54 ÷ 108 = 0.5なので半分になる
        if (StartCount > 0)
        {
            BonnoGauge.fillAmount =
                (float)RemainingCount / StartCount;
        }
        else
        {
            // 0で割らないようにする
            BonnoGauge.fillAmount = 0f;
        }
    }

    // 時間切れになったとき、GameTimerから呼ばれる
    public void ShowResult()
    {
        // 結果画面では、ゲージ上の文字を隠す
        BonnoText.enabled = false;

        // 成功した場合だけ、クリア用の文字を表示する
        ClearTitleText.enabled = IsClear;
        ClearScoreText.enabled = IsClear;

        // 成功していない場合だけ、ゲームオーバー用を表示する
        GameOverTitleText.enabled = !IsClear;
        GameOverScoreText.enabled = !IsClear;

        if (IsClear)
        {
            ClearTitleText.color = Color.green;
            ClearScoreText.color = Color.green;

            ClearTitleText.text = "HAPPY NEW YEAR!";
            ClearScoreText.text =
                $"獲得したご利益 {BenefitCount}";
        }
        else
        {
            // 煩悩が残っていた場合は、灰色の背景にする
            BackgroundChange.ShowTimeUp();

            GameOverTitleText.color = Color.red;
            GameOverScoreText.color = Color.red;

            GameOverTitleText.text = "HAPPY NEW YEAR";
            GameOverScoreText.text =
                $"残された煩悩 {RemainingCount}";
        }
    }

    // 編集に戻ったとき、PlayArmから呼ばれる
    public void ResetCount()
    {

        // 編集へ戻ったら、ゲージ上の文字を再表示する
        BonnoText.enabled = true;

        // 煩悩・ご利益・クリア状態を戻す
        RemainingCount = StartCount;
        BenefitCount = 0;
        IsClear = false;

        // 前回の赤い表示の待ち時間を消す
        DamageTimer = 0f;

        UpdateDisplay();

        // 赤いゲージも最初の長さに戻す
        BonnoDamageGauge.fillAmount = BonnoGauge.fillAmount;

        // 成功・失敗どちらの結果も非表示にする
        ClearTitleText.enabled = false;
        ClearScoreText.enabled = false;
        GameOverTitleText.enabled = false;
        GameOverScoreText.enabled = false;
        // 編集に戻ったら、通常背景へ戻す
        BackgroundChange.ShowNormal();
        // 演出の途中で停止しても、文字を消す
        BonusText.ResetEffect();

        // 編集へ戻ったらBGMを止める
        // 次に再生するときは曲の最初から始まる
        BonusBGM.Stop();
    }
}