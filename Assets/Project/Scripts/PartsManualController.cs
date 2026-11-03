using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MestreDoPC
{
    public class PartsManualController : MonoBehaviour
    {
        [Header("Textos do Manual")]
        [SerializeField] private TMP_Text partNameText;
        [SerializeField] private TMP_Text partDescriptionText;

        [Header("Imagem da Peça")]
        [SerializeField] private Image partImage;

        [Header("Imagens das Peças")]
        [SerializeField] private Sprite cpuImage;
        [SerializeField] private Sprite motherboardImage;
        [SerializeField] private Sprite ramImage;
        [SerializeField] private Sprite gpuImage;
        [SerializeField] private Sprite storageImage;
        [SerializeField] private Sprite psuImage;
        [SerializeField] private Sprite coolerImage;

        [Header("Modelos 3D")]
        [SerializeField] private GameObject coolerModel;
        [SerializeField] private GameObject cpuModel;
        [SerializeField] private GameObject motherboardModel;
        [SerializeField] private GameObject ramModel;
        [SerializeField] private GameObject gpuModel;

        private void HideAllModels()
        {
            SetModelActive(coolerModel, false);
            SetModelActive(cpuModel, false);
            SetModelActive(motherboardModel, false);
            SetModelActive(ramModel, false);
            SetModelActive(gpuModel, false);
        }

        private void SetModelActive(GameObject model, bool active)
        {
            if (model != null)
            {
                model.SetActive(active);
            }
        }

        private void Start()
                {
                    if (partImage != null)
                    {
                        partImage.sprite = null;
                        partImage.enabled = false;
                    }
        }

        private void ShowPartImage(Sprite image)
        {
            HideAllModels();

            if (partImage != null)
            {
                partImage.sprite = image;
                partImage.enabled = image != null;
                partImage.preserveAspect = true;
            }
            else
            {
                Debug.LogWarning(
                    "PartImage não foi atribuído no Inspector."
                );
            }

            if (image == null)
            {
                Debug.LogWarning(
                    "A imagem da peça selecionada não foi atribuída."
                );
            }
        }

        public void ShowCPU()
        {
            partNameText.text = "PROCESSADOR (CPU)";
            partDescriptionText.text =
                "O processador (CPU) executa instruções e realiza " +
                "os cálculos do computador.\n" +
                "Seu desempenho influencia a velocidade de processamento.";

            ShowPartImage(cpuImage);
        }

        public void ShowMotherboard()
        {
            partNameText.text = "PLACA-MÃE";
            partDescriptionText.text =
                "A placa-mãe conecta os componentes do computador " +
                "e permite que eles se comuniquem.\n" +
                "Nela são instalados o processador, a memória RAM " +
                "e outros componentes.";

            ShowPartImage(motherboardImage);
        }

        public void ShowRAM()
        {
            partNameText.text = "MEMÓRIA RAM";
            partDescriptionText.text =
                "A memória RAM armazena temporariamente os dados " +
                "utilizados pelo computador.\n" +
                "Ela ajuda a executar programas e tarefas simultaneamente.";

            ShowPartImage(ramImage);
        }

        public void ShowGPU()
        {
            partNameText.text = "PLACA DE VÍDEO (GPU)";
            partDescriptionText.text =
                "A placa de vídeo (GPU) processa imagens e gráficos " +
                "exibidos pelo computador.\n" +
                "Ela é especialmente importante para jogos e aplicações gráficas.";

            ShowPartImage(gpuImage);
        }

        public void ShowStorage()
        {
            partNameText.text = "ARMAZENAMENTO";
            partDescriptionText.text =
                "O armazenamento guarda arquivos, programas e o sistema operacional.\n" +
                "Os tipos mais comuns são SSD e HD.";

            ShowPartImage(storageImage);
        }

        public void ShowPSU()
        {
            partNameText.text = "FONTE DE ALIMENTAÇÃO (PSU)";
            partDescriptionText.text =
                "A fonte de alimentação fornece energia elétrica " +
                "aos componentes do computador.\n" +
                "Sua potência deve ser adequada às necessidades do sistema.";

            ShowPartImage(psuImage);
        }

public void ShowCooler()
        {
            partNameText.text = "COOLER DE CPU";
            partDescriptionText.text =
                "O cooler dissipa o calor do processador, ajudando " +
                "a evitar o superaquecimento e a manter a estabilidade.";

            // Esconde os outros modelos 3D.
            SetModelActive(cpuModel, false);
            SetModelActive(motherboardModel, false);
            SetModelActive(ramModel, false);
            SetModelActive(gpuModel, false);

            // Esconde a imagem 2D.
            if (partImage != null)
            {
                partImage.sprite = null;
                partImage.enabled = false;
            }

            // Exibe o cooler em 3D.
            if (coolerModel != null)
            {
                coolerModel.SetActive(true);
            }
            else
            {
                Debug.LogWarning(
                    "Cooler Model não foi atribuído no Inspector."
                );
            }
        }

        public void BackToMainMenu()
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.Load(SceneNames.MainMenu);
            }
            else
            {
                Debug.LogError(
                    "SceneLoader.Instance não foi encontrado."
                );
            }
        }
    }
}