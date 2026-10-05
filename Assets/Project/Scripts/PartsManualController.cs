using TMPro;
using UnityEngine;

namespace MestreDoPC
{
    public class PartsManualController : MonoBehaviour
    {
        [SerializeField] private TMP_Text partNameText;
        [SerializeField] private TMP_Text partDescriptionText;

        public void ShowCPU()
        {
            partNameText.text = "PROCESSADOR (CPU)";

            partDescriptionText.text =
                "O processador (CPU) é responsável por executar " +
                "as instruções e realizar os cálculos do computador.\n\n" +
                "Quanto melhor o processador, maior pode ser " +
                "a capacidade de processamento do sistema.";
        }

        public void ShowMotherboard()
        {
            partNameText.text = "PLACA-MÃE";

            partDescriptionText.text =
                "A placa-mãe é a principal placa do computador. " +
                "Ela conecta e permite a comunicação entre os diferentes componentes.\n\n" +
                "Nela são instalados o processador, memória RAM, armazenamento " +
                "e outros componentes.";
        }

        public void ShowRAM()
        {
            partNameText.text = "MEMÓRIA RAM";

            partDescriptionText.text =
                "A memória RAM armazena temporariamente os dados " +
                "que estão sendo utilizados pelo computador.\n\n" +
                "Quanto maior a quantidade de RAM, mais programas " +
                "podem ser executados simultaneamente com maior fluidez.";
        }

        public void ShowGPU()
        {
            partNameText.text = "PLACA DE VÍDEO (GPU)";

            partDescriptionText.text =
                "A placa de vídeo (GPU) é responsável pelo processamento " +
                "e pela renderização de imagens, vídeos e gráficos.\n\n" +
                "Uma GPU mais potente permite executar jogos e aplicações " +
                "gráficas com maior qualidade e desempenho.";
        }

        public void ShowStorage()
        {
            partNameText.text = "ARMAZENAMENTO";

            partDescriptionText.text =
                "O armazenamento é responsável por guardar permanentemente " +
                "os arquivos e programas do computador.\n\n" +
                "Os principais tipos são SSD e HD. Os SSDs geralmente oferecem " +
                "maior velocidade de leitura e gravação.";
        }

        public void ShowPSU()
        {
            partNameText.text = "FONTE DE ALIMENTAÇÃO (PSU)";

            partDescriptionText.text =
                "A fonte de alimentação fornece energia elétrica para os " +
                "componentes do computador.\n\n" +
                "Ela deve possuir potência suficiente e qualidade adequada " +
                "para alimentar o sistema com segurança.";
        }
        public void BackToMainMenu()
        {
            SceneLoader.Instance.Load(SceneNames.MainMenu);
        }
    }
}