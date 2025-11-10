using UnityEngine;
using System.Collections.Generic; // Penting untuk menggunakan List

public class PetSelectionManager : MonoBehaviour
{
    // Seret semua objek 'Highlight_Image' Anda ke list ini di Inspector
    public List<GameObject> highlightObjects;

    private int currentSelectedIndex = -1; // -1 berarti tidak ada yg dipilih

    void Start()
    {
        // Pastikan semua highlight mati di awal permainan
        DeselectAll();
    }

    // Ini adalah fungsi UTAMA yang akan dipanggil oleh Tombol
    public void SelectItem(int index)
    {
        // Jika mengklik item yang sama lagi, tidak terjadi apa-apa
        if (index == currentSelectedIndex)
        {
            return; 
        }

        // 1. Matikan dulu highlight yang sedang aktif (jika ada)
        DeselectAll();

        // 2. Nyalakan highlight baru yang dipilih
        if (index >= 0 && index < highlightObjects.Count)
        {
            highlightObjects[index].SetActive(true);
            currentSelectedIndex = index;
            
            // CATATAN: Di sinilah Anda bisa menambahkan fungsi
            // untuk mengubah teks di 'PetDescription'
            // Contoh: UpdateDescription(index);
        }
    }

    // Fungsi bantuan untuk mematikan semua highlight
    public void DeselectAll()
    {
        foreach (GameObject highlight in highlightObjects)
        {
            if (highlight != null)
            {
                highlight.SetActive(false);
            }
        }
        currentSelectedIndex = -1;
    }
}