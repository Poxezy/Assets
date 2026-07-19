using TMPro;
using UnityEngine;

/// <summary>
/// Versi LOKAL dari LeaderboardDisplay: ambil data langsung dari
/// DatabaseManager.Instance.GetLeaderboard() (yang sekarang baca dari file
/// JSON lokal), bukan hit server PHP lewat UnityWebRequest/Coroutine.
///
/// Format tampilan dan bentuk data (LeaderboardResponse, PlayerLeaderboardData)
/// SAMA seperti versi online kalian, jadi kalau nanti DatabaseManager
/// diisi ulang untuk hit server sungguhan, script ini tidak perlu diubah.
/// </summary>
public class LeaderboardDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text leaderboardText;
    [SerializeField] private int topCount = 10;

    private void Start()
    {
        RefreshLeaderboard();
    }

    /// <summary>Panggil ulang kalau perlu refresh tampilan (misal setelah quiz/quest selesai).</summary>
    public void RefreshLeaderboard()
    {
        if (DatabaseManager.Instance == null)
        {
            leaderboardText.text = "Leaderboard belum tersedia";
            return;
        }

        LeaderboardResponse response = DatabaseManager.Instance.GetLeaderboard(topCount);

        if (!response.success || response.players == null || response.players.Length == 0)
        {
            leaderboardText.text = "Belum ada data leaderboard";
            return;
        }

        leaderboardText.text = "RANK     PLAYER     LEVEL     POINT\n\n";
        for (int i = 0; i < response.players.Length; i++)
        {
            PlayerLeaderboardData player = response.players[i];
            leaderboardText.text +=
                $"{i + 1}.     {player.player_name}     " +
                $"Lv.{player.level}     {player.score}\n";
        }
    }
}
