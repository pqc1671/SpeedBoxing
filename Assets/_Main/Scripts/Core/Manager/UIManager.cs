using System;
using UnityEngine;


public class UIManager : Singleton<UIManager>
{
    public enum Panel
    {
        PausePanel,
        WinPanel,
        LosePanel,
        OutOfEnergyPanel,
        ShopPanel
    }

    [SerializeField] private GameObject laserPoint;
    
    [Header("Panels")]
    [SerializeField]
    private PausePanel pausePanel;
    [SerializeField]
    private WinPanel winPanel;
    [SerializeField]
    private LosePanel losePanel;

    private void HideLaser()
    {
        laserPoint.SetActive(false);
    }
    
    private void ShowLaser()
    {
        laserPoint.SetActive(true);
    }
    private void Start()
    {
        Time.timeScale = 1;
        HideLaser();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        RegisterEvent();
    }
    
    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Three)&& Time.timeScale!=0)
        {
            if(pausePanel.gameObject.activeSelf)
                return;
            BGSoundManager.Instance.StopBackgroundSound();
            Time.timeScale = 0;
            Show(Panel.PausePanel);
        }
    }
    private void OutOfEnergy()
    {
        losePanel.Deactivate();
        Show(Panel.OutOfEnergyPanel);
    }

    private void RegisterEvent()
    {
        DataManager.Instance.OnOutOfEnergy += OutOfEnergy;
    }
    
    public void Show(Panel panel)
    {
        ShowLaser();
        switch (panel)
        {
            case Panel.PausePanel:
                pausePanel.Open(Constants.DurationOpenPanel);
                break;
            case Panel.WinPanel:
                winPanel.Open(Constants.DurationOpenPanel);
                break;
            case Panel.LosePanel:
                losePanel.Open(Constants.DurationOpenPanel);
                break;
        }
    }
    
    public void Hide(Panel panel)
    {
        HideLaser();
        switch (panel)
        {
            case Panel.PausePanel:
                pausePanel.Close(Constants.DurationClosePanel);
                break;
            case Panel.WinPanel:
                winPanel.Close(Constants.DurationClosePanel);
                break;
            case Panel.LosePanel:
                losePanel.Close(Constants.DurationClosePanel);
                break;
        }
    }
    
}
