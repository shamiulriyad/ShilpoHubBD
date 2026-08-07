using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Configurations;

public class DistrictConfiguration : IEntityTypeConfiguration<District>
{
    // All 64 districts of Bangladesh, grouped by division.
    private static readonly (string Name, string Division)[] Districts =
    {
        ("Barguna", "Barishal"), ("Barishal", "Barishal"), ("Bhola", "Barishal"),
        ("Jhalokati", "Barishal"), ("Patuakhali", "Barishal"), ("Pirojpur", "Barishal"),

        ("Bandarban", "Chattogram"), ("Brahmanbaria", "Chattogram"), ("Chandpur", "Chattogram"),
        ("Chattogram", "Chattogram"), ("Cumilla", "Chattogram"), ("Cox's Bazar", "Chattogram"),
        ("Feni", "Chattogram"), ("Khagrachhari", "Chattogram"), ("Lakshmipur", "Chattogram"),
        ("Noakhali", "Chattogram"), ("Rangamati", "Chattogram"),

        ("Dhaka", "Dhaka"), ("Faridpur", "Dhaka"), ("Gazipur", "Dhaka"), ("Gopalganj", "Dhaka"),
        ("Kishoreganj", "Dhaka"), ("Madaripur", "Dhaka"), ("Manikganj", "Dhaka"), ("Munshiganj", "Dhaka"),
        ("Narayanganj", "Dhaka"), ("Narsingdi", "Dhaka"), ("Rajbari", "Dhaka"), ("Shariatpur", "Dhaka"),
        ("Tangail", "Dhaka"),

        ("Bagerhat", "Khulna"), ("Chuadanga", "Khulna"), ("Jashore", "Khulna"), ("Jhenaidah", "Khulna"),
        ("Khulna", "Khulna"), ("Kushtia", "Khulna"), ("Magura", "Khulna"), ("Meherpur", "Khulna"),
        ("Narail", "Khulna"), ("Satkhira", "Khulna"),

        ("Jamalpur", "Mymensingh"), ("Mymensingh", "Mymensingh"), ("Netrokona", "Mymensingh"), ("Sherpur", "Mymensingh"),

        ("Bogura", "Rajshahi"), ("Chapainawabganj", "Rajshahi"), ("Joypurhat", "Rajshahi"), ("Naogaon", "Rajshahi"),
        ("Natore", "Rajshahi"), ("Pabna", "Rajshahi"), ("Rajshahi", "Rajshahi"), ("Sirajganj", "Rajshahi"),

        ("Dinajpur", "Rangpur"), ("Gaibandha", "Rangpur"), ("Kurigram", "Rangpur"), ("Lalmonirhat", "Rangpur"),
        ("Nilphamari", "Rangpur"), ("Panchagarh", "Rangpur"), ("Rangpur", "Rangpur"), ("Thakurgaon", "Rangpur"),

        ("Habiganj", "Sylhet"), ("Moulvibazar", "Sylhet"), ("Sunamganj", "Sylhet"), ("Sylhet", "Sylhet"),
    };

    private static Guid DistrictId(int index) => Guid.Parse($"20000000-0000-0000-0000-{index:D12}");

    public void Configure(EntityTypeBuilder<District> builder)
    {
        builder.ToTable("Districts");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(d => d.Name).IsUnique();

        builder.Property(d => d.Division).IsRequired().HasMaxLength(100);
        builder.Property(d => d.DisplayOrder).IsRequired();
        builder.Property(d => d.IsActive).IsRequired();

        builder.HasData(Districts.Select((d, i) => new
        {
            Id = DistrictId(i + 1),
            Name = d.Name,
            Division = d.Division,
            DisplayOrder = i + 1,
            IsActive = true,
        }));
    }
}
