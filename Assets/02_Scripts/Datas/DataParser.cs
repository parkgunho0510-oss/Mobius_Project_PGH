using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

/// <summary>
/// A generic data parsing class that handles CSV and Binary data for any type implementing IDataEntity.
/// T must be a class, implement IDataEntity, and have a parameterless constructor.
/// </summary>
public class DataParser<T> where T : class, IDataEntity, new()
{
    /// <summary>
    /// Internal storage for parsed data using ID as a key for quick lookup.
    /// </summary>
    private Dictionary<string, T> _table = new Dictionary<string, T>();

    /// <summary>
    /// Public access to the parsed data table.
    /// </summary>
    public Dictionary<string, T> Table => _table;

    /// <summary>
    /// A helper method that converts string data into various types (int, float, bool, Enum).
    /// Uses reflection-safe type checking.
    /// </summary>
    /// <param name="type">The target field type.</param>
    /// <param name="value">The string value to convert.</param>
    /// <returns>An object cast to the target type.</returns>

    // 문자열을 실제 변수 타입(int, float, Enum 등)으로 바꿔주는 변환기
    private object ConvertValue(Type type, string value)
    {
        // Handle empty values by creating a default instance of the type
        if (string.IsNullOrEmpty(value))
            return type.IsValueType ? Activator.CreateInstance(type) : null;

        // --- For Handling <List> Logic ---
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            Type elementType = type.GetGenericArguments()[0]; // List Type (Ex: int, string)

            // Creat list instance
            var list = (System.Collections.IList)Activator.CreateInstance(type);
            if (string.IsNullOrWhiteSpace(value))
                return list;

            string[] splitValues = value.Split(';'); // Split by ';'

            foreach (var val in splitValues)
            {
                // 리스트의 각 요소들을 재귀적으로 변환하여 추가
                string trimmedValue = val.Trim();

                if (string.IsNullOrEmpty(trimmedValue))
                    continue;

                list.Add(ConvertValue(elementType, trimmedValue));
            }
            return list;
        }
        // -------------------------
        if (string.IsNullOrEmpty(value))
            return type.IsValueType ? Activator.CreateInstance(type) : null;

        if (type == typeof(int)) return int.Parse(value);
        if (type == typeof(float)) return float.Parse(value);
        if (type == typeof(bool)) return bool.Parse(value);
        if (type.IsEnum) return Enum.Parse(type, value); 

        // Unity Asset (Ex: GameObject, AnimatorController, Sprite)
        if (typeof(UnityEngine.Object).IsAssignableFrom(type))
        {
            // CSV : "Prefabs/Hero" path.
            return Resources.Load(value, type);
        }

        return value;
    }

    /// <summary>
    /// Loads data from a CSV file located in the 'Resources/Excel' folder.
    /// Parses headers and automatically matches them to class fields using Reflection.
    /// </summary>
    /// <param name="fileName">The name of the CSV file (without extension).</param>
    public void LoadFromCsv(string fileName)
    {
        // Load the CSV file from the Resources folder
        TextAsset csvFile = Resources.Load<TextAsset>($"Data/{fileName}");
        if (csvFile == null) { Debug.LogError($"Resources/Data/{fileName} 경로에서 CSV를 찾을 수 없습니다."); return; }

        // Split text into lines, removing empty entries for stability
        string[] lines = csvFile.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        // Extract field names from the first line (headers)
        string[] headers = lines[0].Split(','); // 첫 줄은 필드 변수명

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length <= 1) continue;

            // Create a new instance of the data class T
            //T entity = new T();

            // [추가] SO와 일반 클래스 구분 생성
            T entity = typeof(ScriptableObject).IsAssignableFrom(typeof(T))
                ? ScriptableObject.CreateInstance(typeof(T)) as T
                : new T();

            for (int j = 0; j < headers.Length; j++)
            {
                if (j >= values.Length) break;

                // Match CSV column name with Class field name
                string fieldName = headers[j].Trim();

                // 상속 및 비공개 필드까지 검색
                FieldInfo field = typeof(T).GetField(fieldName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (field != null)
                {
                    // Clean the string value and convert it to the appropriate field type
                    string rawValue = values[j].Trim();
                    rawValue = rawValue.Replace("\\n", "\n");
                    field.SetValue(entity, ConvertValue(field.FieldType, rawValue));
                }
            }
            // Add the fully populated entity to the dictionary using its ID
            if (!_table.ContainsKey(entity.ID))
                _table.Add(entity.ID, entity);
        }
    }

    /// <summary>
    /// Serializes the current memory table into a binary file (.bin).
    /// Used during development to create optimized data files for production.
    /// </summary>
    /// <param name="fileName">The desired name for the binary file.</param>
    public void SaveToBinary(string fileName)
    {
        // Define path within StreamingAssets (included in the final build)
        string path = Path.Combine(Application.streamingAssetsPath, fileName + ".bin");

        // Ensure the directory exists before writing
        if (!Directory.Exists(Application.streamingAssetsPath))
            Directory.CreateDirectory(Application.streamingAssetsPath);

        // Open a stream to write binary data
        using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
        {
            // Record the total count of items first to know how many to read later
            writer.Write(_table.Count); // 데이터 총 개수 기록
            FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

            foreach (var item in _table.Values)
            {
                foreach (var field in fields)
                {
                    // Serialize every field value as a string for consistent reading
                    // 모든 데이터를 문자열 형태로 직렬화하여 기록
                    writer.Write(field.GetValue(item)?.ToString() ?? "");
                }
            }
        }
        Debug.Log($"{fileName}.bin 바이너리 생성 완료!");
    }

    /// <summary>
    /// Reads optimized binary data back into memory.
    /// This is significantly faster than CSV parsing and should be used in the final game build.
    /// </summary>
    /// <param name="fileName">The name of the binary file.</param>
    public void LoadFromBinary(string fileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName + ".bin");
        if (!File.Exists(path)) { Debug.LogError($"{fileName}.bin 파일이 없습니다."); return; }

        using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
        {
            // Read the total count to control the loop
            int count = reader.ReadInt32();
            // 저장할 때와 동일하게 BindingFlags를 추가해야 순서와 개수가 맞습니다.
            FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

            for (int i = 0; i < count; i++)
            {
                T entity = typeof(ScriptableObject).IsAssignableFrom(typeof(T))
                ? ScriptableObject.CreateInstance(typeof(T)) as T
                 : new T();
                foreach (var field in fields)
                {
                    // Read serialized string and convert it back to the original field type
                    string value = reader.ReadString();
                    field.SetValue(entity, ConvertValue(field.FieldType, value));
                }
                _table[entity.ID] = entity;
            }
        }
    }

    
    /// <summary>
    /// Retrieves a specific data entry by its ID. Returns null if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the data.</param>
    /// <returns>The data instance of type T.</returns>
    public T Get(string id) => _table.GetValueOrDefault(id);
}