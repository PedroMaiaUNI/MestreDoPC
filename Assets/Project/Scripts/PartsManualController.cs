
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
        [SerializeField] private GameObject storageModel;
        [SerializeField] private GameObject psuModel;

        [Header("Rotação dos Modelos")]
        [SerializeField] private ModelRotation modelRotation;

        private void Start()
        {
            HideAllModels();
            HidePartImage();

            if (modelRotation != null)
            {
                modelRotation.SetModel(null);
            }
        }

        private void HideAllModels()
        {
            SetModelActive(coolerModel, false);
            SetModelActive(cpuModel, false);
            SetModelActive(motherboardModel, false);
            SetModelActive(ramModel, false);
            SetModelActive(gpuModel, false);
            SetModelActive(storageModel, false);
            SetModelActive(psuModel, false);
        }

        private void SetModelActive(GameObject model, bool active)
        {
            if (model != null)
            {
                model.SetActive(active);
            }
        }

        private void HidePartImage()
        {
            if (partImage != null)
            {
                partImage.sprite = null;
                partImage.enabled = false;
            }
        }

        private void Show3DPart(
            string partName,
            string description,
            GameObject selectedModel,
            Sprite fallbackImage)
        {
            if (partNameText != null)
            {
                partNameText.text = partName;
            }

            if (partDescriptionText != null)
            {
                partDescriptionText.text = description;
            }

            HideAllModels();
            HidePartImage();

            if (modelRotation != null)
            {
                modelRotation.SetModel(null);
            }

            if (selectedModel != null)
            {
                selectedModel.SetActive(true);

                if (modelRotation != null)
                {
                    modelRotation.SetModel(selectedModel.transform);
                }
            }
            else
            {
                Debug.LogWarning(
                    "Modelo 3D não atribuído para: " + partName
                );

                if (partImage != null && fallbackImage != null)
                {
                    partImage.sprite = fallbackImage;
                    partImage.enabled = true;
                    partImage.preserveAspect = true;
                }
            }
        }

        public void ShowCPU()
        {
            Show3DPart(
                "PROCESSADOR (CPU)",
                "O processador (CPU) executa instruções e realiza " +
                "os cálculos do computador.\n" +
                "Seu desempenho influencia a velocidade de processamento.",
                cpuModel,
                cpuImage
            );
        }

        public void ShowMotherboard()
        {
            Show3DPart(
                "PLACA-MÃE",
                "A placa-mãe conecta os componentes do computador " +
                "e permite que eles se comuniquem.\n" +
                "Nela são instalados o processador, a memória RAM " +
                "e outros componentes.",
                motherboardModel,
                motherboardImage
            );
        }

        public void ShowRAM()
        {
            Show3DPart(
                "MEMÓRIA RAM",
                "A memória RAM armazena temporariamente os dados " +
                "utilizados pelo computador.\n" +
                "Ela ajuda a executar programas e tarefas simultaneamente.",
                ramModel,
                ramImage
            );
        }

        public void ShowGPU()
        {
            Show3DPart(
                "PLACA DE VÍDEO (GPU)",
                "A placa de vídeo (GPU) processa imagens e gráficos " +
                "exibidos pelo computador.\n" +
                "Ela é especialmente importante para jogos e aplicações gráficas.",
                gpuModel,
                gpuImage
            );
        }

        public void ShowStorage()
        {
            Show3DPart(
                "ARMAZENAMENTO",
                "O armazenamento guarda arquivos, programas e o sistema operacional.\n" +
                "Os tipos mais comuns são SSD e HD.",
                storageModel,
                storageImage
            );
        }

        public void ShowPSU()
        {
            Show3DPart(
                "FONTE DE ALIMENTAÇÃO (PSU)",
                "A fonte de alimentação fornece energia elétrica " +
                "aos componentes do computador.\n" +
                "Sua potência deve ser adequada às necessidades do sistema.",
                psuModel,
                psuImage
            );
        }

        public void ShowCooler()
        {
            Show3DPart(
                "COOLER DE CPU",
                "O cooler dissipa o calor do processador, ajudando " +
                "a evitar o superaquecimento e a manter a estabilidade.",
                coolerModel,
                coolerImage
            );
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
