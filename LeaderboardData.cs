using System;
using System.Collections.Generic;

[Serializable]
public class PlayerLeaderboardData
{
    public string player_name;
    public int score;
    public int level;
}

[Serializable]
public class LeaderboardResponse
{
    public bool success;
    public PlayerLeaderboardData[] players;
}

/// <summary>
/// Wrapper khusus untuk menyimpan daftar pemain ke file JSON lokal.
/// JsonUtility Unity tidak bisa serialize List sebagai root object,
/// jadi perlu dibungkus class biasa seperti ini.
/// Ditambahkan untuk mendukung versi lokal DatabaseManager — tidak
/// mengubah PlayerLeaderboardData / LeaderboardResponse yang sudah ada.
/// </summary>
[Serializable]
public class LeaderboardSaveData
{
    public List<PlayerLeaderboardData> players = new List<PlayerLeaderboardData>();
}
