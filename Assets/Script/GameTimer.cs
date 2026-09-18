using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Inspectorで指定")]
    [SerializeField] private PlayArm PlayArm;
    [SerializeField] private BonnoCounter BonnoCounter;
    [SerializeField] private TMP_Text TimeText;

    [Header("制限時間")]
    [SerializeField] private float TimeLimit = 30f;

    private float RemainingTime;
    private bool IsTimeUp;

    // 得点を変更してよい状態か
    // 再生中で、まだ時間切れになっていなければtrue
    public bool CanScore
    {
        get
        {
            return PlayArm.IsPlaying && !IsTimeUp;
        }
    }

    private void Awake()
    {
        ResetTimer();
    }

    private void Update()
    {
        // 編集中・時間切れ後はカウントしない
        if (!CanScore) return;

        RemainingTime -= Time.deltaTime;

        if (RemainingTime <= 0f)
        {
            RemainingTime = 0f;

            // ここから先は、鐘に当たっても得点が変わらない
            IsTimeUp = true;

            // タイマーを隠して、結果を表示する
            TimeText.enabled = false;
            BonnoCounter.ShowResult();

            // timeScaleは変更しない。
            // 結果表示中もアームと鐘は動き続ける
        }

        UpdateDisplay();
    }

    // ご利益タイムに入ったとき、残り時間を増やす
    public void AddTime(float seconds)
    {
        // 終了後に時間が復活しないようにする
        if (!CanScore) return;

        RemainingTime += seconds;

        UpdateDisplay();
    }

    // 編集に戻ったとき、PlayArmから呼ばれる
    public void ResetTimer()
    {
        RemainingTime = TimeLimit;
        IsTimeUp = false;

        // 結果表示中に隠したタイマーを再表示する
        TimeText.enabled = true;

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        // 0.5秒残っていれば「1秒」と表示する
        int seconds = Mathf.CeilToInt(RemainingTime);

        TimeText.text = $"年明けまであと {seconds} 秒";
    }
}