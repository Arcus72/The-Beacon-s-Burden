#if UNITY_EDITOR
using UnityEngine;
using System.IO;

public class IconGenerator : MonoBehaviour
{
    [Header("Przeci�gnij tutaj prefab broni ze sceny")]
    public GameObject targetObject;

    [Header("Nazwa pliku ikony")]
    public string iconName = "Bro�_Ikona";

    [ContextMenu("GENERUJ IKON� Z PRZEZROCZYSTO�CI�")]
    public void GenerateIcon()
    {
        if (targetObject == null)
        {
            Debug.LogError("Musisz przypisa� obiekt w polu Target Object!");
            return;
        }

        // Pobieramy automatyczn� miniaturk�, kt�r� Unity generuje dla edytora
        Texture2D texture = UnityEditor.AssetPreview.GetAssetPreview(targetObject);

        if (texture == null)
        {
            Debug.LogWarning("Unity jeszcze nie wygenerowa�o podgl�du. Spr�buj ponownie za chwil� lub kliknij na prefab w folderze Project.");
            return;
        }

        // Tworzymy now� tekstur�, w kt�rej upewniamy si�, �e t�o b�dzie czyst� przezroczysto�ci� (RGBA32)
        Texture2D alphaTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                Color color = texture.GetPixel(x, y);

                // Trik: Domy�lne miniaturki Unity maj� szare t�o. 
                // Wycinamy je, sprawdzaj�c czy piksel jest idealnie szarym t�em edytora.
                if (color.r > 0.2f && color.r < 0.3f && color.g > 0.2f && color.g < 0.3f && color.b > 0.2f && color.b < 0.3f)
                {
                    alphaTexture.SetPixel(x, y, Color.clear);
                }
                else
                {
                    alphaTexture.SetPixel(x, y, new Color(color.r, color.g, color.b, 1f));
                }
            }
        }
        alphaTexture.Apply();

        // Kodowanie do pliku PNG
        byte[] bytes = alphaTexture.EncodeToPNG();
        string path = Path.Combine(Application.dataPath, iconName + ".png");
        File.WriteAllBytes(path, bytes);

        Debug.Log($"Sukces! Ikona zapisana bez t�a w: {path}");
        UnityEditor.AssetDatabase.Refresh();
    }
}
#endif