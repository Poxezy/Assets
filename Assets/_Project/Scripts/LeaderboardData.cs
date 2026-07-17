using System;

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