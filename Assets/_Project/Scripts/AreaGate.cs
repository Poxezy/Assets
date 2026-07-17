using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaGate : MonoBehaviour
{
    [Header("Area Tujuan")]
    [SerializeField] private string areaName = "Library";
    [SerializeField] private string targetScene = "Library";

    [Header("Pesan")]
    [SerializeField] private int requiredLevel = 2;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
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

        if (ScoreManager.Instance.IsAreaUnlocked(areaName))
        {
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            Debug.Log(
                $"{areaName} masih terkunci. " +
                $"Dibutuhkan Level {requiredLevel}."
            );
        }
    }
}