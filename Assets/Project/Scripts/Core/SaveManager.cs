using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MestreDoPC
{
    public class SaveManager : Singleton<SaveManager>
    {
        private const string Key = "mestredopc.save";
        private const int MaxRecordsPerMission = 10;

        public SaveData Data { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return; // duplicata: o Singleton já a destruiu
            Load();
        }

        private void Load()
        {
            if (PlayerPrefs.HasKey(Key))
            {
                try { Data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(Key)); }
                catch (Exception e) { Debug.LogWarning("Save corrompido, começando do zero: " + e.Message); }
            }
            if (Data == null) Data = new SaveData();
        }

        public void Save()
        {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(Data));
            PlayerPrefs.Save(); // no WebGL isso dispara a gravação no IndexedDB
        }

        // Retorna true se a pontuação for a melhor já registrada para essa missão/modo/dificuldade.
        public bool AddRecord(RecordEntry entry)
        {
            Func<RecordEntry, bool> sameKey = r =>
                r.mode == entry.mode && r.difficulty == entry.difficulty && r.missionId == entry.missionId;

            int best = Data.records.Where(sameKey).Select(r => r.score).DefaultIfEmpty(-1).Max();
            bool isNewBest = entry.score > best;

            Data.records.Add(entry);
            foreach (var extra in Data.records.Where(sameKey).OrderByDescending(r => r.score).Skip(MaxRecordsPerMission).ToList())
                Data.records.Remove(extra);

            Save();
            return isNewBest;
        }

        public void MarkMissionCompleted(string missionId)
        {
            if (string.IsNullOrEmpty(missionId) || Data.completedMissionIds.Contains(missionId)) return;
            Data.completedMissionIds.Add(missionId);
            Save();
        }

        // Para a tela de Recordes do outro dev.
        public List<RecordEntry> GetRecords(string mode, string difficulty)
            => Data.records.Where(r => r.mode == mode && r.difficulty == difficulty)
                           .OrderByDescending(r => r.score).ToList();

        [ContextMenu("Debug: logar save")]
        private void DebugLog() => Debug.Log(JsonUtility.ToJson(Data, true));

        [ContextMenu("Debug: apagar save")]
        private void DebugWipe() { Data = new SaveData(); Save(); }
    }
}
