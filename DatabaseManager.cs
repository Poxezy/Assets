using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Versi LOKAL dari DatabaseManager, untuk demo/prototype tanpa server PHP.
/// Skor pemain disimpan ke file JSON di Application.persistentDataPath,
/// bukan dikirim lewat UnityWebRequest ke metaedu_api/save_score.php.
///
/// SaveScore(playerName, score, level) sengaja dipertahankan dengan
/// signature yang SAMA seperti versi online kalian, karena ScoreManager
/// sudah memanggil method ini (lihat ScoreManager.SaveToDatabase()) —
/// jadi tidak ada satu pun baris kode di ScoreManager yang perlu diubah.
///
/// Upgrade ke server nanti: ganti isi SaveScore() dan GetLeaderboard()
/// di file ini dengan UnityWebRequest seperti versi awal kalian.
/// Script lain (ScoreManager, LeaderboardDisplay) tidak perlu disentuh
/// karena mereka hanya bergantung pada method + bentuk data ini, bukan
/// cara data itu didapat.
/// </summary>
public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    private const string LeaderboardFileName = "leaderboard_local.json";

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

    /// <summary>Simpan/update skor pemain. Dipanggil otomatis oleh ScoreManager.AddScore().</summary>
    public void SaveScore(string playerName, int score, int level)
    {
        LeaderboardSaveData data = LoadLocalData();

        PlayerLeaderboardData existing = data.players
            .FirstOrDefault(p => p.player_name == playerName);

        if (existing != null)
        {
            existing.score = score;
            existing.level = level;
        }
        else
        {
            data.players.Add(new PlayerLeaderboardData
            {
                player_name = playerName,
                score = score,
                level = level
            });
        }

        SaveLocalData(data);
    }

    /// <summary>
    /// Ambil leaderboard, dibungkus persis seperti response API online
    /// (LeaderboardResponse) supaya LeaderboardDisplay tidak perlu tahu
    /// datanya dari lokal atau dari server.
    /// </summary>
    public LeaderboardResponse GetLeaderboard(int topCount = 10)
    {
        LeaderboardSaveData data = LoadLocalData();

        PlayerLeaderboardData[] sorted = data.players
            .OrderByDescending(p => p.score)
            .Take(topCount)
            .ToArray();

        return new LeaderboardResponse
        {
            success = true,
            players = sorted
        };
    }

    private LeaderboardSaveData LoadLocalData()
    {
        string path = Path.Combine(Application.persistentDataPath, LeaderboardFileName);

        if (!File.Exists(path))
            return new LeaderboardSaveData();

        string json = File.ReadAllText(path);
        LeaderboardSaveData data = JsonUtility.FromJson<LeaderboardSaveData>(json);
        return data ?? new LeaderboardSaveData();
    }

    private void SaveLocalData(LeaderboardSaveData data)
    {
        string path = Path.Combine(Application.persistentDataPath, LeaderboardFileName);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }
}
