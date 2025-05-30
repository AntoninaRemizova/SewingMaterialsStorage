using SewingMaterialsStorage.Services;

namespace SewingMaterialsStorage.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context, IWebHostEnvironment env)
        {
            context.Database.EnsureCreated();

            if (!context.Countries.Any())
            {
                var filePath = Path.Combine(env.WebRootPath, "data", "countries.xlsx");

                if (File.Exists(filePath))
                {
                    var importService = new CountryImportService();
                    var countries = importService.ImportCountriesFromExcel(filePath);

                    context.Countries.AddRange(countries);
                    context.SaveChanges();
                }
            }
        }
    }
}