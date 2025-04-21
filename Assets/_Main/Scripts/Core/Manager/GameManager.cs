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
        set => _score = Mathf.Max(0, value);
        get => _score;
    }

    [Header("Combo Settings")]
    public int comboRequirement = 5;
    public bool comboActive = false;
    public int consecutiveHits = 0;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPointLeft;
    [SerializeField] private Transform spawnPointRight;
    [SerializeField] private float minSpawnDelay = 0.5f;
    [SerializeField] private float maxSpawnDelay = 1.5f;
    public float speedBallBase = 30f;

    [SerializeField] private List<Transform> pos;

    private float _baseMinSpawnDelay;
    private float _baseMaxSpawnDelay;
    private int _currentLevel = 1;
    private float _currentBallSpeed = 10f;
    private int timePlay = 0;
    private int missedHit = 0;
    private const int maxMissedHit = 10;

    private Coroutine _leftSpawnCoroutine;
    private Coroutine _rightSpawnCoroutine;

    private GameState _currentGameState;

    public Action OnGameStarting;
    public Action OnGameEnding;
    public Action OnMissedHit;
    public Action<int> OnScoreChanged;
    public Action<int> OnMissedChanged;

    private List<SpawnerType> BallSpawnerTypeList = new List<SpawnerType>()
    {
        SpawnerType.RedBall,
        SpawnerType.YellowBall,
        SpawnerType.WhiteBall,
        SpawnerType.GreyBall,
        SpawnerType.OrangeBall
    };

    private List<SpawnerType> originalBallList;

    private List<SpawnerType> AdvanceBallList = new List<SpawnerType>()
    {
        SpawnerType.RedBall1,
        SpawnerType.YellowBall1,
        SpawnerType.WhiteBall1,
        SpawnerType.GreyBall1,
        SpawnerType.OrangeBall1
    };

    public int CurrentLevel
    {
        get => _currentLevel;
        set
        {
            int newLevel = (value - 1) % 3 + 1;
            if (value > 1 && newLevel == 1)
            {
                speedBallBase *= 1.3f;
                Debug.Log($"Quay về Level 1 - tăng speed bóng lên: {speedBallBase}");
            }

            _currentLevel = newLevel;
        }
    }

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
                    break;
                case GameState.Ending:
                    OnGameEnding?.Invoke();
                    BGSoundManager.Instance.StopBackgroundSound();
                    StopSpawnBall();
                    break;
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        _baseMinSpawnDelay = minSpawnDelay;
        _baseMaxSpawnDelay = maxSpawnDelay;
        originalBallList = new List<SpawnerType>(BallSpawnerTypeList);
    }

    private void Start()
    {
        OnMissedHit += MissedHit;
        CurrentGameState = GameState.Playing;
    }

    private void OnDestroy()
    {
        OnGameStarting = null;
        OnGameEnding = null;
        OnScoreChanged = null;
        OnMissedChanged = null;
    }

    private void ResetData()
    {
        timePlay = 0;
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
    }

    private void OnGameLose()
    {
        Debug.Log("Lose");
        SoundManager.Instance.PlaySound2D(Sound.Lose);
        Time.timeScale = 0;
        CurrentGameState = GameState.Ending;
        UIManager.Instance.Show(UIManager.Panel.LosePanel);
    }

    private IEnumerator TimeCountDown()
    {
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(1f);
        while (true)
        {
            yield return wait;
            if (Mathf.Approximately(Time.timeScale, 1f))
            {
                timePlay++;
                if (timePlay >= 30)
                {
                    timePlay = 0;
                    CurrentLevel++;
                    ApplyLevelSettings();
                }
            }
        }
    }

    private void ApplyLevelSettings()
    {
        switch (CurrentLevel)
        {
            case 1:
                BallSpawnerTypeList = new List<SpawnerType>(originalBallList);
                spawnPointLeft = pos[0];
                spawnPointRight = pos[1];
                break;

            case 2:
                Debug.Log("Level 2: thêm bóng nâng cao");
                BallSpawnerTypeList = new List<SpawnerType>(originalBallList);
                BallSpawnerTypeList.AddRange(AdvanceBallList);
                break;

            case 3:
                Debug.Log("Level 3: spawn ngẫu nhiên toàn bộ điểm trong list");
                if (pos.Count >= 2)
                {
                    spawnPointLeft = pos[Random.Range(0, pos.Count)];
                    do
                    {
                        spawnPointRight = pos[Random.Range(0, pos.Count)];
                    } while (spawnPointRight == spawnPointLeft);
                }
                break;
        }
    }

    public void AddScore(int amount, Transform ballTransform)
    {
        if (comboActive && amount > 0)
        {
            amount *= 2;
            SoundManager.Instance.PlaySound(Sound.RealBallExplosion, ballTransform.position);
            var vfx = ObjectPutter.Instance.PutObject(SpawnerType.VFXCombo);
            vfx.position = ballTransform.position;
            vfx.rotation = ballTransform.rotation;
        }

        Score += amount;
        OnScoreChanged?.Invoke(_score);

        if (amount > 0)
        {
            consecutiveHits++;
            if (consecutiveHits >= comboRequirement)
                comboActive = true;
        }
        else
        {
            consecutiveHits = 0;
            comboActive = false;
        }
    }

    public void AddScore(int amount)
    {
        if (comboActive && amount > 0)
            amount *= 2;

        Score += amount;
        OnScoreChanged?.Invoke(_score);

        if (amount > 0)
        {
            consecutiveHits++;
            if (consecutiveHits >= comboRequirement)
                comboActive = true;
        }
        else
        {
            consecutiveHits = 0;
            comboActive = false;
        }
    }

    private void MissedHit()
    {
        missedHit++;
        OnMissedChanged?.Invoke(missedHit);
        if (missedHit > maxMissedHit)
            OnGameLose();
    }

    IEnumerator ReadyCountdown()
    {
        SoundManager.Instance.PlaySound2D(Sound.Ready);
        yield return new WaitForSeconds(1f);
        SoundManager.Instance.PlaySound2D(Sound.Countdown);
        yield return new WaitForSeconds(1f);
        SoundManager.Instance.PlaySound2D(Sound.Countdown);
        yield return new WaitForSeconds(1f);
        SoundManager.Instance.PlaySound2D(Sound.Countdown);
        yield return new WaitForSeconds(1f);
        SoundManager.Instance.PlaySound2D(Sound.Fight);

        if (SoundManager.Instance.IsSoundOn)
            BGSoundManager.Instance.PlayBackgroundSound();
        else
            BGSoundManager.Instance.StopBackgroundSound();

        StartCoroutine(TimeCountDown());
        StartSpawnBall();
    }

    private void StartSpawnBall()
    {
        _leftSpawnCoroutine = StartCoroutine(SpawnBallAt(true));
        _rightSpawnCoroutine = StartCoroutine(SpawnBallAt(false));
    }

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

    IEnumerator SpawnBallAt(bool isLeft)
    {
        while (CurrentGameState == GameState.Playing)
        {
            Transform spawnPoint;

            if (CurrentLevel == 1)
                spawnPoint = isLeft ? spawnPointLeft : spawnPointRight;
            else if (CurrentLevel == 3)
                spawnPoint = pos[Random.Range(0, pos.Count)];
            else
                spawnPoint = isLeft ? spawnPointLeft : spawnPointRight;

            var randomType = GetRandomBallType();
            var ball = ObjectPutter.Instance.PutObject(randomType);
            if (ball)
            {
                ball.position = spawnPoint.position;
                ball.rotation = spawnPoint.rotation;
                if (ball.TryGetComponent(out Ball ballComponent))
                    ballComponent.ActiveForce(_currentBallSpeed);
            }

            if (CurrentLevel == 3)
                ChangePosition();

            yield return new WaitForSeconds(Random.Range(minSpawnDelay, maxSpawnDelay));
        }
    }

    void ChangePosition()
    {
        HashSet<Transform> used = new HashSet<Transform>();

        if (pos.Count > 0)
        {
            do { spawnPointLeft = pos[Random.Range(0, pos.Count)]; }
            while (!used.Add(spawnPointLeft));
        }

        if (pos.Count > 0)
        {
            do { spawnPointRight = pos[Random.Range(0, pos.Count)]; }
            while (!used.Add(spawnPointRight));
        }
    }

    SpawnerType GetRandomBallType()
    {
        return BallSpawnerTypeList[Random.Range(0, BallSpawnerTypeList.Count)];
    }
}
