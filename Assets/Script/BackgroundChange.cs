using UnityEngine;

public class BackgroundChange : MonoBehaviour
{
    [Header("”wŒi‚ÌSprite Renderer")]
    [SerializeField] private SpriteRenderer BackgroundRenderer;

    [Header("Ø‚è‘Ö‚¦‚é‰æ‘œ")]
    [SerializeField] private Sprite NormalBackground;
    [SerializeField] private Sprite TimeUpBackground;
    [SerializeField] private Sprite BenefitBackground;

    // Å‰‚Ìó‘ÔE•ÒW‚Ö–ß‚Á‚½‚Æ‚«
    public void ShowNormal()
    {
        BackgroundRenderer.sprite = NormalBackground;
    }

    // ”Ï”Y‚ªc‚Á‚½‚Ü‚ÜŠÔØ‚ê‚É‚È‚Á‚½‚Æ‚«
    public void ShowTimeUp()
    {
        BackgroundRenderer.sprite = TimeUpBackground;
    }

    // ”Ï”Y‚ğ‚·‚×‚ÄÁ‚µ‚ÄA‚²—˜‰vƒ^ƒCƒ€‚É“ü‚Á‚½‚Æ‚«
    public void ShowBenefit()
    {
        BackgroundRenderer.sprite = BenefitBackground;
    }
}