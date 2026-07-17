using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    private const string SaveScoreUrl =
        "http://localhost/metaedu_api/save_score.php";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveScore(string playerName, int score, int level)
    {
        StartCoroutine(SaveScoreCoroutine(playerName, score, level));
    }

    private IEnumerator SaveScoreCoroutine(
        string playerName,
        int score,
        int level)
    {
        WWWForm form = new WWWForm();

        form.AddField("player_name", playerName);
        form.AddField("score", score);
        form.AddField("level", level);

        using UnityWebRequest request =
            UnityWebRequest.Post(SaveScoreUrl, form);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Database: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogWarning(
                "Gagal menyimpan score (server offline): " + request.error
            );
        }
    }
}