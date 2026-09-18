using UnityEngine;
using UnityEngine.EventSystems;

public class CraneTurnButton : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [SerializeField] private ArmMouseEdit ArmMouseEdit;

    [Header("回転方向：1か-1を指定")]
    [SerializeField] private float Direction = 1f;

    [Header("1秒間に回転する角度")]
    [SerializeField] private float RotateSpeed = 45f;

    private bool IsPressed;

    private void Update()
    {
        if (!IsPressed) return;

        // 編集中はtimeScaleが0なので、
        // 時間停止の影響を受けない時間を使う
        float angle =
            Direction * RotateSpeed * Time.unscaledDeltaTime;

        ArmMouseEdit.RotateCrane(angle);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        IsPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsPressed = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // ボタンの外へマウスを動かしたら止める
        IsPressed = false;
    }

    private void OnDisable()
    {
        IsPressed = false;
    }
}