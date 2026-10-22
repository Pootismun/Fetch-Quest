using UnityEngine;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance { get; private set; }

    [SerializeField] private GameObject[] lifeIcons;

    private int lives;

    public int Lives => lives;

    private void Awake()
    {
        Instance = this;
        lives = lifeIcons != null ? lifeIcons.Length : 0;
    }

    public void LoseLife()
    {
        if (lives <= 0)
        {
            return;
        }

        lives--;
        lifeIcons[lives].SetActive(false);

        if (DamageFeedback.Instance != null)
        {
            DamageFeedback.Instance.Play();
        }
    }

    

[ContextMenu("Test Lose Life")]
    private void TestLoseLife()
    {
        LoseLife();
    }
}
