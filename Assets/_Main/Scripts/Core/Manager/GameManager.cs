using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public enum GameState
{
    Playing,
    Ending
}
public class GameManager : Singleton<GameManager>
{
    [Header("Gameplay Settings")]
    private int _score = 0;

    public int Score
    {
        set
        {
            _score = value;
            OnScoreChanged?.Invoke(_score);
        }
        get => _score;
    }

    [Header("Combo Settings")]
    // Cần 5 đấm liên tục để kích hoạt combo
    public int comboRequirement = 5;  
    public bool comboActive = false;
    public int consecutiveHits = 0;
    
    [Header("Spawn Settings")]
    [SerializeField]
    private Transform spawnPointLeft;
    [SerializeField]
    private Transform spawnPointRight;
    public int totalTime = 60;
    int timePlay = 0;
    public float speedBallBase = 30f;
    /*[SerializeField] 
    private float intervalSpawn;*/
    [SerializeField] private float minSpawnDelay = 0.5f;
    [SerializeField] private float maxSpawnDelay = 1.5f;
    
    private float _baseMinSpawnDelay;
    private float _baseMaxSpawnDelay;
    private int maxMissedHit = 10;

    private float _currentBallSpeed = 10f;
    
    
    public Action OnGameStarting;
    public Action OnGameEnding;
    public Action OnMissedHit;
    public Action<int> OnTimeChanged;
    public Action<int> OnScoreChanged;
    private int _currentLevel=1;


    private List<SpawnerType> BallSpawnerTypeList = new List<SpawnerType>()
    {
        SpawnerType.RedBall,
        SpawnerType.YellowBall,
        SpawnerType.WhiteBall,
        SpawnerType.GreyBall,
        SpawnerType.OrangeBall
    };
    public int CurrentLevel
    {
        get => _currentLevel;
        set => _currentLevel = Mathf.Clamp(value, 1, _currentLevel);
    }
    private GameState _currentGameState;
    public GameState CurrentGameState
    {
        get => _currentGameState;
        set
        {
            _currentGameState = value;
            switch (_currentGameState)
            {
                case GameState.Playing:
                    OnGameStarting?.Invoke();
                    ResetData();
                    StartCoroutine(ReadyCountdown());
                    /*BGSoundManager.Instance.PlayBackgroundSound();*/
                    /*Time.timeScale = 1;*/
                    break;
                case GameState.Ending:
                    OnGameEnding?.Invoke();
                    /*Time.timeScale = 0;*/
                    BGSoundManager.Instance.StopBackgroundSound();
                    StopSpawnBall();
                    /*Sequence(Delay(1.0).OnComplete(() =>
                    {
                        BGSoundManager.Instance.StopBackgroundSound();
                    }));*/
                    break;
            }
        }
    }
    
    private int _currentTime;

    public int CurrentTime
    {
        private set
        {
            _currentTime = value;
            OnTimeChanged?.Invoke(_currentTime);
        }
        get => _currentTime;
    }
    
    /*private Coroutine _spawnBallCoroutine;*/
    
    private Coroutine _leftSpawnCoroutine;
    private Coroutine _rightSpawnCoroutine;
    protected override void Awake()
    {
        base.Awake();
        /*DontDestroyOnLoad();*/
        _baseMinSpawnDelay = minSpawnDelay;
        _baseMaxSpawnDelay = maxSpawnDelay;
    }

    private void Start()
    {
        OnMissedHit += MissedHit;
        CurrentGameState = GameState.Playing;

        /*intervalSpawn = levelTime / _currentBallSpeed;*/

    }

    private void OnDestroy()
    {
        UnregisterEvents();
    }
    
    private void UnregisterEvents()
    {
        OnGameStarting = null;
        OnGameEnding = null;
        OnTimeChanged = null;
        OnScoreChanged = null;
    }

    private void ResetData()
    {
        totalTime = 60;
        timePlay = 0;
        CurrentTime = totalTime;
        Score = 0;
        comboActive = false;
        consecutiveHits = 0;
        missedHit = 0;
        if (CurrentLevel == 1)
        {
            _currentBallSpeed = speedBallBase;
            minSpawnDelay = _baseMinSpawnDelay;
            maxSpawnDelay = _baseMaxSpawnDelay;
        }
        else
        {
            //cấp số nhân
            minSpawnDelay = _baseMinSpawnDelay * Mathf.Pow(0.05f, CurrentLevel - 1);
            maxSpawnDelay = _baseMaxSpawnDelay * Mathf.Pow(0.05f, CurrentLevel - 1);
            //tuyến tính
            /*_currentBallSpeed = speedBallBase * (1 + 0.5f * (CurrentLevel - 1));*/
            
        }
    }

