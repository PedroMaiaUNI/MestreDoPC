namespace MestreDoPC
{
    public static class CategoryLabels
    {
        public static string Of(ComponentCategory c) => c switch
        {
            ComponentCategory.Motherboard => "Placa-mãe",
            ComponentCategory.Cpu         => "CPU",
            ComponentCategory.Ram         => "Memória",
            ComponentCategory.Storage     => "Armazenamento",
            ComponentCategory.Gpu         => "GPU",
            ComponentCategory.Psu         => "Fonte",
            ComponentCategory.Cooler      => "Cooler",
            _                             => c.ToString()
        };
    }
}
