#if UNITY_EDITOR
#define ON_PC
#endif
#if !UNITY_EDITOR && UNITY_ANDROID
#define ON_PHONE
#endif
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using KemothStudios.EventSystem;
using KemothStudios.KemothAds;
using UnityEngine.InputSystem;
using KemothStudios.Utilities;

public class persistentData : MonoBehaviour
{
    public Sprite[] platSprites;
    [HideInInspector] public int score;
    [HideInInspector] public bool gameOver, gameStarted, canIncreaseRawScore;
    [HideInInspector] public float playerHalfWidth, viewportLeft, viewportRight, playerTopMargin, playerBottomMargin, deathMargin;
    [HideInInspector] public handlePlayer hPlayer = null;
    [HideInInspector] public powerJumpManager pjManager = null;
    [HideInInspector] public handlePlatforms hPlat = null;
    public List<inputBase> ibColl;

    public GameData GameData { get; private set; }

    public static persistentData Instance;

    private float rawScore;
    private bool _isTouched;

    EventBinding<InterstitialAdCompletedEvent> _interstitialAdCompleted;
    private Action _onInterstialAdCompleted;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
#if ON_PHONE
            InputSystem.EnableDevice(Accelerometer.current);
#endif
        }
        else if (Instance != this)
        {
            Destroy(this.gameObject);
        }
    }

#if ON_PHONE
    private void OnDisable()
    {
        if (Instance == this){
            InputSystem.DisableDevice(Accelerometer.current);
        }
    }
#endif

    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        ibColl = new List<inputBase>();
        playerHalfWidth = GameObject.Find("Player_Default_2").GetComponent<SpriteRenderer>().size.x / 2f;
        Camera tempCam = Camera.main;
        viewportRight = tempCam.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0f, 0f)).x;
        viewportLeft = tempCam.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)).x;
        playerTopMargin = tempCam.ViewportToWorldPoint(new Vector2(0f, 0.6f)).y;
        playerBottomMargin = tempCam.ViewportToWorldPoint(new Vector2(0f, 0.7f)).y;
        deathMargin = tempCam.ScreenToWorldPoint(new Vector2(0f, -playerHalfWidth)).y;
        try
        {
            if (File.Exists(Application.persistentDataPath + "/GameData.json"))
            {
                string jsonData = File.ReadAllText(Application.persistentDataPath + "/GameData.json");
                GameData = JsonUtility.FromJson<GameData>(jsonData);
            }
            else
            {
                GameData = new GameData();
                string jsonData = JsonUtility.ToJson(GameData);
                File.WriteAllText(Application.persistentDataPath + "/GameData.json", jsonData);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error in Reading/Writing game data : " + e.Message);
        }

        _interstitialAdCompleted = new EventBinding<InterstitialAdCompletedEvent>(OnInterstialAdCompleted);
        EventBus<InterstitialAdCompletedEvent>.RegisterBinding(_interstitialAdCompleted);
    }

    private void OnInterstialAdCompleted()
    {
        _onInterstialAdCompleted?.Invoke();
        _onInterstialAdCompleted = null;
    }

    void Update()
    {
        if (ibColl == null)
        {
            return;
        }

        if (!gameOver)
        {
            float speed = 9.5f;
            
            if (!gameStarted)
            {
                if (_isTouched)
                {
                    _isTouched = false;
                    ibColl.ForEach(x => x.OnTouch());
                    gameStarted = true;
                }
            }
#if ON_PC
            if (Keyboard.current.aKey.isPressed)
            {
                ibColl.ForEach(x => x.OnAccelerometre(-speed * Time.deltaTime));
            }

            if (Keyboard.current.dKey.isPressed)
            {
                ibColl.ForEach(x => x.OnAccelerometre(speed * Time.deltaTime));
            }

            if (Keyboard.current.aKey.wasReleasedThisFrame)
            {
                ibColl.ForEach(x => x.OnAccelerometre(0f));
            }

            if (Keyboard.current.dKey.wasReleasedThisFrame)
            {
                ibColl.ForEach(x => x.OnAccelerometre(0f));
            }
#endif
#if ON_PHONE
            if (Accelerometer.current != null)
            {
                Vector3 acceleration = Accelerometer.current.acceleration.ReadValue();
                if (!Mathf.Approximately(0f, acceleration.x))
                {
                    float absAccX = Mathf.Abs(acceleration.x);
                    float adjustedAccX = Mathf.InverseLerp(0f, 0.5f, absAccX);
                    speed = Mathf.Lerp(speed / 12f, speed, adjustedAccX);
                    ibColl.ForEach(x => x.OnAccelerometre(acceleration.x > 0f ? speed * Time.deltaTime : -speed * Time.deltaTime));
                }
            }
#endif
        }
    }

    public void OnTouchInput()
    {
        _isTouched = true;
    }

    public void increaseRawScore()
    {
        rawScore += canIncreaseRawScore ? 8f * Time.deltaTime : 0f;
        if (rawScore > (score + 1))
        {
            score++;
        }
    }

    public void ToggleAudioVolume()
    {
        GameData.AudioMuted = !GameData.AudioMuted;
        try
        {
            if (File.Exists(Application.persistentDataPath + "/GameData.json"))
            {
                string jsonData = JsonUtility.ToJson(GameData);
                File.WriteAllText(Application.persistentDataPath + "/GameData.json", jsonData);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error in Writing game data : " + e.Message);
        }
    }

    public void makeGameOver()
    {
        GameData.HighestScore = score > GameData.HighestScore ? score : GameData.HighestScore;
        try
        {
            if (File.Exists(Application.persistentDataPath + "/GameData.json"))
            {
                string jsonData = JsonUtility.ToJson(GameData);
                File.WriteAllText(Application.persistentDataPath + "/GameData.json", jsonData);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error in Writing game data : " + e.Message);
        }

        ibColl.Clear();
        gameOver = true;
        rawScore = 0f;
        score = 0;
    }

    public void showInterstitial()
    {
        _onInterstialAdCompleted = () => UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        EventBus<ShowInterstitialAdEvent>.RaiseEvent(new ShowInterstitialAdEvent());
    }
}

public class GameData
{
    public bool AudioMuted;
    public int HighestScore;
}