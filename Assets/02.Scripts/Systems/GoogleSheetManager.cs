using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Globalization;
using UnityEngine.Networking;

namespace Systems
{
    public class GoogleSheetManager : MonoBehaviour
    {
        [Tooltip("true: google sheet, false: local json")]
        [SerializeField] bool isAccessGoogleSheet = true;
        [Tooltip("Google sheet appsscript webapp url")]
        [SerializeField] string googleSheetUrl;
        [Tooltip("Spreadsheet ID. docs.google.com/spreadsheets/d/<이 부분>/edit")]
        [SerializeField] string sheetId;
        [Tooltip("Google sheet avail sheet tabs. seperate `/`. For example `Sheet1/Sheet2`")]
        [SerializeField] string availSheets = "Sheet1/Sheet2";
        [Tooltip("For example `/GenerateGoogleSheet`")]
        [SerializeField] string generateFolderPath = "/GenerateGoogleSheet";
        [Tooltip("SO 클래스·에셋 이름. 예: DialogueSO -> DialogueSO.asset, DialogueClass.cs, DialogueJson.json")]
        [SerializeField] string soName = "GoogleSheetSO";
        [Tooltip("You must approach through `GoogleSheetManager.SO<SO이름>()`")]
        public ScriptableObject googleSheetSO;

        // soName 끝의 "SO"를 뺀 이름을 파일 접두어로 쓴다 (GoogleSheetSO -> GoogleSheetClass.cs, 기존 파일명 유지)
        string SOName => string.IsNullOrWhiteSpace(soName) ? "GoogleSheetSO" : soName.Trim();
        string FilePrefix => SOName.Length > 2 && SOName.EndsWith("SO") ? SOName.Substring(0, SOName.Length - 2) : SOName;
        string JsonPath => $"{Application.dataPath}{generateFolderPath}/{FilePrefix}Json.json";
        string ClassPath => $"{Application.dataPath}{generateFolderPath}/{FilePrefix}Class.cs";
        string SOPath => $"Assets{generateFolderPath}/{SOName}.asset";

        string[] availSheetArray;
        string json;
        static GoogleSheetManager instance;

        // 스크립트 재컴파일(도메인 리로드) 후 SO 생성을 이어서 하기 위한 플래그
        // 값: 재컴파일 후 SO를 만들어야 하는 매니저의 soName
        const string PendingSOKey = "Systems.GoogleSheetManager.PendingSOName";



        // 매니저가 여러 개(시트마다 다른 SO)여도 T 타입 SO를 가진 매니저를 찾아 돌려준다
        public static T SO<T>() where T : ScriptableObject
        {
            var managers = FindObjectsByType<GoogleSheetManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var manager in managers)
            {
                if (manager.googleSheetSO is T so)
                    return so;
            }

            Debug.Log($"{typeof(T).Name}를 가진 GoogleSheetManager가 없습니다.");
            return null;
        }

        // "text"는 string과 같지만 인스펙터에서 [TextArea]로 표시된다
        static readonly Dictionary<string, Type> TypeMap = new()
        {
            ["int"] = typeof(int),
            ["float"] = typeof(float),
            ["bool"] = typeof(bool),
            ["string"] = typeof(string),
            ["text"] = typeof(string),
        };

        // "Health:int" -> ("Health", "int"). 타입 생략 시 string
        static (string name, string type) ParseHeader(string header)
        {
            int i = header.IndexOf(':');
            if (i < 0) return (header.Trim(), "string");
            return (header.Substring(0, i).Trim(), header.Substring(i + 1).Trim().ToLowerInvariant());
        }

        static object ConvertCell(JToken token, string type, string where)
        {
            string s = token is JValue v && v.Value != null
                ? Convert.ToString(v.Value, CultureInfo.InvariantCulture)
                : token.ToString();
            bool empty = string.IsNullOrEmpty(s);

