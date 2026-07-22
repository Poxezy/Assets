using UnityEngine;

namespace MetaEdu.Player
{
    public class PlayerRespawn : MonoBehaviour
    {
        [Header("Respawn Settings")]
        [Tooltip("Titik spawn tempat player akan dikembalikan.")]
        public Transform spawnPoint;

        [Tooltip("Jika posisi Y player di bawah nilai ini, player akan di-respawn.")]
        public float fallThreshold = -10f;

        [Tooltip("Tekan R untuk respawn manual ke spawn point.")]
        public bool enableManualRespawn = true;

        private CharacterController cc;
        private FPSController fpsController;

        void Start()
        {
            cc = GetComponent<CharacterController>();
            fpsController = GetComponent<FPSController>();

            // Hardcoded spawn per scene (posisi tengah ruangan classroom)
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Vector3 forcedPosition = GetSceneSpawnPosition(sceneName);

            // Jika spawn point belum di-set, buat dari posisi yang sudah dikoreksi
            if (spawnPoint == null)
            {
                GameObject sp = new GameObject("SpawnPoint_Auto");
                sp.transform.position = forcedPosition;
                sp.transform.rotation = transform.rotation;
                spawnPoint = sp.transform;
                Debug.Log($"PlayerRespawn: SpawnPoint dibuat di {spawnPoint.position}");
            }

            // Force player ke posisi yang benar
            if (forcedPosition != Vector3.zero)
            {
                if (cc != null) cc.enabled = false;
                transform.position = forcedPosition;
                if (cc != null) cc.enabled = true;
                Debug.Log($"PlayerRespawn: Player dipindahkan ke {forcedPosition} (scene: {sceneName})");
            }
        }

        private Vector3 GetSceneSpawnPosition(string sceneName)
        {
            switch (sceneName)
            {
                case "classroom":
                    // Tengah ruangan: x antara -11.7~4.1, z antara -18.7~2.3, lantai di y=6.49
                    return new Vector3(-3.8f, 7.5f, -8f);
                default:
                    return transform.position;
            }
        }

        void Update()
        {
            // Respawn manual dengan tombol R
            if (enableManualRespawn && Input.GetKeyDown(KeyCode.R))
            {
                Respawn();
                return;
            }

            // Auto-respawn jika jatuh di bawah threshold
            if (transform.position.y < fallThreshold)
            {
                Respawn();
            }
        }

        public void Respawn()
        {
            if (spawnPoint == null)
            {
                Debug.LogError("PlayerRespawn: Tidak ada spawn point!");
                return;
            }

            // Disable CharacterController agar teleport tidak bentrok
            if (cc != null) cc.enabled = false;

            // Teleport ke spawn point
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;

            Debug.Log($"PlayerRespawn: Player di-respawn ke {spawnPoint.position}");

            // Enable kembali CharacterController
            if (cc != null) cc.enabled = true;
        }

        /// <summary>
        /// Set spawn point ke posisi baru (misalnya dari checkpoint).
        /// </summary>
        public void SetSpawnPoint(Vector3 position)
        {
            if (spawnPoint == null)
            {
                GameObject sp = new GameObject("SpawnPoint");
                sp.transform.position = position;
                spawnPoint = sp.transform;
            }
            else
            {
                spawnPoint.position = position;
            }
            Debug.Log($"PlayerRespawn: SpawnPoint diupdate ke {position}");
        }
    }
}
