#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MestreDoPC.EditorTools
{
    // Utilidades compartilhadas pelos geradores do pacote de missões.
    internal static class PackUtil
    {
        public static Dictionary<string, ComponentDataSO> PartsById()
        {
            var dict = new Dictionary<string, ComponentDataSO>();
            foreach (var guid in AssetDatabase.FindAssets("t:ComponentDataSO"))
            {
                var part = AssetDatabase.LoadAssetAtPath<ComponentDataSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (part != null && !string.IsNullOrEmpty(part.id) && !dict.ContainsKey(part.id))
                    dict[part.id] = part;
            }
            return dict;
        }

        // Usa o primeiro asset do tipo que já existir no projeto; senão cria em "path".
        public static T FindOrCreate<T>(string path) where T : ScriptableObject
        {
            var guid = AssetDatabase.FindAssets("t:" + typeof(T).Name).FirstOrDefault();
            if (guid != null) return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            AssetDatabase.Refresh();
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        // Acrescenta à lista serializada (campo privado) o que ainda não está nela e limpa referências quebradas.
        public static int AddToList(Object owner, string field, IEnumerable<Object> items)
        {
            var so = new SerializedObject(owner);
            var list = so.FindProperty(field);

            for (int i = list.arraySize - 1; i >= 0; i--)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == null)
                    list.DeleteArrayElementAtIndex(i);

            var present = new HashSet<Object>();
            for (int i = 0; i < list.arraySize; i++)
                present.Add(list.GetArrayElementAtIndex(i).objectReferenceValue);

            int added = 0;
            foreach (var item in items)
            {
                if (item == null || !present.Add(item)) continue;
                list.InsertArrayElementAtIndex(list.arraySize);
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = item;
                added++;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(owner);
            return added;
        }
    }

    // Garante as 41 peças do pacote (reaproveita as que já existem, pelo id) e as coloca no ComponentDatabase.
    public static class PartsPackGenerator
    {
        public const string PartsFolder = "Assets/Project/Data/ScriptableObjects/Components";
        public const string DbPath = "Assets/Project/Data/ScriptableObjects/ComponentDatabase.asset";

        [MenuItem("Tools/MestreDoPC/Pacote de missões/1 - Gerar pacote de peças")]
        public static void Generate()
        {
            Directory.CreateDirectory(PartsFolder);
            AssetDatabase.Refresh();

            var existing = PackUtil.PartsById();
            var all = new List<ComponentDataSO>();
            int created = 0;

            // Cria o asset se o id ainda não existir no projeto; senão reaproveita o que já existe.
            void Ensure<T>(string id, string brand, string model, int price, string desc, System.Action<T> fill)
                where T : ComponentDataSO
            {
                if (existing.TryGetValue(id, out var found))
                {
                    all.Add(found);
                    return;
                }

                var asset = ScriptableObject.CreateInstance<T>();
                asset.id = id;
                asset.brand = brand;
                asset.model = model;
                asset.price = price;
                asset.description = desc;
                fill(asset);
                AssetDatabase.CreateAsset(asset, $"{PartsFolder}/{id}.asset");

                existing[id] = asset;
                all.Add(asset);
                created++;
            }

            void AddMb(string id, string brand, string model, int price, SocketType socket, string chipset, RamType ram, int maxGB, string desc)
                => Ensure<MotherboardDataSO>(id, brand, model, price, desc, m =>
                { m.socket = socket; m.chipset = chipset; m.formFactor = FormFactor.MicroATX; m.ramType = ram; m.ramSlots = 2; m.maxRamGB = maxGB; });

            void AddCpu(string id, string brand, string model, int price, SocketType socket, int tdp, int cores, int threads, bool igpu, string desc)
                => Ensure<CpuDataSO>(id, brand, model, price, desc, c =>
                { c.socket = socket; c.tdp = tdp; c.cores = cores; c.threads = threads; c.hasIntegratedGraphics = igpu; });

            void AddRam(string id, string brand, string model, int price, RamType type, int cap, int speed, string desc)
                => Ensure<RamDataSO>(id, brand, model, price, desc, r => { r.type = type; r.capacityGB = cap; r.speedMhz = speed; });

            void AddSto(string id, string brand, string model, int price, StorageType type, int cap, string desc)
                => Ensure<StorageDataSO>(id, brand, model, price, desc, s => { s.type = type; s.capacityGB = cap; });

            void AddGpu(string id, string brand, string model, int price, int tdp, int vram, string desc)
                => Ensure<GpuDataSO>(id, brand, model, price, desc, g => { g.tdp = tdp; g.vramGB = vram; });

            void AddPsu(string id, string brand, string model, int price, int watts, string cert, string desc)
                => Ensure<PsuDataSO>(id, brand, model, price, desc, p => { p.wattage = watts; p.certification = cert; });

            void AddCooler(string id, string brand, string model, int price, CoolerType type, SocketType[] sockets, int rating, string desc)
                => Ensure<CoolerDataSO>(id, brand, model, price, desc, c => { c.type = type; c.supportedSockets = sockets; c.tdpRating = rating; });

            // ---------------- Peças do pacote ----------------
            AddMb("mb_a520mk", "ASUS", "Prime A520M-K", 450, SocketType.AM4, "A520", RamType.DDR4, 64, "Placa-mãe de entrada para processadores AM4 com memória DDR4.");
            AddMb("mb_b550m", "Gigabyte", "B550M DS3H", 620, SocketType.AM4, "B550", RamType.DDR4, 128, "Placa-mãe AM4 mais completa, com bom suporte a processadores topo de linha da plataforma.");
            AddMb("mb_a620m", "ASRock", "A620M-HDV", 650, SocketType.AM5, "A620", RamType.DDR5, 128, "Placa-mãe AM5 de entrada com memória DDR5.");
            AddMb("mb_b650m", "Gigabyte", "B650M", 900, SocketType.AM5, "B650", RamType.DDR5, 128, "Placa-mãe moderna AM5 com memória DDR5.");
            AddMb("mb_h610m", "Gigabyte", "H610M S2H", 480, SocketType.LGA1700, "H610", RamType.DDR4, 64, "Placa-mãe Intel básica (LGA1700) com memória DDR4.");
            AddMb("mb_b760m", "MSI", "PRO B760M-P DDR5", 850, SocketType.LGA1700, "B760", RamType.DDR5, 128, "Placa-mãe Intel (LGA1700) com memória DDR5.");
            AddCpu("cpu_r3_4100", "AMD", "Ryzen 3 4100", 450, SocketType.AM4, 65, 4, 8, false, "Quatro núcleos econômicos para tarefas básicas. Sem vídeo integrado.");
            AddCpu("cpu_r5_5600", "AMD", "Ryzen 5 5600", 800, SocketType.AM4, 65, 6, 12, false, "Seis núcleos para jogos e multitarefa. Não tem vídeo integrado.");
            AddCpu("cpu_r5_5600g", "AMD", "Ryzen 5 5600G", 850, SocketType.AM4, 65, 6, 12, true, "Seis núcleos com vídeo integrado: dispensa placa de vídeo no uso básico.");
            AddCpu("cpu_r7_5700x", "AMD", "Ryzen 7 5700X", 1050, SocketType.AM4, 65, 8, 16, false, "Oito núcleos para criação e multitarefa. Sem vídeo integrado.");
            AddCpu("cpu_r5_7600", "AMD", "Ryzen 5 7600", 1150, SocketType.AM5, 65, 6, 12, true, "Seis núcleos modernos (AM5) com vídeo integrado básico.");
            AddCpu("cpu_r5_8500g", "AMD", "Ryzen 5 8500G", 1100, SocketType.AM5, 65, 6, 12, true, "Seis núcleos com vídeo integrado, ótimo para quem não precisa de placa de vídeo.");
            AddCpu("cpu_r7_7700", "AMD", "Ryzen 7 7700", 1650, SocketType.AM5, 65, 8, 16, true, "Oito núcleos modernos (AM5) com vídeo integrado básico.");
            AddCpu("cpu_i3_12100f", "Intel", "Core i3-12100F", 550, SocketType.LGA1700, 58, 4, 8, false, "Quatro núcleos Intel eficientes, ideais para PCs básicos. Sem vídeo integrado.");
            AddCpu("cpu_i5_12400f", "Intel", "Core i5-12400F", 800, SocketType.LGA1700, 65, 6, 12, false, "Seis núcleos Intel para jogos e trabalho. Sem vídeo integrado.");
            AddCpu("cpu_i7_12700f", "Intel", "Core i7-12700F", 1500, SocketType.LGA1700, 65, 12, 20, false, "Doze núcleos Intel para criação e cargas pesadas. Sem vídeo integrado.");
            AddRam("ram_fury8_ddr4", "Kingston", "Fury 8GB DDR4", 150, RamType.DDR4, 8, 3200, "Módulo de 8 GB DDR4 3200 MHz.");
            AddRam("ram_fury16_ddr4", "Kingston", "Fury 16GB DDR4", 280, RamType.DDR4, 16, 3200, "Módulo de 16 GB DDR4 3200 MHz.");
            AddRam("ram_fury16_ddr5", "Kingston", "Fury 16GB DDR5", 350, RamType.DDR5, 16, 5600, "Módulo de 16 GB DDR5 5600 MHz.");
            AddRam("ram_fury32_ddr5", "Kingston", "Fury 32GB DDR5", 680, RamType.DDR5, 32, 5600, "Módulo de 32 GB DDR5 5600 MHz.");
            AddSto("ssd_wd_240", "WD", "Green SATA 240GB", 140, StorageType.SSD, 240, "SSD SATA de 240 GB: barato e suficiente para o sistema.");
            AddSto("ssd_nv2_500", "Kingston", "NV2 500GB", 250, StorageType.NVMe, 500, "SSD NVMe rápido para o sistema e os programas.");
            AddSto("ssd_nv2_1tb", "Kingston", "NV2 1TB", 420, StorageType.NVMe, 1000, "SSD NVMe de 1 TB para sistema e projetos.");
            AddSto("hdd_barracuda_1tb", "Seagate", "Barracuda 1TB", 260, StorageType.HDD, 1000, "HD de 1 TB: muito espaço por pouco dinheiro, porém mais lento.");
            AddSto("ssd_nv2_2tb", "Kingston", "NV2 2TB", 850, StorageType.NVMe, 2000, "SSD NVMe de 2 TB para quem trabalha com arquivos enormes.");
            AddGpu("gpu_gtx1650", "NVIDIA", "GeForce GTX 1650", 900, 75, 4, "Placa de vídeo básica para jogos leves.");
            AddGpu("gpu_rx6600", "AMD", "Radeon RX 6600", 1450, 132, 8, "Placa de vídeo equilibrada para jogos em Full HD.");
            AddGpu("gpu_rtx3050", "NVIDIA", "GeForce RTX 3050", 1500, 130, 8, "Placa de vídeo de entrada para jogos em Full HD.");
            AddGpu("gpu_rtx4060", "NVIDIA", "GeForce RTX 4060", 2200, 115, 8, "Placa de vídeo eficiente para jogos e edição.");
            AddGpu("gpu_rtx4070", "NVIDIA", "GeForce RTX 4070", 3600, 200, 12, "Placa de vídeo potente, com 12 GB, para 1440p e criação.");
            AddGpu("gpu_rx7800xt", "AMD", "Radeon RX 7800 XT", 4200, 263, 16, "Placa de vídeo topo de linha com 16 GB de VRAM.");
            AddPsu("psu_cv450", "Corsair", "CV450", 280, 450, "80 Plus Bronze", "Fonte de 450 W, para configurações simples.");
            AddPsu("psu_cv550", "Corsair", "CV550", 350, 550, "80 Plus Bronze", "Fonte de 550 W, não modular.");
            AddPsu("psu_a650bn", "MSI", "MAG A650BN", 420, 650, "80 Plus Bronze", "Fonte de 650 W com boa folga para placas de vídeo médias.");
            AddPsu("psu_rm750", "Corsair", "RM750", 700, 750, "80 Plus Gold", "Fonte de 750 W modular, com bastante folga.");
            AddPsu("psu_rm850e", "Corsair", "RM850e", 900, 850, "80 Plus Gold", "Fonte de 850 W modular para configurações exigentes.");
            AddCooler("cooler_wraith", "AMD", "Wraith Stealth", 0, CoolerType.Air, new[] { SocketType.AM4 }, 65, "Cooler que acompanha CPUs AM4. Só aguenta processadores leves.");
            AddCooler("cooler_intel_rm1", "Intel", "Laminar RM1", 0, CoolerType.Air, new[] { SocketType.LGA1700 }, 65, "Cooler original dos processadores Intel LGA1700.");
            AddCooler("cooler_ag400", "DeepCool", "AG400", 160, CoolerType.Air, new[] { SocketType.AM4, SocketType.AM5, SocketType.LGA1700 }, 220, "Cooler a ar eficiente, compatível com AM4, AM5 e LGA1700.");
            AddCooler("cooler_hyper212", "Cooler Master", "Hyper 212", 200, CoolerType.Air, new[] { SocketType.AM4, SocketType.AM5 }, 150, "Cooler a ar para AM4 e AM5.");
            AddCooler("cooler_h100i", "Corsair", "iCUE H100i", 850, CoolerType.Liquid, new[] { SocketType.AM4, SocketType.AM5, SocketType.LGA1700 }, 300, "Water cooler de 240 mm, silencioso e potente.");

            var db = PackUtil.FindOrCreate<ComponentDatabaseSO>(DbPath);
            int added = PackUtil.AddToList(db, "components", all);

            AssetDatabase.SaveAssets();
            Selection.activeObject = db;
            EditorGUIUtility.PingObject(db);
            Debug.Log($"[Pacote] Peças do pacote: {all.Count} (criadas agora: {created}). " +
                      $"Adicionadas ao ComponentDatabase: {added}.");
        }
    }
}
#endif
