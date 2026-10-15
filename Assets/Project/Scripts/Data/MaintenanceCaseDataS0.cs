using System;
using System.Collections.Generic;
using UnityEngine;

namespace MestreDoPC
{
    [CreateAssetMenu(menuName = "MestreDoPC/Maintenance Case")]
    public class MaintenanceCaseDataSO : ScriptableObject
    {
        [Header("Identificação")]
        public string id;
        public string title;
        [TextArea(3, 8)] public string clientComplaint;
        public Difficulty difficulty;

        [Header("Limites")]
        public int budget = 500;
        public float timeLimitSeconds = 300f;

        [Header("PC recebido")]
        public MotherboardDataSO motherboard;
        public CpuDataSO cpu;
        public RamDataSO ram;
        public int ramQuantity = 1;
        public List<StorageDataSO> storage = new List<StorageDataSO>();
        public GpuDataSO gpu;
        public PsuDataSO psu;
        public CoolerDataSO cooler;

        [Header("Defeitos (o primeiro não resolvido define o sintoma do teste)")]
        public List<DefectData> defects = new List<DefectData>();
    }
    
    public enum DefectType { FailedPart, Unseated, Dust }

    [Serializable]
    public class DefectData
    {
        public ComponentCategory category;
        public DefectType type;
        [TextArea] public string symptom;
        [TextArea] public string inspectionHint;
    }

}
