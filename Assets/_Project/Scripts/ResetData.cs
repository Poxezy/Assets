using UnityEngine;

public class ResetData : MonoBehaviour
{
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("Progress PlayerPrefs berhasil dihapus.");
    }
}