using System.Collections.Generic;
using UnityEngine;

namespace MetaEdu.Quiz
{
    /// <summary>
    /// Runtime quiz packs for knowledge books (no SO asset required).
    /// Topics: game design, MetaEdu campus, general knowledge.
    /// </summary>
    public static class BookQuizBank
    {
        static List<QuizPack> packs;

        public static QuizData CreateForItem(string itemId)
        {
            EnsurePacks();
            int idx = 0;
            if (!string.IsNullOrEmpty(itemId) && packs.Count > 0)
            {
                // unchecked abs — GetHashCode can be int.MinValue
                int h = itemId.GetHashCode() & 0x7FFFFFFF;
                idx = h % packs.Count;
            }

            var pack = packs[idx];
            var data = ScriptableObject.CreateInstance<QuizData>();
            data.quizID = pack.id;
            data.quizTitle = pack.title;
            data.questions = new List<QuizQuestion>(pack.questions);
            return data;
        }

        static void EnsurePacks()
        {
            if (packs != null) return;
            packs = new List<QuizPack>
            {
                Pack("game_basics", "Dasar Game",
                    Q("Apa arti FPS dalam genre game?",
                        new[] { "First Person Shooter", "Frames Per Second only", "Final Player Score", "Full Panel Screen" },
                        0, "FPS = First Person Shooter (sudut pandang orang pertama).", 25),
                    Q("Apa fungsi HUD dalam game?",
                        new[] { "Tampilkan info pemain (skor, nyawa)", "Hanya musik", "Ganti scene otomatis", "Hapus save data" },
                        0, "HUD = Heads-Up Display untuk info penting di layar.", 25),
                    Q("Apa yang dimaksud respawn?",
                        new[] { "Pemain muncul kembali setelah kalah/jatuh", "Upgrade senjata", "Pause game", "Ganti avatar saja" },
                        0, "Respawn = muncul ulang di titik aman.", 20)),

                Pack("game_design", "Desain Game",
                    Q("Apa itu level design?",
                        new[] { "Merancang tata ruang & tantangan level", "Coding AI musuh saja", "Membuat trailer", "Marketing game" },
                        0, "Level design mengatur layout, pacing, dan tantangan.", 25),
                    Q("Feedback positif ke pemain biasanya berupa…",
                        new[] { "Skor, badge, atau efek visual/suara", "Crash game", "Hapus progress", "Kunci semua area" },
                        0, "Reward & feedback memperkuat motivasi belajar/main.", 25),
                    Q("Collectible dalam game biasanya…",
                        new[] { "Item yang dikumpulkan untuk poin/ progres", "Bug yang harus diabaikan", "Tombol pause", "Skybox" },
                        0, "Buku di MetaEdu adalah contoh collectible pengetahuan.", 20)),

                Pack("metaedu", "MetaEdu Campus",
                    Q("Di MetaEdu, cara masuk ruangan lewat pintu adalah…",
                        new[] { "Dekati pintu lalu tekan E", "Lompat 3x", "Klik mouse kanan saja", "Tunggu 60 detik" },
                        0, "Pintu scene: zone trigger + tekan E.", 25),
                    Q("Buku bersinar di kampus berfungsi sebagai…",
                        new[] { "Sumber pengetahuan / kuis + poin", "Hiasan saja tanpa fungsi", "Pintu rahasia", "Musuh" },
                        0, "Ambil buku untuk kuis dan dapat poin.", 25),
                    Q("Campus Yard menghubungkan pemain ke…",
                        new[] { "Classroom dan Main Scene", "Hanya Main Menu", "Internet browser", "Editor Unity" },
                        0, "Campus Yard = hub antar area.", 20)),

                Pack("programming", "Dasar Pemrograman",
                    Q("Apa itu variable dalam pemrograman?",
                        new[] { "Tempat menyimpan nilai/data", "Nama file musik", "Jenis musuh", "Resolusi layar" },
                        0, "Variable menyimpan data yang bisa berubah.", 25),
                    Q("Loop digunakan untuk…",
                        new[] { "Mengulang perintah berkali-kali", "Menghapus project", "Mengunci cursor", "Bake lightmap" },
                        0, "for/while = pengulangan logika.", 25),
                    Q("Bug dalam software berarti…",
                        new[] { "Kesalahan yang membuat perilaku salah", "Fitur premium", "Skin karakter", "BGM" },
                        0, "Bug = error; diperbaiki lewat debugging.", 20)),

                Pack("unity", "Dasar Unity",
                    Q("GameObject di Unity adalah…",
                        new[] { "Objek di scene yang bisa punya komponen", "Hanya file audio", "Shader saja", "Build settings" },
                        0, "Hampir semua objek scene = GameObject + components.", 25),
                    Q("Script C# di Unity biasanya ditambahkan lewat…",
                        new[] { "Component pada GameObject", "Mengganti skybox", "Rename project", "Hapus Library folder" },
                        0, "Add Component → script mono behaviour.", 25),
                    Q("Play Mode di Unity digunakan untuk…",
                        new[] { "Menjalankan game di editor", "Export APK saja", "Hapus asset", "Ganti lisensi" },
                        0, "Tombol Play = uji runtime di editor.", 20)),

                Pack("general_edu", "Pengetahuan Umum",
                    Q("Ibu kota Indonesia adalah…",
                        new[] { "Jakarta", "Bandung", "Surabaya", "Medan" },
                        0, "Ibu kota Indonesia: Jakarta.", 20),
                    Q("Planet terdekat dengan Matahari…",
                        new[] { "Merkurius", "Bumi", "Mars", "Neptunus" },
                        0, "Merkurius orbit paling dekat ke Matahari.", 20),
                    Q("Bahasa pemrograman yang umum di Unity…",
                        new[] { "C#", "PHP", "Ruby only", "HTML saja" },
                        0, "Unity modern memakai C#.", 25)),

                Pack("gamedev_ethics", "Game & Belajar",
                    Q("Gamifikasi dalam edukasi bertujuan…",
                        new[] { "Membuat belajar lebih menarik lewat poin/badge", "Menghapus materi", "Mengganti guru", "Matikan UI" },
                        0, "Poin, level, badge = motivasi belajar.", 25),
                    Q("Leaderboard berguna untuk…",
                        new[] { "Melihat peringkat skor pemain", "Menghapus save", "Ganti resolusi", "Bake navmesh" },
                        0, "Papan peringkat memotivasi kompetisi sehat.", 20),
                    Q("Pause menu biasanya dibuka dengan…",
                        new[] { "Esc", "Spasi terus-menerus", "Scroll wheel", "F12 wajib" },
                        0, "Di MetaEdu, Esc membuka pause.", 20)),

                Pack("esport_culture", "Budaya Game",
                    Q("Co-op berarti…",
                        new[] { "Bermain bersama (kerja sama)", "Bermain melawan AI saja", "Mode sunyi", "Offline patch" },
                        0, "Cooperative = kerja sama antar pemain.", 20),
                    Q("Tutorial level biasanya…",
                        new[] { "Mengajari kontrol & aturan dasar", "Level tersulit", "Ending game", "Credits saja" },
                        0, "Tutorial onboarding pemain baru.", 20),
                    Q("Save progress penting agar…",
                        new[] { "Kemajuan pemain tidak hilang", "FPS naik otomatis", "Texture hilang", "Cursor unlock" },
                        0, "Progress disimpan (PlayerPrefs/database).", 25)),
            };
        }

        static QuizPack Pack(string id, string title, params QuizQuestion[] qs)
        {
            return new QuizPack
            {
                id = id,
                title = title,
                questions = new List<QuizQuestion>(qs)
            };
        }

        static QuizQuestion Q(string text, string[] options, int correct, string explain, int score)
        {
            return new QuizQuestion
            {
                questionID = "",
                category = "book",
                difficulty = "normal",
                questionText = text,
                answerOptions = options,
                correctAnswerIndex = correct,
                explanation = explain,
                scoreValue = score
            };
        }

        class QuizPack
        {
            public string id;
            public string title;
            public List<QuizQuestion> questions;
        }
    }
}
