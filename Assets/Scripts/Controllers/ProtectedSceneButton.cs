using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// ProtectedSceneButton : Composant à attacher à un Button qui charge une scène protégée
/// Usage : Ajoute ce script au bouton et configure le nom de la scène dans l'inspecteur
/// </summary>
public class ProtectedSceneButton : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private GameObject loginPanelToShow;
    
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("ProtectedSceneButton doit être attaché à un bouton (Button component)!");
            return;
        }

        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        // Vérifier si l'utilisateur est connecté
        if (AuthService.Instance != null && AuthService.Instance.IsUserLoggedIn())
        {
            // Charger la scène
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            // Afficher le panel de login
            Debug.Log($"Accès refusé à {targetSceneName} - Affichage du panel de login");
            
            if (loginPanelToShow != null)
            {
                loginPanelToShow.SetActive(true);
            }
            else
            {
                Debug.LogWarning("LoginPanel not assigned in ProtectedSceneButton!");
            }
        }
    }
}
