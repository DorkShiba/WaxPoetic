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
        [Tooltip("Google sheet avail sheet tabs. seperate `/`. For example `Sheet1/Sheet2`")]
        [SerializeField] string availSheets = "Sheet1/Sheet2";
        [Tooltip("For example `/GenerateGoogleSheet`")]
        [SerializeField] string generateFolderPath = "/GenerateGoogleSheet";
        [Tooltip("You must approach through `GoogleSheetManager.SO<GoogleSheetSO>()`")]
        public ScriptableObject googleSheetSO;

        string JsonPath => $"{Application.dataPath}{generateFolderPath}/GoogleSheetJson.json";
        string ClassPath => $"{Application.dataPath}{generateFolderPath}/GoogleSheetClass.cs";
        string SOPath => $"Assets{generateFolderPath}/GoogleSheetSO.asset";

        string[] availSheetArray;
        string json;
        bool refeshTrigger;
        static GoogleSheetManager instance;



        public static T SO<T>() where T : ScriptableObject
        {
            if (GetInstance().googleSheetSO == null)
            {
                Debug.Log($"googleSheetSO is null");
                return null;
            }

            return GetInstance().googleSheetSO as T;
        }

        static readonly Dictionary<string, Type> TypeMap = new()
        {
            ["int"] = typeof(int),
            ["float"] = typeof(float),
            ["bool"] = typeof(bool),
            ["string"] = typeof(string),
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
            availSheetArray = availSheets.Split('/');

            if (isAccessGoogleSheet)
            {
                Debug.Log($"Loading from google sheet..");
                json = await LoadDataGoogleSheet(googleSheetUrl);
            }
            else
            {
                Debug.Log($"Loading from local json..");
                json = LoadDataLocalJson();
            }
            if (json == null) return;

            bool isJsonSaved = SaveFileOrSkip(JsonPath, json);
            string allClassCode = GenerateCSharpClass(json);
            bool isClassSaved = SaveFileOrSkip(ClassPath, allClassCode);

            if (isJsonSaved || isClassSaved)
            {
                refeshTrigger = true;
                UnityEditor.AssetDatabase.Refresh();
            }
            else
            {
                CreateGoogleSheetSO();
                Debug.Log($"Fetch done.");
            }
        }

        async Task<string> LoadDataGoogleSheet(string url)
        {
            string separator = url.Contains("?") ? "&" : "?";
            string requestUrl = url + separator
                + "fetchNonce=" + Guid.NewGuid().ToString("N");

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
                try
                {
                    JObject.Parse(body);
                }
                catch (Newtonsoft.Json.JsonException)
                {
                    Debug.LogError(
                        "Google Sheet 응답이 JSON 객체가 아닙니다.\n" +
                        $"Content-Type: {request.GetResponseHeader("Content-Type")}");

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

        bool IsExistAvailSheets(string sheetName)
        {
            return Array.Exists(availSheetArray, x => x == sheetName);
        }

        string GenerateCSharpClass(string jsonInput)
        {
            JObject jsonObject = JObject.Parse(jsonInput);
            StringBuilder classCode = new();

            // Scriptable Object
            classCode.AppendLine("using System;\nusing System.Collections.Generic;\nusing UnityEngine;\n");
            classCode.AppendLine("/// <summary>You must approach through `GoogleSheetManager.SO<GoogleSheetSO>()`</summary>");
            classCode.AppendLine("public class GoogleSheetSO : ScriptableObject\n{");

            foreach (var sheet in jsonObject)
            {
                string className = sheet.Key;
                if (!IsExistAvailSheets(className))
                    continue;

                classCode.AppendLine($"\tpublic List<{className}> {className}List;");
            }
            classCode.AppendLine("}\n");

            // Class
            foreach (var jObject in jsonObject)
            {
                string className = jObject.Key;

                if (!IsExistAvailSheets(className))
                    continue;

                var items = (JArray)jObject.Value;
                if (items.Count == 0) continue;   // 빈 시트 방어

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
                    classCode.AppendLine($"\tpublic {type} {name};");
                }
                classCode.AppendLine("}\n");
            }

            return classCode.ToString();
        }

        string GetCSharpType(JTokenType jsonType)
        {
            switch (jsonType)
            {
                case JTokenType.Integer:
                    return "int";
                case JTokenType.Float:
                    return "float";
                case JTokenType.Boolean:
                    return "bool";
                default:
                    return "string";
            }
        }

        bool CreateGoogleSheetSO()
        {
            if (Type.GetType("GoogleSheetSO") == null)
                return false;

            googleSheetSO = ScriptableObject.CreateInstance("GoogleSheetSO");
            JObject jsonObject = JObject.Parse(json);
            try
            {
                foreach (var jObject in jsonObject)
                {
                    string className = jObject.Key;
                    if (!IsExistAvailSheets(className))
                        continue;

                    Type classType = Type.GetType(className);
                    Type listType = typeof(List<>).MakeGenericType(classType);
                    IList listInst = (IList)Activator.CreateInstance(listType);
                    var items = (JArray)jObject.Value;

                    for (int row = 0; row < items.Count; row++)
                    {
                        object classInst = Activator.CreateInstance(classType);

                        foreach (var property in ((JObject)items[row]).Properties())
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

                    googleSheetSO.GetType().GetField($"{className}List").SetValue(googleSheetSO, listInst);
                }
            }
            catch (Exception e)
            { 
                Debug.LogError($"CreateGoogleSheetSO error: {e.Message}");
            }
            print("CreateGoogleSheetSO");
            UnityEditor.AssetDatabase.CreateAsset(googleSheetSO, SOPath);
            UnityEditor.AssetDatabase.SaveAssets();
            return true;
        }

        void OnValidate()
        {
            if (refeshTrigger)
            {
                bool isCompleted = CreateGoogleSheetSO();
                if (isCompleted)
                {
                    refeshTrigger = false;
                    Debug.Log($"Fetch done.");
                }
            }
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
