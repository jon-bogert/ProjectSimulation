namespace XephTools
{
    public interface ISavable
    {
        public string SaveID { get; }
        public void Load(SaveData data);
        public void Save(SaveData data);
    }
}