            switch (type)
            {
                case "int":
                    if (empty) return 0;
                    if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)) return i;
                    break;
                case "float":
                    if (empty) return 0f;
                    if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var f)) return f;
                    break;
                case "bool":
                    if (empty) return false;
                    if (bool.TryParse(s, out var b)) return b;
                    break;
                default:
                    return s ?? "";
            }

            Debug.LogError($"{where}: '{s}' 를 {type}으로 변환할 수 없습니다.");
            return Activator.CreateInstance(TypeMap[type]);
        }

    #if UNITY_EDITOR
        [ContextMenu("FetchGoogleSheet")]
        async void FetchGoogleSheet()
        {
            //Init
            availSheetArray = SplitSheets(availSheets);
            UnityEditor.SessionState.EraseString(PendingSOKey);

            if (!IsValidIdentifier(SOName))
            {
                Debug.LogError($"SO 이름 '{SOName}'은 C# 클래스 이름으로 쓸 수 없습니다. 영문/한글/숫자/_만, 숫자로 시작하지 않게 해주세요.");
                return;
            }

            if (isAccessGoogleSheet)
            {
                Debug.Log($"Loading from google sheet..");
                if (string.IsNullOrWhiteSpace(googleSheetUrl) || string.IsNullOrWhiteSpace(sheetId))
                {
                    Debug.LogError("googleSheetUrl 또는 sheetId가 비어 있습니다.");
                    return;
                }
                json = await LoadDataGoogleSheet(googleSheetUrl, sheetId.Trim());
            }
            else
            {
                Debug.Log($"Loading from local json..");
                json = LoadDataLocalJson();
            }
            if (json == null) return;

            // availSheets와 받아온 시트 이름이 하나도 안 맞으면 빈 SO를 만들지 않고 중단
            var received = JObject.Parse(json).Properties().Select(x => x.Name).ToList();
            var missing = availSheetArray.Where(x => !received.Contains(x)).ToList();
            if (missing.Count > 0)
                Debug.LogWarning(
                    $"availSheets에 적었지만 응답에 없는 시트: {string.Join(", ", missing)}\n" +
                    $"응답에 들어 있는 시트: {string.Join(", ", received)}");
            if (missing.Count == availSheetArray.Length)
            {
                Debug.LogError("가져올 시트가 하나도 없어 중단합니다. 웹앱 URL·Sheet Id·availSheets를 확인해주세요.");
                return;
            }

            bool isJsonSaved = SaveFileOrSkip(JsonPath, json);
            string allClassCode = GenerateCSharpClass(json);
            bool isClassSaved = SaveFileOrSkip(ClassPath, allClassCode);

            if (isClassSaved)
            {
                // 클래스가 바뀌면 재컴파일이 끝난 뒤(OnScriptsReloaded) SO를 만든다
                UnityEditor.SessionState.SetString(PendingSOKey, SOName);
                UnityEditor.AssetDatabase.Refresh();
                return;
            }

            // 데이터만 바뀌었거나 바뀐 게 없으면 바로 SO를 만든다
            if (isJsonSaved)
                UnityEditor.AssetDatabase.Refresh();

            if (CreateGoogleSheetSO())
                Debug.Log($"Fetch done.");
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnScriptsReloaded()
        {
            string pendingName = UnityEditor.SessionState.GetString(PendingSOKey, "");
            if (string.IsNullOrEmpty(pendingName))
                return;
            UnityEditor.SessionState.EraseString(PendingSOKey);

            var manager = FindObjectsByType<GoogleSheetManager>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(x => x.SOName == pendingName);
            if (manager == null)
            {
                Debug.LogError("GoogleSheetManager를 씬에서 찾을 수 없어 SO를 만들지 못했습니다. 다시 Fetch 해주세요.");
                return;
            }

            if (manager.CreateGoogleSheetSO())
                Debug.Log($"Fetch done.");
        }

        async Task<string> LoadDataGoogleSheet(string url, string id)
        {
            url = url.Trim();
            string separator = url.Contains("?") ? "&" : "?";
            string requestUrl = url + separator
                + "id=" + UnityWebRequest.EscapeURL(id)
                + "&fetchNonce=" + Guid.NewGuid().ToString("N");

            using (var request = UnityWebRequest.Get(requestUrl))
            {
                request.timeout = 120;

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(
                        $"Google Sheet 다운로드 실패 (UnityWebRequest)\n" +
                        $"HTTP: {request.responseCode}\n" +
                        $"오류: {request.error}");

                    return null;
                }

                string body = request.downloadHandler.text;

                // HTTP 200이어도 로그인 페이지 등이 반환됐는지 확인
                JObject parsed;
                try
                {
                    parsed = JObject.Parse(body);
                }
                catch (Newtonsoft.Json.JsonException)
                {
                    Debug.LogError(
                        "Google Sheet 응답이 JSON 객체가 아닙니다.\n" +
                        $"Content-Type: {request.GetResponseHeader("Content-Type")}");

                    return null;
                }

                // Apps Script가 { "error": "..." } 를 돌려주면 아무것도 생성하지 않고 중단
                if (parsed.TryGetValue("error", out JToken error))
                {
                    Debug.LogError($"Google Sheet 오류 (Apps Script): {error}");
                    return null;
                }

                Debug.Log("Google Sheet 다운로드 성공 (UnityWebRequest)");
                return body;
            }
        }

        string LoadDataLocalJson()
        {
            if (File.Exists(JsonPath))
            {
                return File.ReadAllText(JsonPath);
            }

            Debug.Log($"File not exist.\n{JsonPath}");
            return null;
        }

        bool SaveFileOrSkip(string path, string contents)
        {
            string directoryPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (File.Exists(path) && File.ReadAllText(path).Equals(contents))
                return false;

            File.WriteAllText(path, contents);
            return true;
        }

        static bool IsValidIdentifier(string name)
        {
            return name.Length > 0 && !char.IsDigit(name[0]) && name.All(c => char.IsLetterOrDigit(c) || c == '_');
        }

        static string[] SplitSheets(string sheets)
        {
            return sheets.Split('/').Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
        }

        bool IsExistAvailSheets(string sheetName)
        {
            return Array.Exists(availSheetArray, x => x == sheetName);
        }

        // 클래스와 SO 리스트를 만들 시트: availSheets에 있고, 행이 1개 이상인 배열
        List<(string name, JArray items)> GetTargetSheets(JObject jsonObject)
        {
            var result = new List<(string, JArray)>();
            foreach (var sheet in jsonObject)
            {
                if (!IsExistAvailSheets(sheet.Key))
                    continue;

                if (sheet.Value is not JArray items || items.Count == 0 || items[0] is not JObject)
                {
                    Debug.LogWarning($"[{sheet.Key}] 시트가 비어 있거나 형식이 달라 건너뜁니다.");
                    continue;
                }

                result.Add((sheet.Key, items));
            }
            return result;
        }

        string GenerateCSharpClass(string jsonInput)
        {
            JObject jsonObject = JObject.Parse(jsonInput);
            var sheets = GetTargetSheets(jsonObject);
            StringBuilder classCode = new();

            // Scriptable Object
            classCode.AppendLine("using System;\nusing System.Collections.Generic;\nusing UnityEngine;\n");
            classCode.AppendLine($"/// <summary>You must approach through `GoogleSheetManager.SO<{SOName}>()`</summary>");
            classCode.AppendLine($"public class {SOName} : ScriptableObject\n{{");

            foreach (var (className, _) in sheets)
                classCode.AppendLine($"\tpublic List<{className}> {className}List;");

            classCode.AppendLine("}\n");

            // Class
            foreach (var (className, items) in sheets)
            {
                classCode.AppendLine($"[Serializable]\npublic class {className}\n{{");

                foreach (var property in ((JObject)items[0]).Properties())
                {
                    var (name, type) = ParseHeader(property.Name);
                    if (string.IsNullOrEmpty(name)) continue;   // 헤더가 빈 열 무시
                    if (!TypeMap.ContainsKey(type))
                    {
                        Debug.LogError($"[{className}] 알 수 없는 타입 '{type}' (헤더: {property.Name}), string으로 처리합니다.");
                        type = "string";
                    }

                    if (type == "text")
                        classCode.AppendLine($"\t[TextArea(3, 10)] public string {name};");
                    else
                        classCode.AppendLine($"\tpublic {type} {name};");
                }
                classCode.AppendLine("}\n");
            }

            return classCode.ToString();
        }

        bool CreateGoogleSheetSO()
        {
            if (json == null)
                json = LoadDataLocalJson();
            if (json == null)
                return false;
            if (availSheetArray == null)
                availSheetArray = SplitSheets(availSheets);

            Type soType = Type.GetType(SOName);
            if (soType == null || !typeof(ScriptableObject).IsAssignableFrom(soType))
            {
                Debug.LogError($"{SOName} 클래스를 찾을 수 없습니다. 생성된 코드에 컴파일 에러가 없는지 확인해주세요.");
                return false;
            }

            ScriptableObject newSO = ScriptableObject.CreateInstance(soType);
            JObject jsonObject = JObject.Parse(json);

            // 시트 하나가 실패해도 나머지 시트는 계속 채운다
            foreach (var (className, items) in GetTargetSheets(jsonObject))
            {
                try
                {
                    Type classType = soType.Assembly.GetType(className);
                    FieldInfo listField = soType.GetField($"{className}List");
                    if (classType == null || listField == null)
                    {
                        Debug.LogError($"[{className}] 생성된 클래스를 찾을 수 없어 건너뜁니다. 다시 Fetch 해주세요.");
                        continue;
                    }

                    Type listType = typeof(List<>).MakeGenericType(classType);
                    IList listInst = (IList)Activator.CreateInstance(listType);

                    for (int row = 0; row < items.Count; row++)
                    {
                        if (items[row] is not JObject rowObject) continue;
                        object classInst = Activator.CreateInstance(classType);

                        foreach (var property in rowObject.Properties())
                        {
                            var (name, type) = ParseHeader(property.Name);
                            if (string.IsNullOrEmpty(name)) continue;

                            FieldInfo fieldInfo = classType.GetField(name);
                            if (fieldInfo == null) continue;

                            if (!TypeMap.ContainsKey(type)) type = "string";
                            object value = ConvertCell(property.Value, type, $"{className} {row + 2}행 {name}");
                            fieldInfo.SetValue(classInst, value);
                        }
                        listInst.Add(classInst);
                    }

                    listField.SetValue(newSO, listInst);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[{className}] CreateGoogleSheetSO error: {e.Message}");
                }
            }

            // 기존 에셋이 있으면 내용만 덮어써서 GUID(다른 곳의 참조)를 유지한다
            var existing = UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObject>(SOPath);
            ScriptableObject asset;
            if (existing != null && existing.GetType() == soType)
            {
                UnityEditor.EditorUtility.CopySerialized(newSO, existing);
                existing.name = Path.GetFileNameWithoutExtension(SOPath);   // CopySerialized가 빈 이름까지 복사하므로 복구
                UnityEditor.EditorUtility.SetDirty(existing);
                DestroyImmediate(newSO);
                asset = existing;
            }
            else
            {
                if (existing != null)
                    UnityEditor.AssetDatabase.DeleteAsset(SOPath);
                UnityEditor.AssetDatabase.CreateAsset(newSO, SOPath);
                asset = newSO;
            }
            UnityEditor.AssetDatabase.SaveAssets();

            // 씬/프리팹에 참조가 저장되도록 변경 사항으로 기록
            if (googleSheetSO != asset)
            {
                UnityEditor.Undo.RecordObject(this, "Assign GoogleSheetSO");
                googleSheetSO = asset;
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
                UnityEditor.EditorUtility.SetDirty(this);
            }

            Debug.Log("CreateGoogleSheetSO");
            return true;
        }
    #endif

        static GoogleSheetManager GetInstance()
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GoogleSheetManager>();
            }
            return instance;
        }
    }
}
