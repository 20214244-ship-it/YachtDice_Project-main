using UnityEngine;
using UnityEditor;

public class DiceMeshReplacer : EditorWindow
{
    [MenuItem("Tools/Replace Dice Mesh")]
    public static void ReplaceMesh()
    {
        // Dice_d6 FBX에서 메시 가져오기
        GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Dice_6/Meshes/Dice_d6.fbx");
        if (fbx == null)
        {
            EditorUtility.DisplayDialog("오류", "Assets/Dice_6/Meshes/Dice_d6.fbx 를 찾을 수 없어요!", "확인");
            return;
        }

        // FBX에서 MeshFilter 메시 추출
        MeshFilter[] fbxFilters = fbx.GetComponentsInChildren<MeshFilter>();
        if (fbxFilters.Length == 0)
        {
            EditorUtility.DisplayDialog("오류", "FBX에서 메시를 찾을 수 없어요!", "확인");
            return;
        }
        Mesh diceMesh = fbxFilters[0].sharedMesh;

        // Material 가져오기 (URP용)
        Material diceMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/4. Materials/DiceMat_URP.mat");

        // 씬에서 Dice 오브젝트들 찾아서 메시 교체
        string[] diceNames = { "Dice", "Dice (1)", "Dice (2)", "Dice (3)", "Dice (4)" };
        int count = 0;

        foreach (string diceName in diceNames)
        {
            GameObject diceObj = GameObject.Find(diceName);
            if (diceObj == null) continue;

            // MeshFilter 교체
            MeshFilter mf = diceObj.GetComponent<MeshFilter>();
            if (mf == null) mf = diceObj.AddComponent<MeshFilter>();
            mf.sharedMesh = diceMesh;

            // MeshRenderer Material 교체
            MeshRenderer mr = diceObj.GetComponent<MeshRenderer>();
            if (mr == null) mr = diceObj.AddComponent<MeshRenderer>();
            if (diceMat != null) mr.sharedMaterial = diceMat;

            count++;
        }

        EditorUtility.DisplayDialog("완료!", $"주사위 {count}개 메시 교체 완료!", "확인");
    }
}
