﻿namespace SewingMaterialsStorage.Models
{
    public class MaterialThread
    {
        public int MaterialId { get; set; }
        public Material Material { get; set; }
        public int? Thickness { get; set; }
        public int? LengthPerSpool { get; set; }
    }
}