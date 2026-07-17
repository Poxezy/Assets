using System.Collections.Generic;
using UnityEngine;

namespace MetaEdu.MiniGames
{
    public class NetworkBuilderGame : MonoBehaviour
    {
        [System.Serializable]
        public class Connection
        {
            public string deviceA;
            public string deviceB;
        }

        public List<Connection> requiredConnections = new List<Connection>();
        private List<Connection> playerConnections = new List<Connection>();

        public void StartGame()
        {
            playerConnections.Clear();
        }

        public void AddConnection(string devA, string devB)
        {
            playerConnections.Add(new Connection { deviceA = devA, deviceB = devB });
        }

        public void SubmitTopology()
        {
            bool isCorrect = true;
            foreach (var req in requiredConnections)
            {
                bool found = false;
                foreach (var conn in playerConnections)
                {
                    if ((conn.deviceA == req.deviceA && conn.deviceB == req.deviceB) ||
                        (conn.deviceA == req.deviceB && conn.deviceB == req.deviceA))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    isCorrect = false;
                    break;
                }
            }

            int finalScore = isCorrect ? 100 : 0;
            MiniGameManager.Instance.FinishMiniGame(MiniGameType.NetworkBuilder, finalScore, isCorrect);
        }
    }
}
