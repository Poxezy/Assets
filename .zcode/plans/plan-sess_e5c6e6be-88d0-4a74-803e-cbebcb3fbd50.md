## Rencana: Penyesuaian Player di Scene Classroom

### Ringkasan
Tiga perubahan: **tambah jump**, **respawn saat jatuh**, dan **cegah jatuh keluar area** classroom.

---

### 1. Modifikasi `FPSController.cs` — Tambah Fitur Jump

**File:** `Assets/_Project/Scripts/Player/FPSController.cs`

Menambahkan:
- Field `jumpHeight` (default `3f`) — tinggi lompatan
- Deteksi `Input.GetButtonDown("Jump")` di Update
- Saat grounded dan Jump ditekan: `verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y)`
- Gravity yang sudah ada tetap jalan — tidak ada perubahan pada mekanik jatuh

---

### 2. Script Baru: `PlayerRespawn.cs`

**File:** `Assets/_Project/Scripts/Player/PlayerRespawn.cs`

Komponen yang ditempel di GameObject Player:
- `spawnPoint` (Transform) — reference ke titik spawn (bisa di-set di Inspector)
- `fallThreshold` (float, default `-10f`) — jika player Y di bawah ini → respawn
- `enableManualRespawn` (bool) — tekan `R` untuk respawn manual
- Saat Start: jika `spawnPoint` null, simpan posisi awal sebagai spawn point
- Saat Update: cek `transform.position.y < fallThreshold` → teleport ke spawn point, reset vertical velocity (via reflection/internal method)

---

### 3. Setup SpawnPoint di Classroom Scene

**Langkah manual di Unity Editor (saya akan pandu):**
- Tambah empty GameObject bernama `SpawnPoint` di classroom scene
- Posisikan di tengah ruangan pada y yang aman (sekitar `(-4, 8, -8)`)
- Assign SpawnPoint ke `PlayerRespawn.spawnPoint` pada Player
- Set `fallThreshold` ke `0` (jauh di bawah floor classroom di y=6.49)

Dengan ini, player tidak akan jatuh selamanya — begitu melewati batas bawah, langsung teleport balik ke dalam kelas.

---

### File yang Diubah / Dibuat

| File | Aksi |
|---|---|
| `Assets/_Project/Scripts/Player/FPSController.cs` | Edit — tambah jump |
| `Assets/_Project/Scripts/Player/PlayerRespawn.cs` | **Baru** — script respawn |
| `Assets/_Project/Scenes/classroom.unity` | Manual via Unity — tambah SpawnPoint + assign reference |

---

### Tidak Perlu
- Tidak perlu invisible wall tambahan — classroom sudah punya dinding dan lantai. Respawn otomatis menangani kasus player glitch keluar.
- Tidak perlu ubah Physics.gravity global.