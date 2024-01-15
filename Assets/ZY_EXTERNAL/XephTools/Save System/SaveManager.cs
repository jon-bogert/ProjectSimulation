using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;

namespace XephTools
{
    public enum DataType { Int, Float, Bool, Char, String, Vector2, Vector3, Quaternion, Color }
    public enum FileType { Text, Binary, EncryptedBinary }

    public class SaveData
    {
        public string id;
        public List<Tuple<DataType, string, object>> data = new List<Tuple<DataType, string, object>>();

        public object Find(string fieldName)
        {
            Tuple<DataType, string, object> found = data.Find(i => i.Item2 == fieldName);
            if (found == null) { Debug.Log("was Null");  return null; }
            return found.Item3;
        }

        public int FindIndex(string fieldName)
        {
            return data.FindIndex(i => i.Item2 == fieldName);
        }

        public bool Exists (string fieldName)
        {
            return data.FindIndex(i => i.Item2 == fieldName) >= 0;
        }

        public void TrySet<T>(string fieldName, ref T fieldData)
        {
            if (Exists(fieldName))
            {
                try
                {
                    fieldData = (T)Find(fieldName);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }

        public void AddOrChange(DataType type, string fieldName, object fieldData)
        {
            Tuple<DataType, string, object> entry = new Tuple<DataType, string, object>(type, fieldName, fieldData);
            int index = FindIndex(fieldName);
            if (index >= 0) // Found field
            {
                if (type != data[index].Item1) Debug.LogWarning("SaveData -> DataType not matching for field: " + fieldName);
                data[index] = entry;
                return;
            }
            data.Add(entry);
        }

        public bool Remove(string fieldName)
        {
            int index = FindIndex(fieldName);
            if (index >= 0) // Found field
            {
                data.RemoveAt(index);
                return true;
            }
            Debug.LogWarning("SaveData -> Remove data could not be found with name: " + fieldName);
            return false;
        }
    }

    public class SaveManager : MonoBehaviour
    {
        [SerializeField] FileType _fileType;
        [SerializeField] string _fileName;
        [SerializeField] bool _skipLoad;
        [SerializeField] bool _usingSlots = false;
        [SerializeField] ushort _slot = 0;

        byte[] _key = { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF }; // NOTE: DO NOT SERIALIZE KEY (or it will be accessable to users)
        byte[] _iv = { 0x01, 0x12, 0x23, 0x34, 0x45, 0x56, 0x67, 0x78, 0x89, 0x9A, 0xAB, 0xBC, 0xCD, 0xDE, 0xEF, 0xF0 }; // NOTE: DO NOT SERIALIZE KEY (or it will be accessable to users)
        string _path;
        List<Tuple<ISavable, SaveData>> _data = new List<Tuple<ISavable, SaveData>>();

        //========================================================
        //                 UNITY CALLS
        //========================================================

        private void Awake()
        {
            if (_fileName == "") Debug.LogError("SaveManager -> file name is empty");

            _path = Application.persistentDataPath + "/saves/";

            if (_usingSlots)
                _path += "slot" + _slot.ToString() + "/";

            ReadData();
        }

        //========================================================
        //                 FILE ABSTRACT
        //========================================================

        private void ReadData()
        {
            switch (_fileType)
            {
                case FileType.Text:
                    ReadText();
                    break;
                case FileType.Binary:
                    ReadBinary();
                    break;
                case FileType.EncryptedBinary:
                    ReadEncrypted();
                    break;
                default:
                    Debug.LogError("SaveManager -> FileType not added");
                    break;
            }
        }

        private void WriteData()
        {
            if (!Directory.Exists(_path)) Directory.CreateDirectory(_path);
            switch (_fileType)
            {
                case FileType.Text:
                    WriteText();
                    break;
                case FileType.Binary:
                    WriteBinary();
                    break;
                case FileType.EncryptedBinary:
                    WriteEncrypted();
                    break;
                default:
                    Debug.LogError("SaveManager -> FileType not added");
                    break;
            }
        }

        //========================================================
        //               WORKING WITH ISAVABLE
        //========================================================

        public bool Register(ISavable obj)
        {
            int index = _data.FindIndex(i => i.Item2.id == obj.SaveID);
            if (index != -1)
            {
                if (_data[index].Item1 != null)
                {
                    string name1 = ((MonoBehaviour)_data[index].Item1).name;
                    string name2 = ((MonoBehaviour)obj).name;
                    Debug.LogError("SaveManager -> Register: Duplicate id \"" + obj.SaveID + "\" between objects \"" + name1 + "\" & \"" + name2 + "\"");
                    return false;
                }
                _data[index] = new Tuple<ISavable, SaveData>(obj, _data[index].Item2);
                if (!Debug.isDebugBuild || !_skipLoad) obj.Load(_data[index].Item2);
                return true;
            }
            if (obj.SaveID == "")
            {
                Debug.LogWarning("SaveManager -> Register: Cannot create SaveData with empty ID");
                return false;
            }

            Tuple<ISavable, SaveData> entry = new Tuple<ISavable, SaveData>(obj, new SaveData());
            entry.Item2.id = obj.SaveID;
            obj.Save(entry.Item2);
            _data.Add(entry);
            return true;
        }
        public bool Unregister(ISavable obj)
        {
            int index = _data.FindIndex(i => i.Item2.id == obj.SaveID);
            if (index != -1)
            {
                if (_data[index] == null)
                {
                    Debug.Log("SaveManager -> Unregister: Savable object already null: " + obj.SaveID);
                    return false;
                }
                _data[index] = new Tuple<ISavable, SaveData>(null, _data[index].Item2);
                return true;
            }
            Debug.LogWarning("SaveManager -> Unregister: Could not find item with id: " + obj.SaveID);
            return false;
        }

        public void ReReadFile()
        {
            _data.Clear();
            ReadData();
        }
        public void Load()
        {
            for (int i = 0; i < _data.Count; ++i)
            {
                if (_data[i].Item1 != null)
                {
                    _data[i].Item1.Load(_data[i].Item2);
                }
            }
        }

        public void Save()
        {
            for (int i = 0; i < _data.Count; ++i)
            {
                if (_data[i].Item1 != null)
                {
                    _data[i].Item1.Save(_data[i].Item2);
                }
            }
            WriteData();
        }

        //========================================================
        //                 FORMAT SPECIFIC
        //========================================================

        string ValueToString(Tuple<DataType, string, object> data)
        {
            switch (data.Item1)
            {
                case DataType.Int:
                    return data.Item3.ToString();
                case DataType.Float:
                    return data.Item3.ToString();
                case DataType.Bool:
                    return data.Item3.ToString();
                case DataType.Char:
                    return data.Item3.ToString();
                case DataType.String:
                    return (string)data.Item3;
                case DataType.Vector2:
                    Vector2 v2 = (Vector2)data.Item3;
                    return v2.x.ToString() + ", " + v2.y.ToString();
                case DataType.Vector3:
                    Vector3 v3 = (Vector3)data.Item3;
                    return v3.x.ToString() + ", " + v3.y.ToString() + ", " + v3.z.ToString();
                case DataType.Quaternion:
                case DataType.Color:
                    Quaternion q = (Quaternion)data.Item3;
                    return q.x.ToString() + ", " + q.y.ToString() + ", " + q.z.ToString() + ", " + q.w.ToString();
                default:
                    Debug.LogError("SaveManager -> ValueToString type not supported");
                    return "";
            }
        }

        string TypeConv(DataType type)
        {
            switch (type)
            {
                case DataType.Int:
                    return "int";
                case DataType.Float:
                    return "float";
                case DataType.Bool:
                    return "bool";
                case DataType.Char:
                    return "char";
                case DataType.String:
                    return "string";
                case DataType.Vector2:
                    return "Vector2";
                case DataType.Vector3:
                    return "Vector3";
                case DataType.Quaternion:
                    return "Quaternion";
                case DataType.Color:
                    return "Color";
                default:
                    Debug.LogError("SaveManager -> TypeConv type not supported");
                    return "";
            }
        }
        DataType TypeConv(string type)
        {
            if (type == "int")
                return DataType.Int;
            else if (type == "float")
                return DataType.Float;
            else if (type == "bool")
                return DataType.Bool;
            else if (type == "char")
                return DataType.Char;
            else if (type == "string")
                return DataType.String;
            else if (type == "Vector2")
                return DataType.Vector2;
            else if (type == "Vector3")
                return DataType.Vector3;
            else if (type == "Quaternion")
                return DataType.Quaternion;
            else if (type == "Color")
                return DataType.Color;

            Debug.LogError("SaveManager -> TypeConv: \"" + type + "\" not recognized as valid data type, returning int");
            return DataType.Int;
        }

        void WriteText()
        {
            if (!File.Exists(_path + _fileName)) File.Create(_path + _fileName).Close();
            using (FileStream fstream = new FileStream(_path + _fileName, FileMode.Truncate))
            {
                using (StreamWriter writer = new StreamWriter(fstream))
                {
                    foreach (var d in _data)
                    {
                        writer.WriteLine(d.Item2.id + ":");
                        foreach (var field in d.Item2.data)
                        {
                            string val = ValueToString(field);
                            writer.WriteLine("  " + TypeConv(field.Item1) + " " + field.Item2 + " = " + val);
                        }
                    }
                }
            }
        }
        void ReadText()
        {
            if(!File.Exists(_path + _fileName)) return;
            int currDataIndex = -1;

            using (FileStream fstream = new FileStream(_path + _fileName, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(fstream))
                {
                    while(reader.Peek() != -1)
                    {
                        string line = reader.ReadLine();
                        if (line.Length < 2) continue;
                        if (line.Substring(0, 2) == "  ") // Value
                        {
                            if (currDataIndex == -1)
                            {
                                Debug.LogError("Error reading save file, empty Object ID");
                                continue;
                            }

                            DataType type = DataType.Int;
                            string tag = "";
                            object data = null;

                            line = line.Substring(2);
                            for (int i = 0; i < line.Length; ++i) // Find Type
                            {
                                if (line[i] == ' ')
                                {
                                    type = TypeConv(line.Substring(0, i));
                                    line = line.Substring(i + 1);
                                    break;
                                }
                            }
                            for (int i = 0; i < line.Length; ++i) // Find tag
                            {
                                if (line[i] == ' ')
                                {
                                    tag = line.Substring(0, i);
                                    line = line.Substring(i + 1);
                                    break;
                                }
                            }
                            //line.Replace(" ", ""); // remove remaining spaces
                            line = line.Replace("=", "");// remove = sign
                            while (line[0] == ' ') line = line.Remove(0, 1);// remove leading spaces
                            switch(type)
                            {
                                case DataType.Int:
                                    data = int.Parse(line);
                                    break;
                                case DataType.Float:
                                    data = float.Parse(line);
                                    break;
                                case DataType.Bool:
                                    data = bool.Parse(line);
                                    break;
                                case DataType.Char:
                                    data = char.Parse(line);
                                    break;
                                case DataType.String:
                                    data = line;
                                    break;
                                case DataType.Vector2:
                                    Match matchV2 = Regex.Match(line, @"\b-?\d+(\.\d+)?(,\s*-?\d+(\.\d+)?){2}\b");
                                    string[] floatsV2 = matchV2.Value.Split(',');
                                    Vector2 v2 = new Vector2();
                                    v2.x = float.Parse(floatsV2[0]);
                                    v2.y = float.Parse(floatsV2[1]);
                                    data = v2;
                                    break;
                                case DataType.Vector3:
                                    Match matchV3 = Regex.Match(line, @"\b-?\d+(\.\d+)?(,\s*-?\d+(\.\d+)?){2}\b");
                                    string[] floatsV3 = matchV3.Value.Split(',');
                                    Vector3 v3 = new Vector3();
                                    v3.x = float.Parse(floatsV3[0]);
                                    v3.y = float.Parse(floatsV3[1]);
                                    v3.z = float.Parse(floatsV3[2]);
                                    data = v3;
                                    break;
                                case DataType.Quaternion:
                                    Match matchQ = Regex.Match(line, @"\b-?\d+(\.\d+)?(,\s*-?\d+(\.\d+)?){2}\b");
                                    string[] floatsQ = matchQ.Value.Split(',');
                                    Quaternion q = new Quaternion();
                                    q.x = float.Parse(floatsQ[0]);
                                    q.y = float.Parse(floatsQ[1]);
                                    q.z = float.Parse(floatsQ[2]);
                                    q.w = float.Parse(floatsQ[3]);
                                    data = q;
                                    break;
                                case DataType.Color:
                                    Match matchC = Regex.Match(line, @"\b\d+(\.\d+)?(,\s*\d+(\.\d+)?){2}\b");
                                    string[] floatsC = matchC.Value.Split(',');
                                    Color c = new Color();
                                    c.r = float.Parse(floatsC[0]);
                                    c.g = float.Parse(floatsC[1]);
                                    c.b = float.Parse(floatsC[2]);
                                    c.a = float.Parse(floatsC[3]);
                                    data = c;
                                    break;
                            }
                            _data[currDataIndex].Item2.AddOrChange(type, tag, data);
                            continue;
                        }

                        //Else -> new object
                        for (int i = 0; i < line.Length; ++i)
                        {
                            if (line[i] == ':')
                            {
                                currDataIndex = _data.FindIndex(d => d.Item2.id == line.Substring(0, i));
                                if (currDataIndex == -1) // there was no field with that tag
                                {
                                    SaveData saveData = new SaveData();
                                    saveData.id = line.Substring(0, i);
                                    _data.Add(new Tuple<ISavable, SaveData>(null, saveData));
                                    currDataIndex = _data.Count - 1;
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }
        void WriteBinary()
        {
            if (!File.Exists(_path + _fileName)) File.Create(_path + _fileName).Close();
            using (FileStream fstream = new FileStream(_path + _fileName, FileMode.Truncate))
            {
                using (BinaryWriter writer = new BinaryWriter(fstream))
                {
                    foreach (var d in _data)
                    {
                        writer.Write(byte.MaxValue);// Signal for new Object
                        writer.Write(d.Item2.id);
                        foreach (var field in d.Item2.data)
                        {
                            writer.Write((byte)field.Item1); // type
                            writer.Write(field.Item2);      // name
                            switch (field.Item1)
                            {
                                case DataType.Int:
                                    writer.Write((int)field.Item3);
                                    break;
                                case DataType.Float:
                                    writer.Write((float)field.Item3);
                                    break;
                                case DataType.Bool:
                                    writer.Write((bool)field.Item3);
                                    break;
                                case DataType.Char:
                                    writer.Write((char)field.Item3);
                                    break;
                                case DataType.String:
                                    writer.Write((string)field.Item3);
                                    break;
                                case DataType.Vector2:
                                    Vector2 v2 = (Vector2)field.Item3;
                                    writer.Write(v2.x);
                                    writer.Write(v2.y);
                                    break;
                                case DataType.Vector3:
                                    Vector3 v3 = (Vector3)field.Item3;
                                    writer.Write(v3.x);
                                    writer.Write(v3.y);
                                    writer.Write(v3.z);
                                    break;
                                case DataType.Quaternion:
                                    Quaternion q = (Quaternion)field.Item3;
                                    writer.Write(q.x);
                                    writer.Write(q.y);
                                    writer.Write(q.z);
                                    writer.Write(q.w);
                                    break;
                                case DataType.Color:
                                    Color c = (Color)field.Item3;
                                    writer.Write(c.r);
                                    writer.Write(c.g);
                                    writer.Write(c.b);
                                    writer.Write(c.a);
                                    break;
                            }
                        }
                    }
                }
            }
        }
        void ReadBinary()
        {
            if (!File.Exists(_path + _fileName)) return;
            int currDataIndex = -1;

            using (FileStream fstream = new FileStream(_path + _fileName, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(fstream))
                {
                    while (reader.PeekChar() != -1)
                    {

                        byte typeByte = reader.ReadByte();
                        if (typeByte == byte.MaxValue) // New Object
                        {
                            string fieldName = reader.ReadString();
                            currDataIndex = _data.FindIndex(d => d.Item2.id == fieldName);
                            if (currDataIndex == -1) // there was no field with that tag
                            {
                                SaveData saveData = new SaveData();
                                saveData.id = fieldName;
                                _data.Add(new Tuple<ISavable, SaveData>(null, saveData));
                                currDataIndex = _data.Count - 1;
                            }
                            continue;
                        }

                        if (currDataIndex == -1)
                        {
                            Debug.LogError("Error reading save file, empty Object ID");
                            continue;
                        }
                        DataType type;
                        string tag = "";
                        object data = null;

                        type = (DataType)typeByte;
                        tag = reader.ReadString();
                        switch (type)
                        {
                            case DataType.Int:
                                data = reader.ReadInt32();
                                break;
                            case DataType.Float:
                                data = reader.ReadSingle();
                                break;
                            case DataType.Bool:
                                data = reader.ReadBoolean();
                                break;
                            case DataType.Char:
                                data = reader.ReadChar();
                                break;
                            case DataType.String:
                                data = reader.ReadString();
                                break;
                            case DataType.Vector2:
                                Vector2 v2 = new Vector2();
                                v2.x = reader.ReadSingle();
                                v2.y = reader.ReadSingle();
                                data = v2;
                                break;
                            case DataType.Vector3:
                                Vector3 v3 = new Vector3();
                                v3.x = reader.ReadSingle();
                                v3.y = reader.ReadSingle();
                                v3.z = reader.ReadSingle();
                                data = v3;
                                break;
                            case DataType.Quaternion:
                                Quaternion q = new Quaternion();
                                q.x = reader.ReadSingle();
                                q.y = reader.ReadSingle();
                                q.z = reader.ReadSingle();
                                q.w = reader.ReadSingle();
                                data = q;
                                break;
                            case DataType.Color:
                                Color c = new Color();
                                c.r = reader.ReadSingle();
                                c.g = reader.ReadSingle();
                                c.b = reader.ReadSingle();
                                c.a = reader.ReadSingle();
                                data = c;
                                break;
                        }
                        _data[currDataIndex].Item2.AddOrChange(type, tag, data);
                    }
                }
            }
        }
        void WriteEncrypted()
        {
            if (!File.Exists(_path + _fileName)) File.Create(_path + _fileName).Close();
            using (FileStream fstream = new FileStream(_path + _fileName, FileMode.Truncate))
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = _key;
                    aes.IV = _iv;
                    //byte[] iv = aes.IV;
                    //fstream.Write(iv, 0, iv.Length);
                    //aes.Padding = PaddingMode.Zeros;
                    using (CryptoStream cstream = new CryptoStream( fstream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (BinaryWriter writer = new BinaryWriter(cstream))
                        {
                            foreach (var d in _data)
                            {
                                writer.Write(byte.MaxValue);// Signal for new Object
                                writer.Write(d.Item2.id);
                                foreach (var field in d.Item2.data)
                                {
                                    writer.Write((byte)field.Item1); // type
                                    writer.Write(field.Item2);      // name
                                    switch (field.Item1)
                                    {
                                        case DataType.Int:
                                            writer.Write((int)field.Item3);
                                            break;
                                        case DataType.Float:
                                            writer.Write((float)field.Item3);
                                            break;
                                        case DataType.Bool:
                                            writer.Write((bool)field.Item3);
                                            break;
                                        case DataType.Char:
                                            writer.Write((char)field.Item3);
                                            break;
                                        case DataType.String:
                                            writer.Write((string)field.Item3);
                                            break;
                                        case DataType.Vector2:
                                            Vector2 v2 = (Vector2)field.Item3;
                                            writer.Write(v2.x);
                                            writer.Write(v2.y);
                                            break;
                                        case DataType.Vector3:
                                            Vector3 v3 = (Vector3)field.Item3;
                                            writer.Write(v3.x);
                                            writer.Write(v3.y);
                                            writer.Write(v3.z);
                                            break;
                                        case DataType.Quaternion:
                                            Quaternion q = (Quaternion)field.Item3;
                                            writer.Write(q.x);
                                            writer.Write(q.y);
                                            writer.Write(q.z);
                                            writer.Write(q.w);
                                            break;
                                        case DataType.Color:
                                            Color c = (Color)field.Item3;
                                            writer.Write(c.r);
                                            writer.Write(c.g);
                                            writer.Write(c.b);
                                            writer.Write(c.a);
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        void ReadEncrypted()
        {
            if (!File.Exists(_path + _fileName)) return;
            int currDataIndex = -1;

            using (FileStream fstream = new FileStream(_path + _fileName, FileMode.Open))
            {
                using (Aes aes = Aes.Create())
                {
                    using (CryptoStream cstream = new CryptoStream(fstream, aes.CreateDecryptor(_key, _iv), CryptoStreamMode.Read))
                    {
                        using (MemoryStream mstream = new MemoryStream())
                        {
                            cstream.CopyTo(mstream);
                            mstream.Seek(0, SeekOrigin.Begin);
                            using (BinaryReader reader = new BinaryReader(mstream))
                            {
                                Debug.Log("BinaryReader created");
                                //while (reader.PeekChar() != -1)
                                while (mstream.Position < mstream.Length)
                                { 
                                    byte typeByte = reader.ReadByte();
                                    if (typeByte == byte.MaxValue) // New Object
                                    {
                                        string fieldName = reader.ReadString();
                                        currDataIndex = _data.FindIndex(d => d.Item2.id == fieldName);
                                        if (currDataIndex == -1) // there was no field with that tag
                                        {
                                            SaveData saveData = new SaveData();
                                            saveData.id = fieldName;
                                            _data.Add(new Tuple<ISavable, SaveData>(null, saveData));
                                            currDataIndex = _data.Count - 1;
                                        }
                                        continue;
                                    }

                                    if (currDataIndex == -1)
                                    {
                                        Debug.LogError("Error reading save file, empty Object ID");
                                        continue;
                                    }
                                    DataType type;
                                    string tag = "";
                                    object data = null;

                                    type = (DataType)typeByte;
                                    tag = reader.ReadString();
                                    switch (type)
                                    {
                                        case DataType.Int:
                                            data = reader.ReadInt32();
                                            break;
                                        case DataType.Float:
                                            data = reader.ReadSingle();
                                            break;
                                        case DataType.Bool:
                                            data = reader.ReadBoolean();
                                            break;
                                        case DataType.Char:
                                            data = reader.ReadChar();
                                            break;
                                        case DataType.String:
                                            data = reader.ReadString();
                                            break;
                                        case DataType.Vector2:
                                            Vector2 v2 = new Vector2();
                                            v2.x = reader.ReadSingle();
                                            v2.y = reader.ReadSingle();
                                            data = v2;
                                            break;
                                        case DataType.Vector3:
                                            Vector3 v3 = new Vector3();
                                            v3.x = reader.ReadSingle();
                                            v3.y = reader.ReadSingle();
                                            v3.z = reader.ReadSingle();
                                            data = v3;
                                            break;
                                        case DataType.Quaternion:
                                            Quaternion q = new Quaternion();
                                            q.x = reader.ReadSingle();
                                            q.y = reader.ReadSingle();
                                            q.z = reader.ReadSingle();
                                            q.w = reader.ReadSingle();
                                            data = q;
                                            break;
                                        case DataType.Color:
                                            Color c = new Color();
                                            c.r = reader.ReadSingle();
                                            c.g = reader.ReadSingle();
                                            c.b = reader.ReadSingle();
                                            c.a = reader.ReadSingle();
                                            data = c;
                                            break;
                                    }
                                    _data[currDataIndex].Item2.AddOrChange(type, tag, data);
                                }
                            }
                        }
                    }
                }
            }
        }
        public bool DeleteFile()
        {
            if (_path == "")
            {
                _path = Application.persistentDataPath + "/saves/";
                if (_usingSlots)
                    _path += "slot" + _slot.ToString() + "/";
            }
            if (File.Exists(_path + _fileName))
            {
                File.Delete(_path + _fileName);
                return true;
            }
            return false;
        }

        public bool FileExists
        {
            get
            {
                if (_path == "")
                {
                    _path = Application.persistentDataPath + "/saves/";
                    if (_usingSlots)
                        _path += "slot" + _slot.ToString() + "/";
                }
                return File.Exists(_path + _fileName);
            }
        }
    }
}