    #region WIN LOSE
    
    private void OnGameWin()
    {
        Debug.Log("Win");
        SoundManager.Instance.PlaySound2D(Sound.Win);
        Time.timeScale = 0;
        CurrentGameState = GameState.Ending;
        DataManager.Instance.Energy++;
        UIManager.Instance.Show(UIManager.Panel.WinPanel);
    }

    private void OnGameLose()
    {
        Debug.Log("Lose");
        SoundManager.Instance.PlaySound2D(Sound.Lose);
        Time.timeScale = 0;
        CurrentGameState = GameState.Ending;
        UIManager.Instance.Show(UIManager.Panel.LosePanel);
    }
    #endregion

    #region Logic and Events

    public void AddScore(int amount, Transform ballTransform)
    {
        // Nếu comboActive và amount > 0 (đấm đúng) => nhân đôi
        if (comboActive && amount > 0)
        {
            amount *= 2;
            SoundManager.Instance.PlaySound(Sound.RealBallExplosion, ballTransform.position);
            Transform vfxCombo = ObjectPutter.Instance.PutObject(SpawnerType.VFXCombo);
            vfxCombo.position = ballTransform.position;
            vfxCombo.rotation = ballTransform.rotation;
        }

        Score += amount;

        // Xử lý combo
        if (amount > 0)
        {
            consecutiveHits++;
            if (consecutiveHits >= comboRequirement)
            {
                comboActive = true;
            }
        }
        else
        {
            // Đấm sai => reset combo
            consecutiveHits = 0;
            comboActive = false;
        }

        //UpdateUI();
    }
        int missedHit = 0;
    private void MissedHit()
    {
        missedHit++;
        if(missedHit > maxMissedHit)
        {
            OnGameLose();
        }
    }
    
    public void AddScore(int amount)
    {
        // Nếu comboActive và amount > 0 (đấm đúng) => nhân đôi
        if (comboActive && amount > 0)
        {
            amount *= 2;
        }

        Score += amount;

        // Xử lý combo
        if (amount > 0)
        {
            consecutiveHits++;
            if (consecutiveHits >= comboRequirement)
            {
                comboActive = true;
            }
        }
        else
        {
            // Đấm sai => reset combo
            consecutiveHits = 0;
            comboActive = false;
        }
        if(_score <= 0)
        {
            _score = 0;
        }
        //UpdateUI();
    }
    
    /*private void OnGameOverTime()
    {
        SoundManager.Instance.PlaySound(Sound.Lose, transform.position);
        Time.timeScale = 0;
        UIManager.Instance.Show(UIManager.Panel.GameOverTimePanel);
        /*Debug.Log("lose me m roi");#1#
    }*/
    private IEnumerator TimeCountDown()
    {
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(1f);
        while (CurrentTime > 0)
        {
            yield return wait;
            if (Mathf.Approximately(Time.timeScale, 1f))
            {
                timePlay++;
                if(timePlay >= 20)
                {
                    _currentBallSpeed *= 1.2f;
                }
                else if (timePlay >= 40)
                {
                    _currentBallSpeed *= 1.2f;
                }
                    CurrentTime--;
            }
            if (CurrentTime <= 0 && CurrentGameState == GameState.Playing)
            {
                Sequence(Delay(0.5).OnComplete(OnGameLose));
            }
        }

        
    }
    #endregion

    #region Spawn Level

    IEnumerator ReadyCountdown()
    {
        SoundManager.Instance.PlaySound2D(Sound.Ready);
        yield return new WaitForSeconds(1.0f);
        SoundManager.Instance.PlaySound2D(Sound.Countdown);
        yield return new WaitForSeconds(1.0f);
        SoundManager.Instance.PlaySound2D(Sound.Countdown);
        yield return new WaitForSeconds(1.0f);
        SoundManager.Instance.PlaySound2D(Sound.Countdown);
        yield return new WaitForSeconds(1.0f);
        SoundManager.Instance.PlaySound2D(Sound.Fight);
        if(SoundManager.Instance.IsSoundOn)
        {
            BGSoundManager.Instance.PlayBackgroundSound();
        }
        else
        {
            BGSoundManager.Instance.StopBackgroundSound();
        }
        StartCoroutine(TimeCountDown());
        StartSpawnBall();
    }
    /*private void StartSpawnBall()
    {
        if (_spawnBallCoroutine != null)
        {
            StopCoroutine(_spawnBallCoroutine);
        }
        _spawnBallCoroutine = StartCoroutine(SpawnBall());
    }*/
    
