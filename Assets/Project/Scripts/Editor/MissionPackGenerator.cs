#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MestreDoPC.EditorTools
{
    // Gera 45 missões (15 por modo: 5 fáceis, 5 médias, 5 difíceis), os catálogos e os perfis da IA.
    // Rode ANTES o menu "1 - Gerar pacote de peças". Não sobrescreve assets que já existem.
    public static class MissionPackGenerator
    {
        private const string Root = "Assets/Project/Data/ScriptableObjects";
        private const string AssemblyFolder = Root + "/Missions/Assembly";
        private const string AIFolder = Root + "/Missions/AIChallenge";
        private const string CasesFolder = Root + "/Maintenance/Cases";
        private const string ProfilesFolder = Root + "/AIProfiles";
        private const string MissionCatalogPath = Root + "/Missions/MissionCatalog.asset";
        private const string MaintenanceCatalogPath = Root + "/Maintenance/MaintenanceCatalog.asset";

        // ---------------------------------------------------------------------------------
        //  Estruturas de dados (só para este gerador)
        // ---------------------------------------------------------------------------------
        private class MissionDef
        {
            public string id, title, brief;
            public Difficulty diff;
            public int budget, time, ram, storage, cores, vram;
        }

        private class DefectDef
        {
            public ComponentCategory cat;
            public DefectType type;
            public string symptom, hint;
        }

        private class CaseDef
        {
            public string id, title, complaint;
            public Difficulty diff;
            public int budget, time, ramQty;
            public string mb, cpu, ram, gpu, psu, cooler;
            public string[] storage;
            public DefectDef[] defects;
        }

        private static MissionDef M(string id, Difficulty diff, string title, string brief,
                                    int budget, int time, int ram, int storage, int cores, int vram)
            => new MissionDef { id = id, diff = diff, title = title, brief = brief, budget = budget, time = time,
                                ram = ram, storage = storage, cores = cores, vram = vram };

        private static DefectDef D(ComponentCategory cat, DefectType type, string symptom, string hint)
            => new DefectDef { cat = cat, type = type, symptom = symptom, hint = hint };

        private static CaseDef C(string id, Difficulty diff, string title, string complaint, int budget, int time,
                                 string mb, string cpu, string ram, int ramQty, string[] storage,
                                 string gpu, string psu, string cooler, DefectDef[] defects)
            => new CaseDef { id = id, diff = diff, title = title, complaint = complaint, budget = budget, time = time,
                             mb = mb, cpu = cpu, ram = ram, ramQty = ramQty, storage = storage,
                             gpu = gpu, psu = psu, cooler = cooler, defects = defects };

        // ---------------------------------------------------------------------------------
        //  DADOS DAS MISSÕES (edite aqui e regenere, se quiser mudar textos ou números)
        // ---------------------------------------------------------------------------------
        private static List<MissionDef> AssemblyMissions()
        {
            var list = new List<MissionDef>();
            list.Add(M("asm_e01", Difficulty.Easy, "Primeiro PC da faculdade", "Marina, caloura de Pedagogia: \"Preciso de um PC para trabalhos, pesquisas e aulas online. Nada de jogos pesados, só confiável e barato.\"", 2600, 0, 8, 240, 4, 0));
            list.Add(M("asm_e02", Difficulty.Easy, "Home office do seu Antônio", "Seu Antônio, contador: \"Vivo com dezenas de abas e planilhas abertas. Preciso de memória de sobra e espaço para os arquivos dos clientes.\"", 3000, 0, 16, 500, 4, 0));
            list.Add(M("asm_e03", Difficulty.Easy, "Acervo da biblioteca", "Dona Rita, bibliotecária: \"Estamos digitalizando livros antigos. O PC fica ligado o dia todo e o espaço para guardar tudo é a prioridade.\"", 2400, 900, 8, 1000, 4, 0));
            list.Add(M("asm_e04", Difficulty.Easy, "Lan house do bairro", "Jaílson, dono de lan house: \"Quero uma máquina para jogos leves de competição. Tem que ter placa de vídeo dedicada, sem exagero.\"", 3400, 900, 16, 500, 4, 4));
            list.Add(M("asm_e05", Difficulty.Easy, "Setup da turma de TI", "Davi, aluno de TI: \"Vou programar, rodar máquinas virtuais e fazer os laboratórios da faculdade. Quero um processador com bons núcleos.\"", 3000, 900, 16, 500, 6, 0));
            list.Add(M("asm_m01", Difficulty.Medium, "Gamer de entrada", "Rafael, 17 anos: \"Quero jogar os lançamentos em Full HD sem gastar uma fortuna. A placa de vídeo é o que mais importa.\"", 4300, 540, 16, 500, 6, 6));
            list.Add(M("asm_m02", Difficulty.Medium, "Streamer iniciante", "Duda, streamer: \"Vou jogar e transmitir ao vivo ao mesmo tempo, e ainda guardar as gravações. Preciso de processador, vídeo e espaço.\"", 4600, 480, 16, 1000, 6, 8));
            list.Add(M("asm_m03", Difficulty.Medium, "Editor freelancer", "Caio, editor de vídeo: \"A linha do tempo trava toda hora. Preciso de muita memória, um bom processador e espaço para os projetos.\"", 5200, 480, 32, 1000, 8, 6));
            list.Add(M("asm_m04", Difficulty.Medium, "Escritório de arquitetura", "Letícia, arquiteta: \"Modelamos projetos em 3D e renderizamos imagens. Precisa de bastante memória e uma placa de vídeo razoável.\"", 4400, 420, 32, 500, 6, 4));
            list.Add(M("asm_m05", Difficulty.Medium, "Laboratório de redes em casa", "Tiago, estudante de redes: \"Quero rodar vários roteadores virtuais ao mesmo tempo para estudar. Memória e núcleos são tudo.\"", 4600, 420, 32, 1000, 8, 0));
            list.Add(M("asm_h01", Difficulty.Hard, "Estação de renderização 3D", "Helena, animadora: \"Renderizo cenas pesadas com texturas enormes. Preciso de muita RAM, 8 núcleos e uma placa de vídeo com 12 GB.\"", 8800, 360, 64, 1000, 8, 12));
            list.Add(M("asm_h02", Difficulty.Hard, "Gamer 1440p sem concessões", "Bruno, jogador competitivo: \"Jogo em 1440p com texturas no máximo. Quero 12 GB de VRAM e nenhum gargalo.\"", 6800, 300, 32, 1000, 6, 12));
            list.Add(M("asm_h03", Difficulty.Hard, "Laboratório de IA", "Prof. Nunes: \"Vou treinar modelos de IA na universidade. Preciso de 64 GB de RAM, 2 TB de espaço e uma GPU com 16 GB.\"", 10000, 360, 64, 2000, 8, 16));
            list.Add(M("asm_h04", Difficulty.Hard, "Ilha de edição 4K", "Valentina, cineasta: \"Edito material em 4K com várias camadas. Quero 12 núcleos, 2 TB para as filmagens e vídeo decente.\"", 6000, 300, 32, 2000, 12, 8));
            list.Add(M("asm_h05", Difficulty.Hard, "Orçamento extremo", "Mariana, caçadora de pechinchas: \"Quero 12 GB de VRAM e 32 GB de RAM gastando o mínimo possível. Cada real conta.\"", 6700, 240, 32, 1000, 8, 12));
            return list;
        }

        private static List<MissionDef> AIMissions()
        {
            var list = new List<MissionDef>();
            list.Add(M("ai_e01", Difficulty.Easy, "Duelo de estreia", "Seu primeiro duelo! A IA Amadora vai montar um PC básico para estudos. Mostre que você é mais rápido e mais esperto.", 2800, 720, 8, 240, 4, 0));
            list.Add(M("ai_e02", Difficulty.Easy, "Desafio da vizinhança", "A vizinhança apostou: quem monta o melhor PC para a família toda, você ou a IA? Memória de sobra e espaço para as fotos.", 3000, 660, 16, 500, 4, 0));
            list.Add(M("ai_e03", Difficulty.Easy, "Café-internet", "O café-internet precisa de uma máquina com vídeo dedicado para jogos leves. Quem entrega a melhor opção?", 3400, 660, 16, 500, 4, 4));
            list.Add(M("ai_e04", Difficulty.Easy, "Orçamento apertado", "Cada real conta! A IA Amadora vai tentar te passar a perna com pouco dinheiro. Oito gigas de RAM bastam.", 2500, 600, 8, 500, 4, 0));
            list.Add(M("ai_e05", Difficulty.Easy, "Amistoso de bancada", "Um amistoso entre técnicos: PC de uso geral com 6 núcleos e um HD grande para guardar tudo.", 3000, 720, 16, 1000, 6, 0));
            list.Add(M("ai_m01", Difficulty.Medium, "Duelo gamer", "A IA Técnica foi treinada em milhares de montagens gamer. Dá para ganhar dela jogando em Full HD?", 4300, 480, 16, 1000, 6, 6));
            list.Add(M("ai_m02", Difficulty.Medium, "Torneio de criadores", "Dois técnicos, um canal de streaming: monte o PC que melhor equilibra processador, vídeo e espaço.", 4600, 480, 16, 1000, 6, 8));
            list.Add(M("ai_m03", Difficulty.Medium, "Disputa de edição", "A IA Técnica garante que edita vídeo em qualquer PC. Prove que o seu é melhor, com 32 GB de RAM e 8 núcleos.", 4500, 450, 32, 1000, 8, 0));
            list.Add(M("ai_m04", Difficulty.Medium, "Final regional de eSports", "Final regional! O PC precisa de 8 núcleos, placa de vídeo com 8 GB e boa quantidade de memória.", 4600, 450, 16, 500, 8, 8));
            list.Add(M("ai_m05", Difficulty.Medium, "Criação 3D iniciante", "Quem monta o melhor PC de entrada para modelagem 3D? Muita memória e um pouco de vídeo.", 4400, 480, 32, 500, 6, 4));
            list.Add(M("ai_h01", Difficulty.Hard, "Final do campeonato", "A IA Mestre nunca perdeu uma final. Uma workstation de 64 GB com GPU de 12 GB decide o título.", 8800, 330, 64, 1000, 8, 12));
            list.Add(M("ai_h02", Difficulty.Hard, "Duelo 1440p", "Quem monta o melhor PC para 1440p? A IA Mestre calcula tudo em segundos.", 6800, 300, 32, 1000, 6, 12));
            list.Add(M("ai_h03", Difficulty.Hard, "O desafio dos 16 GB de VRAM", "Só uma placa de vídeo topo de linha resolve, e o orçamento é justo. A IA Mestre já sabe qual é.", 8200, 330, 32, 2000, 8, 16));
            list.Add(M("ai_h04", Difficulty.Hard, "Edição 4K contra o relógio", "A IA Mestre monta uma ilha de edição 4K em poucos minutos. Será que você consegue ser mais rápido?", 6000, 300, 32, 2000, 12, 8));
            list.Add(M("ai_h05", Difficulty.Hard, "Mestre dos mestres", "A prova final: 64 GB de RAM, 2 TB, 12 núcleos e 12 GB de VRAM dentro do orçamento. Nada de erros.", 9200, 330, 64, 2000, 12, 12));
            return list;
        }

        private static List<CaseDef> MaintenanceCases()
        {
            var list = new List<CaseDef>();
            list.Add(C("mnt_e01", Difficulty.Easy, "Desliga sozinho", "Carlos: \"Meu PC desliga sozinho depois de alguns minutos de jogo. Antes funcionava bem.\"", 300, 300,
                "mb_a520mk", "cpu_r5_5600", "ram_fury8_ddr4", 1, new[] { "ssd_nv2_500" }, "gpu_rtx3050", "psu_cv550", "cooler_hyper212",
                new[] { D(ComponentCategory.Cooler, DefectType.Dust, "Liga normal, mas esquenta muito e desliga sozinho depois de alguns minutos.", "O dissipador e a ventoinha estão cobertos de poeira.") }));
            list.Add(C("mnt_e02", Difficulty.Easy, "Barulho de turbina", "Fernanda: \"O PC faz um barulho de avião quando abro jogos e a imagem começa a travar.\"", 300, 300,
                "mb_h610m", "cpu_i3_12100f", "ram_fury8_ddr4", 1, new[] { "ssd_wd_240" }, "gpu_gtx1650", "psu_cv450", "cooler_intel_rm1",
                new[] { D(ComponentCategory.Gpu, DefectType.Dust, "A placa de vídeo chega a temperaturas altíssimas e a ventoinha gira no máximo.", "As ventoinhas da placa de vídeo estão entupidas de poeira.") }));
            list.Add(C("mnt_e03", Difficulty.Easy, "Apita e não dá imagem", "Seu Joaquim: \"Liguei o PC e ele apita, mas a tela fica preta. Mexi dentro dele ontem para limpar.\"", 200, 300,
                "mb_a520mk", "cpu_r5_5600g", "ram_fury8_ddr4", 2, new[] { "ssd_nv2_500" }, null, "psu_cv450", "cooler_wraith",
                new[] { D(ComponentCategory.Ram, DefectType.Unseated, "O PC apita em sequência e não mostra imagem.", "Um dos pentes de memória parece torto, fora do encaixe.") }));
            list.Add(C("mnt_e04", Difficulty.Easy, "O Windows não inicia", "Beatriz: \"Aparece uma mensagem de disco não encontrado, e ouço um clique estranho vindo do PC.\"", 400, 360,
                "mb_b550m", "cpu_r5_5600", "ram_fury16_ddr4", 1, new[] { "hdd_barracuda_1tb" }, "gpu_gtx1650", "psu_cv550", "cooler_hyper212",
                new[] { D(ComponentCategory.Storage, DefectType.FailedPart, "O sistema não encontra o disco e o PC fica em loop de tentativa de boot.", "O HD faz cliques repetidos: o mecanismo está com defeito.") }));
            list.Add(C("mnt_e05", Difficulty.Easy, "Reinicia no meio do jogo", "Otávio: \"Toda vez que o jogo fica mais pesado o PC reinicia do nada.\"", 500, 360,
                "mb_h610m", "cpu_i5_12400f", "ram_fury16_ddr4", 1, new[] { "ssd_nv2_500" }, "gpu_rx6600", "psu_a650bn", "cooler_ag400",
                new[] { D(ComponentCategory.Psu, DefectType.FailedPart, "O PC reinicia sempre que a placa de vídeo exige mais energia.", "A ventoinha da fonte está travada e há cheiro de componente aquecido.") }));
            list.Add(C("mnt_m01", Difficulty.Medium, "Tela preta, de novo", "Sra. Helena: \"Meu neto mexeu no PC e agora a tela não mostra nada, só o computador fica ligado.\"", 400, 420,
                "mb_a520mk", "cpu_r5_5600", "ram_fury8_ddr4", 2, new[] { "ssd_nv2_500" }, "gpu_rtx3050", "psu_cv550", "cooler_hyper212",
                new[] { D(ComponentCategory.Ram, DefectType.FailedPart, "Liga, mas apita e não dá vídeo.", "Os contatos de um dos pentes de memória estão oxidados."), D(ComponentCategory.Gpu, DefectType.Unseated, "A tela continua sem sinal, mesmo com a memória trocada.", "A placa de vídeo está levemente solta no slot.") }));
            list.Add(C("mnt_m02", Difficulty.Medium, "Artefatos e superaquecimento", "Diego: \"Aparecem riscos coloridos na tela e o PC esquenta demais depois de uma hora jogando.\"", 1500, 480,
                "mb_b550m", "cpu_r7_5700x", "ram_fury16_ddr4", 2, new[] { "ssd_nv2_1tb" }, "gpu_rx6600", "psu_a650bn", "cooler_hyper212",
                new[] { D(ComponentCategory.Cooler, DefectType.Dust, "O PC esquenta muito e as ventoinhas ficam no máximo o tempo todo.", "O dissipador do processador está tomado por poeira."), D(ComponentCategory.Gpu, DefectType.FailedPart, "Riscos coloridos e travamentos mesmo com o PC frio.", "A placa de vídeo tem a memória danificada pelo calor.") }));
            list.Add(C("mnt_m03", Difficulty.Medium, "Lento e com tela azul", "Priscila: \"O PC demora uma eternidade para ligar e de vez em quando aparece uma tela azul.\"", 400, 420,
                "mb_h610m", "cpu_i5_12400f", "ram_fury8_ddr4", 1, new[] { "hdd_barracuda_1tb" }, "gpu_gtx1650", "psu_cv450", "cooler_intel_rm1",
                new[] { D(ComponentCategory.Storage, DefectType.FailedPart, "O sistema demora minutos para iniciar e trava ao abrir arquivos.", "O HD tem muitos setores ruins e está no fim da vida."), D(ComponentCategory.Ram, DefectType.Unseated, "Telas azuis aparecem de forma aleatória, mesmo com o disco novo.", "O pente de memória está meio encaixado e faz mau contato.") }));
            list.Add(C("mnt_m04", Difficulty.Medium, "A montagem do primo", "Lúcio: \"Meu primo montou o PC para mim e ele nunca ligou direito. Ele disse que as peças eram todas boas.\"", 300, 420,
                "mb_h610m", "cpu_i3_12100f", "ram_fury16_ddr4", 1, new[] { "ssd_nv2_500" }, "gpu_gtx1650", "psu_cv450", "cooler_hyper212",
                new[] { D(ComponentCategory.Cooler, DefectType.FailedPart, "O PC não liga: há uma incompatibilidade entre o cooler e a placa-mãe.", "O cooler foi feito para outros processadores e não encaixa nesse soquete."), D(ComponentCategory.Ram, DefectType.Unseated, "Mesmo depois de resolver o cooler, o PC liga e reinicia sozinho.", "Um pente de memória está torto no slot.") }));
            list.Add(C("mnt_m05", Difficulty.Medium, "A imagem some do nada", "Vitória: \"Às vezes a imagem some e o PC fica muito quente. Parece que está doente.\"", 200, 420,
                "mb_b550m", "cpu_r5_5600", "ram_fury16_ddr4", 1, new[] { "ssd_nv2_500" }, "gpu_rtx3050", "psu_cv550", "cooler_wraith",
                new[] { D(ComponentCategory.Gpu, DefectType.Unseated, "A imagem some de repente e volta depois de mexer no gabinete.", "A placa de vídeo está levemente solta."), D(ComponentCategory.Cooler, DefectType.Dust, "O PC esquenta e as ventoinhas aceleram o tempo todo.", "As aletas do dissipador estão cobertas de poeira.") }));
            list.Add(C("mnt_h01", Difficulty.Hard, "Sem sinal de vida", "Dra. Marta: \"O PC não dá nenhum sinal: sem luz, sem ventoinha. Já troquei o cabo da tomada.\"", 1000, 540,
                "mb_b650m", "cpu_r7_7700", "ram_fury16_ddr5", 2, new[] { "ssd_nv2_1tb" }, "gpu_rtx4060", "psu_cv550", "cooler_hyper212",
                new[] { D(ComponentCategory.Psu, DefectType.FailedPart, "Nenhuma luz, nenhum som: o PC está totalmente sem energia.", "A fonte tem cheiro de queimado e a ventoinha não gira."), D(ComponentCategory.Cooler, DefectType.Dust, "Depois de ligar, o PC esquenta e desliga em poucos minutos.", "Muita poeira nas aletas do dissipador."), D(ComponentCategory.Ram, DefectType.Unseated, "Liga, mas reinicia e não completa a inicialização.", "Um dos pentes de memória está torto no slot.") }));
            list.Add(C("mnt_h02", Difficulty.Hard, "Cheiro de queimado", "Sr. Edgar: \"Senti cheiro de queimado vindo do PC e agora ele não liga de jeito nenhum.\"", 800, 600,
                "mb_a520mk", "cpu_r5_5600", "ram_fury16_ddr4", 1, new[] { "ssd_nv2_500" }, "gpu_rtx3050", "psu_cv550", "cooler_hyper212",
                new[] { D(ComponentCategory.Motherboard, DefectType.FailedPart, "Nenhuma resposta: a placa-mãe não envia energia aos componentes.", "A região dos reguladores de tensão da placa-mãe está escurecida e queimada."), D(ComponentCategory.Gpu, DefectType.Dust, "Depois de resolver a placa-mãe, o PC esquenta durante jogos.", "As ventoinhas da placa de vídeo estão cheias de poeira."), D(ComponentCategory.Storage, DefectType.Unseated, "O disco some do sistema de vez em quando.", "O SSD está meio solto no conector.") }));
            list.Add(C("mnt_h03", Difficulty.Hard, "A montagem do estagiário", "Gerente da loja: \"O estagiário montou este PC de um cliente e nunca funcionou. Ele jura que seguiu o manual.\"", 700, 540,
                "mb_b760m", "cpu_i5_12400f", "ram_fury8_ddr4", 1, new[] { "ssd_nv2_500" }, "gpu_rx6600", "psu_cv550", "cooler_hyper212",
                new[] { D(ComponentCategory.Ram, DefectType.FailedPart, "O PC não liga: o tipo de memória não combina com a placa-mãe.", "A placa-mãe usa memória DDR5, mas o pente instalado é DDR4."), D(ComponentCategory.Cooler, DefectType.FailedPart, "O PC não liga: o cooler não suporta o soquete do processador.", "O cooler foi projetado para AMD e não encaixa no soquete Intel dessa placa."), D(ComponentCategory.Gpu, DefectType.Unseated, "Depois de resolver tudo, não aparece imagem.", "A placa de vídeo está torta no slot PCIe.") }));
            list.Add(C("mnt_h04", Difficulty.Hard, "Fonte no limite", "Renato: \"Comprei uma placa de vídeo nova e agora o PC desliga quando o jogo exige muito.\"", 800, 540,
                "mb_h610m", "cpu_i7_12700f", "ram_fury16_ddr4", 2, new[] { "ssd_nv2_1tb" }, "gpu_rx7800xt", "psu_cv450", "cooler_ag400",
                new[] { D(ComponentCategory.Psu, DefectType.FailedPart, "O PC desliga quando o jogo exige o máximo da placa de vídeo.", "A fonte de 450 W está no limite para essa placa de vídeo: o ideal é uma mais potente."), D(ComponentCategory.Cooler, DefectType.Dust, "Fora dos jogos, o PC já esquenta mais que o normal.", "O cooler está coberto de poeira."), D(ComponentCategory.Ram, DefectType.Unseated, "O PC às vezes reinicia mesmo sem jogar.", "Um pente de memória está mal encaixado.") }));
            list.Add(C("mnt_h05", Difficulty.Hard, "A live que não terminou", "Léo, streamer: \"O PC apagou no meio da minha live e não quis mais ligar direito.\"", 1600, 600,
                "mb_b550m", "cpu_r7_5700x", "ram_fury16_ddr4", 2, new[] { "ssd_nv2_1tb" }, "gpu_rx6600", "psu_a650bn", "cooler_hyper212",
                new[] { D(ComponentCategory.Gpu, DefectType.FailedPart, "A tela fica preta logo depois de ligar o jogo.", "A placa de vídeo sofreu curto e tem componentes escurecidos."), D(ComponentCategory.Psu, DefectType.Dust, "A fonte aquece muito e a ventoinha faz um barulho forte.", "A fonte está entupida de poeira por dentro."), D(ComponentCategory.Storage, DefectType.Unseated, "O sistema às vezes não encontra o disco.", "O SSD está meio solto no conector.") }));
            return list;
        }

        // ---------------------------------------------------------------------------------
        //  MENU 2: gerar missões, perfis da IA e catálogos
        // ---------------------------------------------------------------------------------
        [MenuItem("Tools/MestreDoPC/Pacote de missões/2 - Gerar 45 missões e catálogos")]
        public static void Generate()
        {
            var parts = PackUtil.PartsById();
            if (parts.Count == 0)
            {
                Debug.LogError("[Pacote] Não há peças no projeto. Rode antes: Tools > MestreDoPC > Pacote de missões > 1 - Gerar pacote de peças.");
                return;
            }

            foreach (var folder in new[] { AssemblyFolder, AIFolder, CasesFolder, ProfilesFolder })
                Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();

            var assembly = AssemblyMissions().Select(d => EnsureMission(AssemblyFolder, d, GameMode.Assembly)).ToList();
            var duels = AIMissions().Select(d => EnsureMission(AIFolder, d, GameMode.AIChallenge)).ToList();
            var cases = MaintenanceCases().Select(d => EnsureCase(d, parts)).Where(c => c != null).ToList();

            var missionCatalog = PackUtil.FindOrCreate<MissionCatalogSO>(MissionCatalogPath);
            int addedMissions = PackUtil.AddToList(missionCatalog, "missions", assembly.Concat(duels));

            var maintenanceCatalog = PackUtil.FindOrCreate<MaintenanceCatalogSO>(MaintenanceCatalogPath);
            int addedCases = PackUtil.AddToList(maintenanceCatalog, "cases", cases);

            EnsureAIProfiles();

            AssetDatabase.SaveAssets();
            Debug.Log($"[Pacote] Montagem: {assembly.Count} | Contra a IA: {duels.Count} | Manutenção: {cases.Count}. " +
                      $"Novas no MissionCatalog: {addedMissions}; novas no MaintenanceCatalog: {addedCases}. " +
                      "Agora rode o menu 3 (validar) e ligue os catálogos nas cenas (veja o guia).");
        }

        private static MissionDataSO EnsureMission(string folder, MissionDef d, GameMode mode)
        {
            string path = $"{folder}/{d.id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<MissionDataSO>(path);
            if (existing != null) return existing; // preserva edições manuais

            var m = ScriptableObject.CreateInstance<MissionDataSO>();
            m.id = d.id;
            m.title = d.title;
            m.clientBrief = WithRequirements(d);
            m.mode = mode;
            m.difficulty = d.diff;
            m.budget = d.budget;
            m.timeLimitSeconds = d.time;
            m.minRamGB = d.ram;
            m.minStorageGB = d.storage;
            m.minCores = d.cores;
            m.minGpuVramGB = d.vram;
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        // O cliente precisa dizer o que quer: o texto termina com os requisitos que a pontuação vai cobrar.
        private static string WithRequirements(MissionDef d)
        {
            var req = new List<string>();
            if (d.ram > 0) req.Add($"RAM mín. {d.ram} GB");
            if (d.storage > 0) req.Add(d.storage >= 1000 ? $"armazenamento mín. {d.storage / 1000f:0.#} TB" : $"armazenamento mín. {d.storage} GB");
            if (d.cores > 0) req.Add($"CPU com {d.cores}+ núcleos");
            if (d.vram > 0) req.Add($"GPU com {d.vram}+ GB de VRAM");

            string time = d.time > 0 ? $" Tempo: {d.time / 60}:{d.time % 60:00}." : "";
            return $"{d.brief}\n\nRequisitos: {string.Join(" | ", req)}. Orçamento: R$ {d.budget:N0}.{time}";
        }

        private static MaintenanceCaseDataSO EnsureCase(CaseDef d, Dictionary<string, ComponentDataSO> parts)
        {
            string path = $"{CasesFolder}/{d.id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<MaintenanceCaseDataSO>(path);
            if (existing != null) return existing;

            bool ok = true;
            T Get<T>(string id) where T : ComponentDataSO
            {
                if (id == null) return null;
                if (parts.TryGetValue(id, out var p) && p is T typed) return typed;
                Debug.LogError($"[Pacote] Caso {d.id}: peça '{id}' não encontrada. Rode o menu 1 primeiro.");
                ok = false;
                return null;
            }

            var c = ScriptableObject.CreateInstance<MaintenanceCaseDataSO>();
            c.id = d.id;
            c.title = d.title;
            c.clientComplaint = d.complaint;
            c.difficulty = d.diff;
            c.budget = d.budget;
            c.timeLimitSeconds = d.time;
            c.motherboard = Get<MotherboardDataSO>(d.mb);
            c.cpu = Get<CpuDataSO>(d.cpu);
            c.ram = Get<RamDataSO>(d.ram);
            c.ramQuantity = d.ramQty;
            c.storage = d.storage.Select(id => Get<StorageDataSO>(id)).ToList();
            c.gpu = Get<GpuDataSO>(d.gpu);
            c.psu = Get<PsuDataSO>(d.psu);
            c.cooler = Get<CoolerDataSO>(d.cooler);
            c.defects = d.defects.Select(x => new DefectData
            {
                category = x.cat, type = x.type, symptom = x.symptom, inspectionHint = x.hint
            }).ToList();

            if (!ok) { Object.DestroyImmediate(c); return null; }
            AssetDatabase.CreateAsset(c, path);
            return c;
        }

        // Três perfis (um por dificuldade). Se já existir um perfil para a dificuldade, só ajusta o fator de desempenho.
        private static void EnsureAIProfiles()
        {
            var defs = new[]
            {
                new { diff = Difficulty.Easy,   file = "ai_amateur",   name = "IA Amadora", time = new Vector2(300, 420), rank = new Vector2(0.4f, 0.9f),  perf = new Vector2(0.62f, 0.80f) },
                new { diff = Difficulty.Medium, file = "ai_technical", name = "IA Técnica", time = new Vector2(200, 300), rank = new Vector2(0f, 0.15f),   perf = new Vector2(0.82f, 0.93f) },
                new { diff = Difficulty.Hard,   file = "ai_master",    name = "IA Mestre",  time = new Vector2(130, 200), rank = new Vector2(0f, 0f),      perf = new Vector2(0.95f, 1.00f) },
            };

            var existing = AssetDatabase.FindAssets("t:AIProfileSO")
                .Select(g => AssetDatabase.LoadAssetAtPath<AIProfileSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(p => p != null).ToList();

            foreach (var d in defs)
            {
                var profile = existing.FirstOrDefault(p => p.difficulty == d.diff);
                if (profile == null)
                {
                    profile = ScriptableObject.CreateInstance<AIProfileSO>();
                    profile.displayName = d.name;
                    profile.difficulty = d.diff;
                    profile.timeRange = d.time;
                    profile.rankMin = d.rank.x;
                    profile.rankMax = d.rank.y;
                    AssetDatabase.CreateAsset(profile, $"{ProfilesFolder}/{d.file}.asset");
                    Debug.Log($"[Pacote] Perfil criado: {d.name}. Arraste-o na lista Profiles do AIOpponentManager (cena 07).");
                }
                profile.performanceRange = d.perf;
                EditorUtility.SetDirty(profile);
            }
        }

        // ---------------------------------------------------------------------------------
        //  MENU 3: validar se todas as missões têm solução com as peças do projeto
        // ---------------------------------------------------------------------------------
        [MenuItem("Tools/MestreDoPC/Pacote de missões/3 - Validar viabilidade das missões")]
        public static void Validate()
        {
            var parts = PackUtil.PartsById().Values.ToList();
            var builds = EnumerateBuilds(parts);
            var report = new StringBuilder($"[Validação] {builds.Count} montagens possíveis na bancada (sem erro de compatibilidade).\n");
            int problems = 0;

            // --- Montagem e Contra a IA ---
            var missions = AssetDatabase.FindAssets("t:MissionDataSO")
                .Select(g => AssetDatabase.LoadAssetAtPath<MissionDataSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(m => m != null)
                .OrderBy(m => m.mode).ThenBy(m => m.difficulty).ThenBy(m => m.id).ToList();

            var db = AssetDatabase.FindAssets("t:ComponentDatabaseSO")
                .Select(g => AssetDatabase.LoadAssetAtPath<ComponentDatabaseSO>(AssetDatabase.GUIDToAssetPath(g)))
                .FirstOrDefault();
            var profiles = AssetDatabase.FindAssets("t:AIProfileSO")
                .Select(g => AssetDatabase.LoadAssetAtPath<AIProfileSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(p => p != null).ToList();

            report.AppendLine("\n== Missões (Montagem e Contra a IA) ==");
            foreach (var m in missions)
            {
                var ok = builds.Where(b => b.TotalPrice <= m.budget && Meets(m, b)).ToList();
                if (ok.Count == 0)
                {
                    int cheapest = builds.Where(b => Meets(m, b)).Select(b => b.TotalPrice).DefaultIfEmpty(-1).Min();
                    report.AppendLine($"  IMPOSSÍVEL  {m.id} ({m.mode}/{m.difficulty}): orçamento R$ {m.budget:N0}, a montagem mais barata que cumpre os requisitos custa R$ {(cheapest < 0 ? "?" : cheapest.ToString("N0"))}.");
                    problems++;
                    continue;
                }

                int min = ok.Min(b => b.TotalPrice);
                string line = $"  ok  {m.id,-8} {m.mode,-11} {m.difficulty,-6} orç. R$ {m.budget,6:N0} | soluções {ok.Count,5} | mínimo R$ {min,6:N0} | folga {(float)m.budget / min:0.00}x";

                if (m.mode == GameMode.AIChallenge && db != null)
                {
                    var profile = profiles.FirstOrDefault(p => p.difficulty == m.difficulty);
                    var aiBuild = profile != null ? AIBuilder.Build(db, m, profile, m.timeLimitSeconds * 0.5f) : null;
                    line += aiBuild != null ? $" | IA acha montagem (R$ {aiBuild.TotalPrice:N0})" : " | IA NÃO ACHOU MONTAGEM";
                    if (aiBuild == null) problems++;
                }
                report.AppendLine(line);
            }

            // --- Manutenção ---
            report.AppendLine("\n== Casos de Manutenção ==");
            var cases = AssetDatabase.FindAssets("t:MaintenanceCaseDataSO")
                .Select(g => AssetDatabase.LoadAssetAtPath<MaintenanceCaseDataSO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(c => c != null).OrderBy(c => c.difficulty).ThenBy(c => c.id).ToList();

            foreach (var c in cases)
            {
                string result = ValidateCase(c, parts);
                bool bad = result.StartsWith("PROBLEMA");
                if (bad) problems++;
                report.AppendLine($"  {(bad ? "!!" : "ok")}  {c.id,-8} {c.difficulty,-6} orç. R$ {c.budget,5:N0} | {result}");
            }

            report.AppendLine(problems == 0 ? "\nTudo certo: nenhuma missão impossível." : $"\n{problems} problema(s) encontrado(s).");
            if (problems == 0) Debug.Log(report.ToString()); else Debug.LogWarning(report.ToString());
        }

        private static bool Meets(MissionDataSO m, PcBuild b)
        {
            int ram = b.ram != null ? b.ram.capacityGB * b.ramQuantity : 0;
            int storage = b.storage.Sum(s => s.capacityGB);
            int cores = b.cpu != null ? b.cpu.cores : 0;
            int vram = b.gpu != null ? b.gpu.vramGB : 0;
            return ram >= m.minRamGB && storage >= m.minStorageGB && cores >= m.minCores && vram >= m.minGpuVramGB;
        }

        // Todas as montagens que cabem na bancada: 1 armazenamento, 1 a 2 módulos de RAM iguais, GPU opcional.
        private static List<PcBuild> EnumerateBuilds(List<ComponentDataSO> parts)
        {
            var cpus = parts.OfType<CpuDataSO>().ToList();
            var mobos = parts.OfType<MotherboardDataSO>().ToList();
            var rams = parts.OfType<RamDataSO>().ToList();
            var storages = parts.OfType<StorageDataSO>().ToList();
            var gpus = new List<GpuDataSO> { null };
            gpus.AddRange(parts.OfType<GpuDataSO>());
            var psus = parts.OfType<PsuDataSO>().ToList();
            var coolers = parts.OfType<CoolerDataSO>().ToList();

            var result = new List<PcBuild>();
            for (int i = 0; i < cpus.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Validando missões", "Enumerando montagens...", (float)i / cpus.Count);
                var cpu = cpus[i];
                foreach (var mb in mobos.Where(m => m.socket == cpu.socket))
                foreach (var ram in rams.Where(r => r.type == mb.ramType))
                for (int qty = 1; qty <= Mathf.Min(2, mb.ramSlots); qty++)
                foreach (var sto in storages)
                foreach (var gpu in gpus)
                foreach (var psu in psus)
                foreach (var cooler in coolers)
                {
                    var b = new PcBuild { motherboard = mb, cpu = cpu, ram = ram, ramQuantity = qty, gpu = gpu, psu = psu, cooler = cooler };
                    b.storage.Add(sto);
                    if (CompatibilityChecker.MissingParts(b).Length > 0) continue;
                    if (CompatibilityChecker.Validate(b).HasErrors) continue;
                    result.Add(b);
                }
            }
            EditorUtility.ClearProgressBar();
            return result;
        }

        // Confere o PC recebido e procura a reposição mais barata que deixa o PC sem erros e sem avisos.
        private static string ValidateCase(MaintenanceCaseDataSO c, List<ComponentDataSO> parts)
        {
            var initial = new PcBuild
            {
                motherboard = c.motherboard, cpu = c.cpu, ram = c.ram, ramQuantity = Mathf.Max(1, c.ramQuantity),
                gpu = c.gpu, psu = c.psu, cooler = c.cooler
            };
            initial.storage.AddRange(c.storage.Where(s => s != null));

            var missing = CompatibilityChecker.MissingParts(initial);
            if (missing.Length > 0) return "PROBLEMA: PC recebido incompleto (" + string.Join(", ", missing) + ").";

            foreach (var d in c.defects)
                if (!HasCategory(initial, d.category))
                    return $"PROBLEMA: defeito em {d.category}, mas o PC não tem essa peça.";

            var report = CompatibilityChecker.Validate(initial);
            string start = report.HasErrors ? "PC recebido já com erro de compatibilidade (montagem errada)" : "PC recebido compatível";

            var failed = c.defects.Where(d => d.type == DefectType.FailedPart).Select(d => d.category).Distinct().ToList();
            if (failed.Count == 0) return start + "; sem peças a trocar.";

            int best = int.MaxValue;
            Search(0, initial, 0);

            void Search(int index, PcBuild current, int cost)
            {
                if (cost >= best) return;
                if (index == failed.Count)
                {
                    var r = CompatibilityChecker.Validate(current);
                    if (!r.HasErrors && r.Issues.Count == 0) best = cost;
                    return;
                }

                var cat = failed[index];
                foreach (var option in parts.Where(p => p.Category == cat))
                {
                    // Com 2 módulos de RAM, o novo módulo precisa ser igual ao que ficou no PC.
                    if (cat == ComponentCategory.Ram && initial.ramQuantity == 2 && option != initial.ram) continue;
                    Search(index + 1, Replace(current, option), cost + option.price);
                }
            }

            if (best == int.MaxValue) return "PROBLEMA: nenhuma reposição deixa o PC sem erros/avisos (faltam peças no banco?).";
            if (best > c.budget) return $"PROBLEMA: a reposição mais barata custa R$ {best:N0} e o orçamento é R$ {c.budget:N0}.";
            return $"{start}; {c.defects.Count} defeito(s); reposição mínima R$ {best:N0}.";
        }

        private static bool HasCategory(PcBuild b, ComponentCategory cat)
        {
            switch (cat)
            {
                case ComponentCategory.Motherboard: return b.motherboard != null;
                case ComponentCategory.Cpu: return b.cpu != null;
                case ComponentCategory.Ram: return b.ram != null;
                case ComponentCategory.Storage: return b.storage.Count > 0;
                case ComponentCategory.Gpu: return b.gpu != null;
                case ComponentCategory.Psu: return b.psu != null;
                case ComponentCategory.Cooler: return b.cooler != null;
            }
            return false;
        }

        private static PcBuild Replace(PcBuild b, ComponentDataSO part)
        {
            var n = new PcBuild
            {
                motherboard = b.motherboard, cpu = b.cpu, ram = b.ram, ramQuantity = b.ramQuantity,
                gpu = b.gpu, psu = b.psu, cooler = b.cooler
            };
            n.storage.AddRange(b.storage);

            switch (part)
            {
                case MotherboardDataSO m: n.motherboard = m; break;
                case CpuDataSO c: n.cpu = c; break;
                case RamDataSO r: n.ram = r; break;
                case StorageDataSO s: n.storage.Clear(); n.storage.Add(s); break;
                case GpuDataSO g: n.gpu = g; break;
                case PsuDataSO p: n.psu = p; break;
                case CoolerDataSO co: n.cooler = co; break;
            }
            return n;
        }
    }
}
#endif
