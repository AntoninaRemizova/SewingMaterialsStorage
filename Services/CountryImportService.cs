using OfficeOpenXml;
using SewingMaterialsStorage.Models;
using System.Text;

namespace SewingMaterialsStorage.Services
{
    public class CountryImportService
    {
        public List<Country> ImportCountriesFromExcel(string filePath)
        {
            var countries = new List<Country>();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension?.Rows ?? 0;

                for (int row = 1; row <= rowCount; row++)
                {
                    var countryName = worksheet.Cells[row, 1].Text.Trim();

                    if (!string.IsNullOrEmpty(countryName))
                    {
                        countries.Add(new Country
                        {
                            CountryName = countryName
                        });
                    }
                }
            }

            return countries;
        }
    }
}