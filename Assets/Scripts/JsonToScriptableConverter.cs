#if UNITY_EDITOR
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class JsonToScriptableConvertor : EditorWindow
{
    private string jsonFIlePath = "";       //Json경로 문자열 값
    private string outputFolder = "Assets/ScriptableObjects/Items"; //출력 SO 파일 경로 값
    private bool createDatabase = true;       //데이터 베이스 활용 여부 체크 값

    [MenuItem("Tools/Json to ScriptableObjects")]
    public static void ShowWIndow()
    {
        GetWindow<JsonToScriptableConvertor>("Json to ScriptableObjects");
    }

     void OnGUI()
    {
        GUILayout.Label("JSON to scriptable objects Converter", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if(GUILayout.Button("Select JSON File"))
        {
            jsonFIlePath = EditorUtility.OpenFilePanel("Select JSON File", "", "json");
        }

        EditorGUILayout.LabelField("Selected File : ", jsonFIlePath);
        EditorGUILayout.Space();

        outputFolder = EditorGUILayout.TextField("Output Folder : ", outputFolder);
        createDatabase = EditorGUILayout.Toggle("Create Database Asset", createDatabase);
        EditorGUILayout.Space();

        if(GUILayout.Button("Convert to ScriptableObjects"))
        {
            if (string.IsNullOrEmpty(jsonFIlePath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a JSON file first!", "OK");
                return;
            }

            ConvertJsonToScriptableObjects();
        }
    }

    private void ConvertJsonToScriptableObjects()       //Json을 ScriptableObject로 변환하는 함수
    {
        //폴더 생성
        if(!Directory.Exists(outputFolder))     //폴더 위치를 확인하고 없으면 폴더 생성
        {
            Directory.CreateDirectory(outputFolder);
        }

        //Json 파일 읽기
        string jsonText = File.ReadAllText(jsonFIlePath);   //Json파일을 읽는다.

        try
        {
            List<ItemData> itemDataList = JsonConvert.DeserializeObject<List<ItemData>>(jsonText);

            List<ItemSO> createdItems = new List<ItemSO>();     //itemSO 리스트 생성

            //각 아이템을 데이터 스크립터블 오브젝트로 변환
            foreach (ItemData itemData in itemDataList)
            {
                ItemSO itemSO = ScriptableObject.CreateInstance<ItemSO>();   //ScriptableObject 인스턴스 생성

                //실제 데이터 복사 시작
                itemSO.id = itemData.id;
                itemSO.itemName = itemData.itemName;
                itemSO.nameEng = itemData.nameEng;
                itemSO.description = itemData.description;

                //열거형 변환
                if(System.Enum.TryParse(itemData.itemTypeString, out ItemType parsedType))
                {
                    itemSO.itemType = parsedType;
                }
                else
                {
                    Debug.LogWarning($"아이템 {itemData.itemName}의 유효하지 않은 타입 : {itemData.itemTypeString}");
                }

                itemSO.price = itemData.price;
                itemSO.power = itemData.power;
                itemSO.level = itemData.level;
                itemSO.isStackable = itemData.isStackable;

                //아이콘 로드(경로가 있는 경우
                if(!string.IsNullOrEmpty(itemData.iconPath))    //아이콘 경로가 있는지 확인
                {
                    itemSO.icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/{itemData.iconPath}");   //아이콘 경로에서 스프라이트 로드
                
                    if(itemSO.icon == null)
                    {
                        Debug.LogWarning($"아이템 { itemData.nameEng}의 아이콘을 로드할 수 없습니다. 경로 : {itemData.iconPath}");
                    }
                }

                //스크립터블오브젝트 저장 - ID를 4자리 숫자로 포맷팅
                string assetPath = $"{outputFolder}/Item_{itemData.id.ToString("D4")}_{itemData.nameEng}.asset";   //저장할 경로와 파일 이름 설정
                AssetDatabase.CreateAsset(itemSO, assetPath);   //스크립터블 오브젝트를 에셋으로 저장

                //에셋 이름 지정
                itemSO.name = $"Item_{itemData.id.ToString("D4")}_{itemData.nameEng}";    //에셋 이름 설정
                createdItems.Add(itemSO);    //생성된 아이템을 리스트에 추가

                EditorUtility.SetDirty(itemSO);   //에셋이 변경되었음을 알림

            }
            //데이터베이스
            if (createDatabase && createdItems.Count > 0)
            {
                ItemDataBaseSO dataBase = ScriptableObject.CreateInstance<ItemDataBaseSO>();
                dataBase.items = createdItems;

                AssetDatabase.CreateAsset(dataBase, $"{outputFolder}/ItemDataBase.asset");
                EditorUtility.SetDirty(dataBase);
            }

            AssetDatabase.SaveAssets();   //에셋 저장
            AssetDatabase.Refresh();      //에셋 데이터베이스 새로고침

            EditorUtility.DisplayDialog("Success", $"Created {createdItems.Count} scriptable objects!", "OK");
        }

        catch(System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to convert Json : {e.Message}", "OK");
            Debug.LogError($"JSON 변환 오류 : {e}");
        }
    }

}

#endif
