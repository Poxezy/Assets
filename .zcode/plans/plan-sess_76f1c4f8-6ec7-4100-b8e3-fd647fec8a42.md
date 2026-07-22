# Rencana: UI Premium + Sistem Misi MetaEdu

## Konteks singkat
- UI hybrid: scene/prefab shell + banyak panel **runtime** (`QuestUI`, `HelpPanel`, `ProfilePanel`, `QuizUI`) + restyle `ExclusiveUIStyler` + palet `UITheme` (dark-gold).
- Misi = `QuestManager` / `QuestUI` (bukan prefab). 3 quest seed hardcoded.
- **Tidak ada** DOTween/LeanTween di project. **Tidak ada** waypoint/kompas.
- Bug progres: `NotifyBookCollected()` menaikkan **2** keyword sekaligus (`Ambil buku` + `kuis dari buku`); match objektif by string; progress count **tidak** tersimpan.

Pilihan user: **kompas penuh**, **package animasi**, **perbaiki logika misi**.

---

## Prinsip
- Mekanisme inti tetap: buku → kuis → skor; pintu classroom; prereq quest; HUD skor.
- Diff minimum, pola runtime UGUI + TMP dipertahankan.
- Animasi halus, unscaled time (pause-safe), bisa dimatikan.
- DOTween Free ditambah sebagai dependency (belum ada di `Packages/manifest.json`).

---

## 1. Animasi (DOTween)

**Langkah setup**
- Import **DOTween Free** (Asset Store / salin ke `Assets/Plugins/Demigiant/DOTween`).
- `DOTween.Init` sekali di bootstrap UI (mis. `UIMotion.EnsureInit()`).
- Jangan ubah `manifest.json` ke package yang tidak resmi kecuali user sudah punya UPM mirror.

**File baru:** `Assets/_Project/Scripts/UI/UIMotion.cs`
- API tipis: `FadeIn/Out(CanvasGroup)`, `PunchScale(RectTransform)`, `SlideIn`, `ToastPop`.
- Semua pakai `SetUpdate(true)` (jalan saat `timeScale=0`).
- Kill tween saat destroy/hide.

**Dipakai di:** main menu panel, pause open/close, quest card toggle, quest toast, reward popup, help/profile show/hide, quiz open.

---

## 2. Fondasi tema & tombol

**`UITheme.cs`**
- Tetap dark-gold; perjelas token: spacing (8/12/16/24), ukuran judul/body/hint, warna progress bar, locked/disabled.
- `ButtonColors()`: normal / highlighted / pressed / selected / disabled lebih tegas (highlight gold, disabled pudar + alpha).
- Helper: `StyleLabel`, progress fill color.

**`ExclusiveUIStyler.cs`**
- Tombol: ColorTint + outline gold; teks cream; **scale punch** via `UIMotion` (EventTrigger pointer down/up) — ringan, tanpa Animator.
- Jangan override `QuestCard`/`QuestBody` ke warna yang merusak hierarki (sudah ada name-match; rapikan agar konsisten).
- Pastikan Image panel gelap, teks terang (anti putih-di-putih di menu/pause/HUD).

**Tidak** menambah atlas sprite 9-slice baru (tanpa aset art) — polish lewat warna, outline, accent bar, spacing, motion.

---

## 3. Logika misi (fix + alur jelas)

### `QuestData.cs` — model objektif
```csharp
// QuestObjective
string objectiveId;      // "collect_book", "finish_book_quiz", "enter_classroom"
string description;      // teks UI singkat
string hintText;         // "Langkah: cari buku bersinar"
string targetTag;        // "Book" | "ClassroomDoor" | ""
int currentCount, requiredCount;
```

### `QuestManager.cs`
- `ReportObjective(string objectiveId, int amount=1)` — match **ID**, bukan keyword description.
- Pisah hook:
  - `NotifyBookCollected()` → hanya `collect_book`
  - `NotifyBookQuizFinished()` → hanya `finish_book_quiz`
  - `NotifyEnteredClassroom()` → `enter_classroom`
- Seed teks berurutan (pengenalan → tujuan → lokasi/tindakan → reward), contoh:
  1. **Jelajahi Campus Yard** — ambil 1 buku (hint: buku bersinar)
  2. **Kolektor Pengetahuan** — selesaikan 3 kuis buku (prereq intro)
  3. **Masuk Classroom** — temukan pintu emas, tekan E (prereq intro)
