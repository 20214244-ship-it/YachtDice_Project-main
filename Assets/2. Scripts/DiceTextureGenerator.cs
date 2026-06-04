using UnityEngine;
using UnityEditor;
using System.IO;

public class DiceTextureGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Dice Textures")]
    public static void GenerateTextures()
    {
        string savePath = "Assets/4. Materials/DiceTextures";
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        int size = 256;
        int dotRadius = 22;
        Color bgColor  = Color.white;
        Color dotColor = new Color(0.15f, 0.15f, 0.15f); // 진한 회색

        // 각 면의 점 위치 정의 (0~1 UV 좌표)
        Vector2[][] dotPositions = new Vector2[][]
        {
            // 1
            new Vector2[] { new Vector2(0.5f, 0.5f) },
            // 2
            new Vector2[] { new Vector2(0.25f, 0.75f), new Vector2(0.75f, 0.25f) },
            // 3
            new Vector2[] { new Vector2(0.25f, 0.75f), new Vector2(0.5f, 0.5f), new Vector2(0.75f, 0.25f) },
            // 4
            new Vector2[] { new Vector2(0.25f, 0.75f), new Vector2(0.75f, 0.75f),
                            new Vector2(0.25f, 0.25f), new Vector2(0.75f, 0.25f) },
            // 5
            new Vector2[] { new Vector2(0.25f, 0.75f), new Vector2(0.75f, 0.75f),
                            new Vector2(0.5f,  0.5f),
                            new Vector2(0.25f, 0.25f), new Vector2(0.75f, 0.25f) },
            // 6
            new Vector2[] { new Vector2(0.25f, 0.8f),  new Vector2(0.75f, 0.8f),
                            new Vector2(0.25f, 0.5f),  new Vector2(0.75f, 0.5f),
                            new Vector2(0.25f, 0.2f),  new Vector2(0.75f, 0.2f) },
        };

        for (int face = 0; face < 6; face++)
        {
            Texture2D tex = new Texture2D(size, size);

            // 배경 흰색
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = bgColor;

            // 점 그리기
            foreach (var dot in dotPositions[face])
            {
                int cx = Mathf.RoundToInt(dot.x * size);
                int cy = Mathf.RoundToInt(dot.y * size);

                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                        if (dist < dotRadius)
                            pixels[y * size + x] = dotColor;
                        else if (dist < dotRadius + 1.5f) // 안티앨리어싱
                        {
                            float t = dist - dotRadius;
                            pixels[y * size + x] = Color.Lerp(dotColor, bgColor, t / 1.5f);
                        }
                    }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            // PNG 저장
            byte[] pngData = tex.EncodeToPNG();
            string filePath = $"{savePath}/dice_{face + 1}.png";
            File.WriteAllBytes(filePath, pngData);
            Debug.Log($"[DiceTexture] 저장됨: {filePath}");
        }

        AssetDatabase.Refresh();
        Debug.Log("[DiceTexture] 주사위 텍스처 6개 생성 완료!");
        EditorUtility.DisplayDialog("완료!", "주사위 텍스처 6개 생성됐어요!\nAssets/4. Materials/DiceTextures/ 확인!", "확인");
    }
}
