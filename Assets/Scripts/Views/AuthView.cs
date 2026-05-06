using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AuthView : MonoBehaviour
{
    [Header("Login Panel")]
    public GameObject loginPanel;
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public Button loginButton;
    public Button switchToRegisterButton;
    public TextMeshProUGUI loginStatusText;

    [Header("Register Panel")]
    public GameObject registerPanel;
    public TMP_InputField registerEmailInput;
    public TMP_InputField registerPasswordInput;
    public TMP_InputField registerConfirmPasswordInput;
    public Button registerButton;
    public Button switchToLoginButton;
    public TextMeshProUGUI registerStatusText;

    [Header("Main UI")]
    public GameObject mainUI;
    public TextMeshProUGUI userEmailText;
    public Button logoutButton;

    private AuthService authService;

    void Start()
    {
        authService = FindObjectOfType<AuthService>();
        authService.AuthStateChanged += OnAuthStateChanged;

        // Setup login panel
        loginButton.onClick.AddListener(OnLoginClick);
        switchToRegisterButton.onClick.AddListener(() => SwitchPanel(registerPanel));

        // Setup register panel
        registerButton.onClick.AddListener(OnRegisterClick);
        switchToLoginButton.onClick.AddListener(() => SwitchPanel(loginPanel));

        // Setup main UI
        logoutButton.onClick.AddListener(OnLogoutClick);

    }

    private void OnAuthStateChanged(bool isLoggedIn, string userId, string email)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        bool isLoggedIn = authService.IsUserLoggedIn();

        loginPanel.SetActive(!isLoggedIn);
        registerPanel.SetActive(false);
        mainUI.SetActive(isLoggedIn);

        if (isLoggedIn)
        {
            userEmailText.text = "Connecté: " + authService.GetCurrentUserEmail();
        }
    }

    private void SwitchPanel(GameObject targetPanel)
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        targetPanel.SetActive(true);
    }

    private void OnLoginClick()
    {
        string email = loginEmailInput.text;
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            loginStatusText.text = "Veuillez remplir tous les champs";
            return;
        }

        loginStatusText.text = "Connexion en cours...";
        loginButton.interactable = false;

        authService.SignIn(email, password, (success, message) => {
            loginButton.interactable = true;
            if (success)
            {
                loginStatusText.text = "Connexion réussie!";
            }
            else
            {
                loginStatusText.text = "Erreur: " + message;
            }
        });
    }

    private void OnRegisterClick()
    {
        string email = registerEmailInput.text;
        string password = registerPasswordInput.text;
        string confirmPassword = registerConfirmPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            registerStatusText.text = "Veuillez remplir tous les champs";
            return;
        }

        if (password != confirmPassword)
        {
            registerStatusText.text = "Les mots de passe ne correspondent pas";
            return;
        }

        if (password.Length < 6)
        {
            registerStatusText.text = "Le mot de passe doit contenir au moins 6 caractères";
            return;
        }

        registerStatusText.text = "Inscription en cours...";
        registerButton.interactable = false;

        authService.SignUp(email, password, (success, message) => {
            registerButton.interactable = true;
            if (success)
            {
                registerStatusText.text = "Inscription réussie! Vous pouvez maintenant vous connecter.";
                SwitchPanel(loginPanel);
            }
            else
            {
                registerStatusText.text = "Erreur: " + message;
            }
        });
    }

    public void OnProfileButtonClick()
    {
        bool isLoggedIn = authService.IsUserLoggedIn();
        this.gameObject.SetActive(true);

        if (isLoggedIn)
        {
            SwitchPanel(mainUI);
            userEmailText.text = "Email : " + authService.GetCurrentUserEmail();
        }
        else
        {
            SwitchPanel(loginPanel);
        }
    }

    public void CloseAuthSystem()
    {
        this.mainUI.SetActive(false);
        this.loginPanel.SetActive(false);
        this.registerPanel.SetActive(false);
    }

    private void OnLogoutClick()
    {
        authService.SignOut();
        CloseAuthSystem();
    }
}