- `GetFocusTarget(out objectiveId, out targetTag, out worldHint)` untuk kompas.
- Persist ringkas lewat **PlayerPrefs** (status + `currentCount` per quest/obj) — tanpa memaksa wiring `SaveLoadManager` penuh (save game masih jarang dipanggil).
- `ResetAllQuests` bersihkan prefs progress.

### `KnowledgeItem.cs`
- Saat kuis selesai: `NotifyBookQuizFinished()` (+ opsional `NotifyBookCollected` hanya jika intro butuh collect — **satu** ID per seed agar tidak double-count).
- Rekomendasi seed: intro pakai `finish_book_quiz` ×1 (aksi nyata: selesaikan kuis buku); quest 2 pakai `finish_book_quiz` ×3. Satu notify di `OnQuizDone` cukup.

### Copy UI / Help
- `HelpPanel`: update instruksi misi + **J** + kompas.
- Toast: `QUEST AKTIF` / `QUEST SELESAI` + judul; bahasa singkat.

---

## 4. Panel misi (`QuestUI.cs`) — redesign

Layout top-right (CanvasScaler 1920×1080, sort 450):

| Zona | Isi | Warna |
|------|-----|--------|
| Header | `MISI` + jumlah aktif | Gold bold |
| Title | Judul quest | GoldSoft |
| Desc | 1–2 baris | Cream |
| Next step chip | `SELANJUTNYA · {hint}` | Gold on dark row |
| Objectives | `○/✓` + teks + `(n/m)` + **bar tipis** | Cream / Success |
| Reward | XP + badge (jika ada) | Muted |
| Footer | `J tutup` | Muted kecil |

Perbaikan layout:
- Card tinggi dinamis (ContentSizeFitter / hitung tinggi) + `RectMask2D` + scroll jika overflow.
- Padding konsisten 16; gap 6–8; tidak tumpuk header/body.
- Kontras: `PanelDark` + `CardInner`; **larang** cream di atas putih.
- Toggle **J**: fade+scale via `UIMotion`; default **terbuka** saat quest aktif baru.
- Toast di bawah card: fade in/out 3s.

---

## 5. Kompas penuh + waypoint dunia

**File baru**
- `Assets/_Project/Scripts/Quest/QuestWaypointService.cs` — resolve target aktif:
  - `Book` → `KnowledgeItem` aktif terdekat ke player
  - `ClassroomDoor` → `SceneDoor` target scene `classroom` (atau beacon bootstrap)
  - Cache refresh saat quest update / scene load
- `Assets/_Project/Scripts/Quest/QuestWorldMarker.cs` — marker runtime (pole/orb/light gold, mirip door beacon, ringkas) di posisi target; hide jika selesai/null.
- `Assets/_Project/Scripts/Quest/QuestCompassUI.cs` — HUD:
  - Jarum/panah rotasi ke arah target (plane XZ, kamera player)
  - Label jarak (m) + nama langkah singkat
  - Off-screen: panah tepi opsional **atau** jarum 360° di pojok (pilih jarum + jarak — cukup premium, murah)
  - Sembunyi di MainMenu/Leaderboard/saat quiz fullscreen

Boot: ikut `QuestManager.EnsureSystems()` (DontDestroyOnLoad `QuestSystems`).

Performa: update compass di `Update` ringan (satu target); marker tidak spawn tiap frame.

---

## 6. Main menu premium

**`MainMenuController.cs` + `ExclusiveUIStyler`**
- Saat `Start`: restyle canvas; staggered fade/slide tombol & title (`UIMotion`).
- Susunan visual: background gelap, panel menu accent gold, title hierarki jelas, spacing tombol seragam.
- Transisi `StartGame` / `OpenLeaderboard`: fade canvas singkat lalu load scene (coroutine + DOTween).
- Profile/Help: show/hide dengan fade+scale; tutup saling exclusive (sudah ada).

Tanpa redesign total scene hierarchy di Editor jika bisa dicapai runtime (polanya project sudah begitu).

---

## 7. Komponen UI lain (konsisten)

