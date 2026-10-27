using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour, IObserver
{
    public static UIManager Instance;

    public GameObject hitUI;

    public GameObject deathUI;

    public TextMeshProUGUI ammoText;

    public Image healthBar;
    public Gradient healthGradient;

    private PlayerHealth playerHealth;

    private void Awake()
    {
        Time.timeScale = 1f;
        Instance = this;
    }

    private void Start()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogError("UIManager could not find a PlayerHealth component.");
            return;
        }

        playerHealth.Attach(this);
        SetHealthValue(playerHealth.health);
    }

    public void OnSubjectChanged(ISubject subject)
    {
        PlayerHealth healthSubject = subject as PlayerHealth;

        if (healthSubject == null)
        {
            Debug.LogError("UIManager received an unsupported subject.");
            return;
        }

        PlayerLook.Instance.AddShake(0.1f, 0.25f);
        InstantiateHitUI();
        AudioManager.Instance.PlaySFX(healthSubject.hitSFX);
        SetHealthValue(healthSubject.health);

        if (healthSubject.health <= 0)
        {
            EnableDeathUI();
        }
    }

    public void InstantiateHitUI()
    {
        Instantiate(hitUI, transform);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void EnableDeathUI()
    {
        deathUI.SetActive(true);
    }

    public void SetHealthValue(int health)
    {
        float floatHealth = (float)health / 100;
        healthBar.color = healthGradient.Evaluate(floatHealth);
        healthBar.fillAmount = floatHealth;
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.Detach(this);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
