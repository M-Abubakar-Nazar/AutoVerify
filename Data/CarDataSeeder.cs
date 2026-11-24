using AutoVerifyMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoVerifyMVC.Data;

public static class CarDataSeeder
{
    private static readonly CarListing[] DemoCars =
    [
        new()
        {
            SeedKey = "demo-corolla-2023", Make = "Toyota", Model = "Corolla", Trim = "Altis 1.6 X",
            ModelYear = 2023, Price = 6_100_000m, MileageKm = 27_500, Engine = "1.6L four-cylinder",
            Transmission = "Automatic", FuelType = "Petrol",
            Description = "Well-kept local Corolla with service records, dual airbags, reverse camera and recently replaced tires.",
            ImagePath = "/assets/images/corolla.png", Status = "Available", IsSeedData = true
        },
        new()
        {
            SeedKey = "demo-civic-2022", Make = "Honda", Model = "Civic", Trim = "Oriel 1.5 Turbo",
            ModelYear = 2022, Price = 8_250_000m, MileageKm = 31_200, Engine = "1.5L turbocharged four-cylinder",
            Transmission = "CVT automatic", FuelType = "Petrol",
            Description = "Single-owner Oriel with sunroof, Honda Sensing safety features, complete maintenance history and clean interior.",
            ImagePath = "/assets/images/civic.png", Status = "Available", IsSeedData = true
        },
        new()
        {
            SeedKey = "demo-sportage-2021", Make = "Kia", Model = "Sportage", Trim = "AWD 2.0",
            ModelYear = 2021, Price = 7_350_000m, MileageKm = 44_000, Engine = "2.0L four-cylinder",
            Transmission = "6-speed automatic", FuelType = "Petrol",
            Description = "Comfortable all-wheel-drive crossover with panoramic roof, traction control and documented routine servicing.",
            ImagePath = "/assets/images/kia.png", Status = "Available", IsSeedData = true
        },
        new()
        {
            SeedKey = "demo-alto-2024", Make = "Suzuki", Model = "Alto", Trim = "VXL AGS",
            ModelYear = 2024, Price = 3_050_000m, MileageKm = 12_800, Engine = "658cc three-cylinder",
            Transmission = "AGS automated manual", FuelType = "Petrol",
            Description = "City-friendly Alto with power steering, airbags, air conditioning and low mileage for its model year.",
            ImagePath = "/assets/images/alto.png", Status = "Available", IsSeedData = true
        },
        new()
        {
            SeedKey = "demo-land-cruiser-2020", Make = "Toyota", Model = "Land Cruiser Prado", Trim = "TX 2.7",
            ModelYear = 2020, Price = 39_500_000m, MileageKm = 68_000, Engine = "2.7L four-cylinder",
            Transmission = "6-speed automatic", FuelType = "Petrol",
            Description = "Spacious Prado with selectable four-wheel drive, hill-start assistance, leather seating and complete import documentation.",
            ImagePath = "/assets/images/LandCruiser.png", Status = "Available", IsSeedData = true
        },
        new()
        {
            SeedKey = "demo-sonata-linen-2022", Make = "Hyundai", Model = "Sonata", Trim = "2.5 Limited, Linen Interior",
            ModelYear = 2022, Price = 10_250_000m, MileageKm = 36_500, Engine = "2.5L four-cylinder",
            Transmission = "8-speed automatic", FuelType = "Petrol",
            Description = "Executive sedan with a light linen interior, panoramic sunroof, driver assistance features and careful ownership.",
            ImagePath = "/assets/images/Sonata%20Linen.png", Status = "Available", IsSeedData = true
        },
        new()
        {
            SeedKey = "demo-corolla-grande-2021", Make = "Toyota", Model = "Corolla", Trim = "Grande 1.8",
            ModelYear = 2021, Price = 5_350_000m, MileageKm = 58_000, Engine = "1.8L four-cylinder",
            Transmission = "CVT automatic", FuelType = "Petrol",
            Description = "Local Grande with sunroof, traction control, reverse camera and a complete service history.",
            ImagePath = "/assets/images/corolla.png", Status = "Available", IsSeedData = true
        },
        new()
        {
            SeedKey = "demo-civic-turbo-2020", Make = "Honda", Model = "Civic", Trim = "RS Turbo 1.5",
            ModelYear = 2020, Price = 6_650_000m, MileageKm = 63_500, Engine = "1.5L turbocharged four-cylinder",
            Transmission = "CVT automatic", FuelType = "Petrol",
            Description = "Turbocharged Civic with leather trim, sunroof and maintenance records available for review.",
            ImagePath = "/assets/images/civic.png", Status = "Available", IsSeedData = true
        }
    ];

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

        foreach (var car in DemoCars)
        {
            if (!await dbContext.CarListings.AnyAsync(existing => existing.SeedKey == car.SeedKey))
                dbContext.CarListings.Add(car);
        }

        await dbContext.SaveChangesAsync();

        var sellerIds = await (
            from userRole in dbContext.UserRoles
            join role in dbContext.Roles on userRole.RoleId equals role.Id
            where role.NormalizedName == "SELLER"
            select userRole.UserId)
            .Distinct()
            .OrderBy(id => id)
            .Take(2)
            .ToListAsync();

        // The starter inventory has no historical owner. When there is one seller,
        // make that ownership explicit so it appears on that seller's dashboard
        // and buyers can send requests to the right account.
        if (sellerIds.Count == 1)
            await AssignUnownedSeedListingsAsync(dbContext, sellerIds[0]);
    }

    public static Task<int> AssignUnownedSeedListingsAsync(
        ApplicationDbContext dbContext, string sellerId) =>
        dbContext.CarListings
            .Where(car => car.IsSeedData && car.SellerId == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(car => car.SellerId, sellerId));
}
