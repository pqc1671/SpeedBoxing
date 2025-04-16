using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataManager : Singleton<DataManager>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad();
    }

    private const int MinEnergy = 0;
    private const int MaxEnergy = 10;
    
    public Action<int> OnEnergyChanged;
    public Action OnOutOfEnergy;
    
    public int Energy
    {
        get => PlayerPrefs.GetInt(Constants.EnergyKey, MaxEnergy);
        set
        {
            PlayerPrefs.SetInt(Constants.EnergyKey, Mathf.Max(MinEnergy,value));
            PlayerPrefs.Save();
            OnEnergyChanged?.Invoke(value);
        }
    }
    
    public void TryDecreaseEnergyAndLoadScene(string sceneName)
    {
        if (Energy > MinEnergy)
        {
            //Energy--;
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.Log("Out of energy!");
            OnOutOfEnergy?.Invoke();
        }
    }
    
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        UnregisterEvents();
    }

    private void OnDestroy()
    {
        UnregisterEvents();
    }

    public void UnregisterEvents()
    {
        OnEnergyChanged = null;
        OnOutOfEnergy = null;
    }
}