    private void StartSpawnBall()
    {
        // Khởi chạy 2 coroutine độc lập cho 2 spawn point
        _leftSpawnCoroutine = StartCoroutine(SpawnBallAt(spawnPointLeft));
        _rightSpawnCoroutine = StartCoroutine(SpawnBallAt(spawnPointRight));
    }
    
    /*public void StopSpawnBall()
    {
        if (_spawnBallCoroutine != null)
        {
            StopCoroutine(_spawnBallCoroutine);
            _spawnBallCoroutine = null;
        }
    }*/
    
    public void StopSpawnBall()
    {
        if (_leftSpawnCoroutine != null)
        {
            StopCoroutine(_leftSpawnCoroutine);
            _leftSpawnCoroutine = null;
        }
        if (_rightSpawnCoroutine != null)
        {
            StopCoroutine(_rightSpawnCoroutine);
            _rightSpawnCoroutine = null;
        }
    }

    IEnumerator SpawnBallAt(Transform spawnPoint)
    {
        MoveSpawnPoint moveSpawnPoint = GetComponent<MoveSpawnPoint>();

        while (CurrentGameState == GameState.Playing)
        {
            // Sinh bóng tại spawnPoint
            var randomType = GetRandomBallType();
            Transform ball = ObjectPutter.Instance.PutObject(randomType);
            if (ball)
            {
                ball.position = spawnPoint.position;
                ball.rotation = spawnPoint.rotation;
                if (ball.TryGetComponent(out Ball ballComponent))
                {
                    ballComponent.ActiveForce(_currentBallSpeed);
                }
            }

            // Đổi vị trí spawn sau khi spawn bóng
            ChangePosition();

            // Đợi một khoảng thời gian ngẫu nhiên trước khi spawn bóng tiếp theo
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);
        }
    }
    [SerializeField] List<Transform> pos;


    void ChangePosition()
    {
        HashSet<Transform> usedPositions = new HashSet<Transform>();

        if (pos.Count > 0)
        {
            Transform leftPosition;
            do
            {
                leftPosition = pos[UnityEngine.Random.Range(0, pos.Count)];
            } while (usedPositions.Contains(leftPosition));

            spawnPointLeft = leftPosition;
            usedPositions.Add(leftPosition);
        }

        if (pos.Count > 0)
        {
            Transform rightPosition;
            do
            {
                rightPosition = pos[UnityEngine.Random.Range(0, pos.Count)];
            } while (usedPositions.Contains(rightPosition));

            spawnPointRight = rightPosition;
            usedPositions.Add(rightPosition);
        }
    }
    /*IEnumerator SpawnBall()
    {
        while (CurrentGameState== GameState.Playing)
        {
            var randomTypeLeft = GetRandomBallType();
            Transform ballLeft = ObjectPutter.Instance.PutObject(randomTypeLeft);
            if (ballLeft)
            {
                ballLeft.position = spawnPointLeft.position;
                ballLeft.rotation = spawnPointLeft.rotation;
                if (ballLeft.TryGetComponent(out Ball ballLeftComponent))
                {
                    ballLeftComponent.ActiveForce(_currentBallSpeed);
                }
            }

            var randomTypeRight = GetRandomBallType();
            Transform ballRight = ObjectPutter.Instance.PutObject(randomTypeRight);
            if (ballRight)
            {
                ballRight.position = spawnPointRight.position;
                ballRight.rotation = spawnPointRight.rotation;
                if (ballRight.TryGetComponent(out Ball ballRightComponent))
                {
                    ballRightComponent.ActiveForce(_currentBallSpeed);
                }
            }

            yield return new WaitForSeconds(intervalSpawn);
        }
    }*/

    SpawnerType GetRandomBallType()
    {
        var randomIndex = Random.Range(0, BallSpawnerTypeList.Count);
        return BallSpawnerTypeList[randomIndex];
    }
    #endregion
    
}
