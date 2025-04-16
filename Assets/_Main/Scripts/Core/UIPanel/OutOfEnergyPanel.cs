using UnityEngine;

public class OutOfEnergyPanel : UIPanel
{
    public void OnShopButtonClicked()
    {
        SoundManager.Instance.PlaySound2D(Sound.Click);
        UIManager.Instance.Hide(UIManager.Panel.OutOfEnergyPanel);
        UIManager.Instance.Show(UIManager.Panel.ShopPanel);
    }
    
    public void OnHomeButtonClicked()
    {
        SoundManager.Instance.PlaySound2D(Sound.Click);
        UIManager.Instance.Hide(UIManager.Panel.OutOfEnergyPanel);
        DataManager.Instance.LoadScene("MainMenu");
    }

    public void OnHomeInMainMenuClicked()
    {
        SoundManager.Instance.PlaySound2D(Sound.Click);
        MainMenuUIManager.Instance.Hide(MainMenuUIManager.MainMenuPanel.OutOfEnergyPanel);
        MainMenuUIManager.Instance.Show(MainMenuUIManager.MainMenuPanel.MainPanel);
    }
    
}
