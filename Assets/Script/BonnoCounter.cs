using UnityEngine;

public class BonnoCounter : MonoBehaviour
{
    //煩悩の数
    [SerializeField] private int RemainingCount = 108;

    private bool IsClear;

    //指定された数だけ煩悩を減らす
    public void Reduce(int amount)
    {
        // クリアした後は減らさない
        if (IsClear) return;

        // 残り煩悩がマイナスにならないようにする
        RemainingCount = Mathf.Max(0, RemainingCount - amount);

        Debug.Log($"威力：{amount} ／ 残り煩悩：{RemainingCount}");

        // 全て減らしたらクリア
        if (RemainingCount == 0)
        {
            IsClear = true;
            Debug.Log("あけましておめでとうございます");
        }
    }
}
