using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class LeaderboardDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text leaderboardText;

    private const string LeaderboardUrl =
        "http://localhost/metaedu_api/leaderboard.php";

    private void Start()
    {
        StartCoroutine(LoadLeaderboard());
    }

    private IEnumerator LoadLeaderboard()
    {
        using UnityWebRequest request =
            UnityWebRequest.Get(LeaderboardUrl);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            leaderboardText.text = "Gagal mengambil leaderboard";
            yield break;
        }

        LeaderboardResponse response =
            JsonUtility.FromJson<LeaderboardResponse>(
                request.downloadHandler.text
            );

        leaderboardText.text =
            "RANK     PLAYER     LEVEL     POINT\n\n";

        for (int i = 0; i < response.players.Length; i++)
        {
            var player = response.players[i];

            leaderboardText.text +=
                $"{i + 1}.     {player.player_name}     " +
                $"Lv.{player.level}     {player.score}\n";
        }
    }
}