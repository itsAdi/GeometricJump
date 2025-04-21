using UnityEngine;
using UnityEngine.UI;

public class handleUI : MonoBehaviour, inputBase
{
    public Animator initTextAnim, MidUIAnim;
    public Text scoreText, highScoreText;
    public Image AudioImage;
    public Sprite[] AudioSprites;
    public AudioSource[] AudioSources;
    public RectTransform CanvasRect;

    private float[] _originalAudioVolumes;

    public void ToggleAudio()
    {
        persistentData.Instance.ToggleAudioVolume();
        AudioImage.sprite = persistentData.Instance.GameData.AudioMuted ? AudioSprites[1] : AudioSprites[0];
        if (AudioSources != null && AudioSources.Length > 0)
        {
            for(int i = 0; i < AudioSources.Length; i++)
            {
                AudioSources[i].volume = persistentData.Instance.GameData.AudioMuted ? 0f : _originalAudioVolumes[i];
            }
        }
    }

    public void OnTouch()
    {
        if (!persistentData.Instance.gameStarted)
        {
            initTextAnim.SetTrigger("popOut");
            MidUIAnim.SetTrigger("GoOut");
            Vector2 scorePos = new Vector2(0f, 15f);
            scorePos *= Vector2.down;
            Vector2 scale = new Vector2(CanvasRect.rect.width / Screen.width, CanvasRect.rect.height / Screen.height);
            scoreText.rectTransform.anchoredPosition = Vector2.Scale(scorePos, scale);
        }
    }

    public void OnAccelerometre(float xVel) { }

    void Start()
    {
        persistentData.Instance.ibColl.Add(this);
        scoreText.text = "0";
        highScoreText.text = $"Last Highest: {persistentData.Instance.GameData.HighestScore}";
        AudioImage.sprite = persistentData.Instance.GameData.AudioMuted ? AudioSprites[1] : AudioSprites[0];
        if (AudioSources != null && AudioSources.Length > 0)
        {
            _originalAudioVolumes = new float[AudioSources.Length];
            for(int i = 0; i < AudioSources.Length; i++)
            {
                _originalAudioVolumes[i] = AudioSources[i].volume;
                AudioSources[i].volume = persistentData.Instance.GameData.AudioMuted ? 0f : _originalAudioVolumes[i];
            }
        }
    }

    void Update()
    {
        scoreText.text = persistentData.Instance.score.ToString();
    }
}