| Komponen | Perubahan |
|----------|-----------|
| `PauseMenu` | Fade panel; restyle; tombol state jelas; timescale 0 + tween unscaled |
| `GamificationUI` | HUD chip rapi; reward popup punch+fade; teks POINT/LEVEL/BADGE hierarki |
| `QuizUI` | Panel kontras; opsi hover/press; feedback warna Success/Danger; buka/tutup motion |
| `LeaderboardDisplay` | Baris rank konsisten; Back button styled; kontras |
| `SceneDoorPromptUI` | Chip sama bahasa desain |
| `ProfilePanel` / `HelpPanel` | Motion + copy misi terbaru |

Semua lewat `UITheme` + `UIMotion` + `ExclusiveUIStyler` — satu bahasa visual.

---

## 8. Responsif & stabilitas

- `CanvasScaler` reference **1920×1080**, match **0.5** di semua canvas runtime/yang di-fix code.
- Anchor quest: top-right; compass: top-center atau bawah-minimap kecil; HUD kiri-atas — tidak overlap (offset tetap).
- `RectMask2D` + ellipsis/wrap pada teks panjang.
- Jangan ubah input gameplay (WASD, E pintu, quiz) kecuali progress hooks.
- Kill tween on destroy; null-check scene tanpa player.

---

## 9. File yang disentuh / baru

**Baru**
- `Scripts/UI/UIMotion.cs`
- `Scripts/Quest/QuestWaypointService.cs`
- `Scripts/Quest/QuestWorldMarker.cs`
- `Scripts/Quest/QuestCompassUI.cs`

**Edit utama**
- `Scripts/UI/UITheme.cs`
- `Scripts/UI/ExclusiveUIStyler.cs`
- `Scripts/Quest/QuestData.cs`
- `Scripts/Quest/QuestManager.cs`
- `Scripts/Quest/QuestUI.cs`
- `Scripts/KnowledgeItem.cs`
- `Scripts/MainMenuController.cs`
- `Scripts/PauseMenu.cs`
- `Scripts/GamificationUI.cs`
- `Scripts/Quiz/QuizUI.cs` (motion + kontras)
- `Scripts/UI/HelpPanel.cs`, `ProfilePanel.cs`
- `Scripts/LeaderboardDisplay.cs` (ringan)
- Opsional: `SceneDoor.cs` / bootstrap jika perlu tag target classroom

**Eksternal**
- Folder DOTween Free di project

**Tidak diubah** (kecuali perlu tag/marker): mekanik FPS, mini-game scoring, area unlock formula, DB leaderboard.

---

## 10. Urutan implementasi

1. Import DOTween + `UIMotion` + perluas `UITheme` / `ExclusiveUIStyler`
2. Fix model + `QuestManager` + `KnowledgeItem` + persist progress
3. Rebuild `QuestUI` (hierarki + kontras + motion)
4. `QuestWaypointService` + world marker + `QuestCompassUI`
5. Main menu + pause + gamification + quiz + help/profile polish
6. Smoke-check alur & overlap UI

---

## 11. Checklist verifikasi (manual di Editor)

- [ ] MainMenu: kontras, hover/press tombol, anim masuk, Start → campusyard
- [ ] Quest card: teks terbaca, tidak putih-di-putih, tidak potong parah
- [ ] Ambil/selesaikan 1 kuis buku → intro complete sekali (tidak lompat aneh ke 3/3)
- [ ] Quest 2 butuh 3 kuis; progress bertahan setelah reload scene (PlayerPrefs)
- [ ] Kompas mengarah buku/pintu; marker dunia muncul; hilang saat objektif selesai
- [ ] Pintu classroom + E → quest visit complete
- [ ] J toggle panel; Esc pause + resume; quiz modal di atas HUD
- [ ] Reward toast/popup tidak flicker; timeScale 0 tidak membekukan anim UI
- [ ] Leaderboard + Help + Profile tetap jalan

---

## Risiko & mitigasi

| Risiko | Mitigasi |
|--------|----------|
| DOTween belum diimport | `UIMotion` clear error; langkah 1 wajib sebelum play |
| Target buku null (semua terkumpul) | Compass tampil “Tidak ada target” / sembunyi jarum |
| Multi-quest aktif (books + classroom) | Focus = objektif incomplete pertama dari quest aktif urutan seed |
| Overlap HUD | Anchor + padding fixed; compass terpisah dari quest card |

---

## Diluar scope (sengaja)
- Pathfinding navmesh, quest journal penuh, multi-bahasa, ganti font custom, art 9-slice baru, wiring NPC dialogue content, refactor total ke UI Toolkit.