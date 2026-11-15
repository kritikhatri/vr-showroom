using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Connects the showroom's catalogue overlay with first-person input.</summary>
public class ShowroomUIActions : MonoBehaviour
{
    [SerializeField] private ProductCatalogueNavigation navigation;
    [SerializeField] private FirstPersonController playerMovement;
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject categoryPanel;
    [SerializeField] private GameObject cataloguePanel;
    [SerializeField] private ProductInformationPanel informationPanel;
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    private void Start()
    {
        // Start with the small showroom menu visible and the cursor available for UI.
        SetMovementEnabled(false);
        ShowHome();
    }

    public void OpenCategories()
    {
        if (navigation != null)
            navigation.ShowCategories();
        SetMovementEnabled(false);
    }

    public void ReturnToMenu()
    {
        SetMovementEnabled(false);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ResumeShowroom()
    {
        if (homePanel != null) homePanel.SetActive(false);
        if (categoryPanel != null) categoryPanel.SetActive(false);
        if (cataloguePanel != null) cataloguePanel.SetActive(false);
        if (informationPanel != null) informationPanel.HidePanel();
        SetMovementEnabled(true);
    }

    public void ShowHome()
    {
        if (navigation != null)
            navigation.ShowHome();
        SetMovementEnabled(false);
    }

    private void SetMovementEnabled(bool enabled)
    {
        if (playerMovement != null)
            playerMovement.enabled = enabled;
    }
}
