using System.ComponentModel.DataAnnotations;

namespace SewingMaterialsStorage.ViewModels
{
    public class ExcelImportViewModel
    {
        [Required(ErrorMessage = "Выберите файл для загрузки")]
        [Display(Name = "Файл Excel")]
        public IFormFile ExcelFile { get; set; }

        [Display(Name = "Тип данных")]
        public ImportType ImportType { get; set; }
    }

    public enum ImportType
    {
        Colors,
        Compositions
    }
}
