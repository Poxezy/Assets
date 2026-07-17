using UnityEngine;

public class KnowledgeItem : MonoBehaviour
{
    [Header("Informasi Item")]
    [SerializeField] private string itemId = "book_classroom_01";
    [SerializeField] private string itemName = "Programming Book";
    [SerializeField] private int scoreValue = 50;

    [Header("Penyimpanan")]
    [SerializeField] private bool collectOnlyOnce = true;

    private bool hasBeenCollected;

    private string CollectionKey =>
        $"KnowledgeItem_{itemId}_Collected";

    private void Start()
    {
        if (!collectOnlyOnce)
        {
            return;
        }

        bool alreadyCollected =
            PlayerPrefs.GetInt(CollectionKey, 0) == 1;

        if (alreadyCollected)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenCollected ||
            !other.CompareTag("Player"))
        {
            return;
        }

        if (ScoreManager.Instance == null)
        {
            Debug.LogError(
                "ScoreManager tidak ditemukan."
            );

            return;
        }

        hasBeenCollected = true;

        ScoreManager.Instance.AddScore(scoreValue);

        if (collectOnlyOnce)
        {
            PlayerPrefs.SetInt(CollectionKey, 1);
            PlayerPrefs.Save();
        }

        Debug.Log(
            $"{itemName} dikumpulkan. +" +
            $"{scoreValue} poin."
        );

        Destroy(gameObject);
    }
}