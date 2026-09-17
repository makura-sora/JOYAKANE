using UnityEngine;

public class PlayArm : MonoBehaviour
{
    //Ä¶’†‚©‚Ç‚¤‚©
    public bool IsPlaying {  get; private set; }

    private void Awake()
    {
        IsPlaying = false;

        //ŠÔ‚ğ~‚ß‚é
        Time.timeScale = 0f;
    }

    public void Play()
    {
        //Ä¶’†‚È‚ç‰½‚à‚µ‚È‚¢
        if (IsPlaying) return;

        IsPlaying = true;

        //ŠÔ‚ğ’Êí‚É–ß‚·
        Time.timeScale = 1f;
    }
}
