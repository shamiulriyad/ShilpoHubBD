using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.Tourism;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Options;
using ShilpoHubBD.Domain.Entities.Tourism;

namespace ShilpoHubBD.Infrastructure.Tourism;

// The two public read services share one shape: make sure the district's imported data is fresh
// (never throws), then read from OUR database. The frontend never sees Overpass.
public abstract class TourismLocationQueryBase
{
    private readonly ITourismLocationRepository _locations;
    private readonly ITourismExternalSyncService _sync;
    private readonly ExternalPoiOptions _options;

    protected TourismLocationQueryBase(ITourismLocationRepository locations, ITourismExternalSyncService sync, IOptions<ExternalPoiOptions> options)
    {
        _locations = locations;
        _sync = sync;
        _options = options.Value;
    }

    protected abstract IReadOnlyCollection<TourismLocationType> Types { get; }
    protected abstract ExternalPoiScope Scope { get; }

    public async Task<List<TourismLocationDto>> GetByDistrictAsync(Guid districtId, CancellationToken cancellationToken)
    {
        await _sync.EnsureFreshAsync(districtId, cancellationToken, scope: Scope);
        var rows = await _locations.GetForDistrictAsync(districtId, Types, cancellationToken);
        return rows.Select(ToDto).ToList();
    }

    // Stored data only: a coordinate has no district to import into, so this never calls Overpass.
    public async Task<List<TourismLocationDto>> GetNearbyAsync(double latitude, double longitude, double? radiusKm, CancellationToken cancellationToken)
    {
        var radius = Math.Clamp(radiusKm ?? _options.DefaultRadiusKm, 0.5, _options.MaxRadiusKm);
        var dLat = radius / 111.0;
        var dLon = radius / (111.0 * Math.Max(0.1, Math.Cos(latitude * Math.PI / 180)));
        var rows = await _locations.GetInBoundsAsync(latitude - dLat, latitude + dLat, longitude - dLon, longitude + dLon, Types, cancellationToken);
        return rows
            .Where(l => TourismExternalSyncService.DistanceMeters(latitude, longitude, l.Latitude, l.Longitude) <= radius * 1000)
            .OrderBy(l => l.Source == TourismExternalSyncService.OpenStreetMap).ThenByDescending(l => l.IsVerified)
            .ThenBy(l => TourismExternalSyncService.DistanceMeters(latitude, longitude, l.Latitude, l.Longitude))
            .Select(ToDto).ToList();
    }

    protected static NearbyAccommodationDto ToNearbyDto(TourismLocation l, double km)
    {
        var d = ToDto(l);
        return new NearbyAccommodationDto
        {
            Id = d.Id, Name = d.Name, Type = d.Type, Description = d.Description, Address = d.Address,
            Latitude = d.Latitude, Longitude = d.Longitude, Price = d.Price, EntryFee = d.EntryFee,
            OpeningHours = d.OpeningHours, ContactInfo = d.ContactInfo, Facilities = d.Facilities,
            ImageUrl = d.ImageUrl, ImageCredit = d.ImageCredit, ImageSourceUrl = d.ImageSourceUrl,
            IsVerified = d.IsVerified, IsActive = d.IsActive, Area = d.Area, SourceUrl = d.SourceUrl,
            VerificationStatus = d.VerificationStatus, UnverifiedFields = d.UnverifiedFields,
            CoordinatesSource = d.CoordinatesSource, CoordinatesPrecision = d.CoordinatesPrecision,
            DataRetrievedOn = d.DataRetrievedOn, Source = d.Source, ExternalId = d.ExternalId, Upazila = d.Upazila,
            LastSyncedAt = d.LastSyncedAt, DistrictId = d.DistrictId, DistrictName = d.DistrictName,
            CreatedAt = d.CreatedAt, UpdatedAt = d.UpdatedAt, DistanceKm = Math.Round(km, 2),
        };
    }

    private static TourismLocationDto ToDto(TourismLocation l) => new()
    {
        Id = l.Id, Name = l.Name, Type = l.Type.ToString(), Description = l.Description, Address = l.Address,
        Latitude = l.Latitude, Longitude = l.Longitude, Price = l.Price, EntryFee = l.EntryFee,
        OpeningHours = l.OpeningHours, ContactInfo = l.ContactInfo, Facilities = l.Facilities,
        ImageUrl = l.ImageUrl, ImageCredit = l.ImageCredit, ImageSourceUrl = l.ImageSourceUrl,
        IsVerified = l.IsVerified, IsActive = l.IsActive, Area = l.Area, SourceUrl = l.SourceUrl,
        VerificationStatus = l.VerificationStatus, UnverifiedFields = l.UnverifiedFields,
        CoordinatesSource = l.CoordinatesSource, CoordinatesPrecision = l.CoordinatesPrecision,
        DataRetrievedOn = l.DataRetrievedOn, Source = l.Source, ExternalId = l.ExternalId, Upazila = l.Upazila,
        LastSyncedAt = l.LastSyncedAt, DistrictId = l.DistrictId, DistrictName = l.District.Name,
        CreatedAt = l.CreatedAt, UpdatedAt = l.UpdatedAt,
    };
}

public class AccommodationService : TourismLocationQueryBase, IAccommodationService
{
    public static readonly TourismLocationType[] AccommodationTypes =
    [
        TourismLocationType.Hotel, TourismLocationType.Resort, TourismLocationType.Hostel,
        TourismLocationType.GuestHouse, TourismLocationType.Motel, TourismLocationType.Homestay,
    ];

    private readonly IDistrictRepository _districts;
    private readonly IGeocodingProvider _geocoder;
    private readonly ITourismLocationRepository _locationRepository;
    private readonly ITourismExternalSyncService _syncService;
    private readonly ExternalPoiOptions _poiOptions;

    public AccommodationService(
        ITourismLocationRepository locations, ITourismExternalSyncService sync, IOptions<ExternalPoiOptions> options,
        IDistrictRepository districts, IGeocodingProvider geocoder)
        : base(locations, sync, options)
    {
        _districts = districts;
        _geocoder = geocoder;
        _locationRepository = locations;
        _syncService = sync;
        _poiOptions = options.Value;
    }

    protected override IReadOnlyCollection<TourismLocationType> Types => AccommodationTypes;
    protected override ExternalPoiScope Scope => ExternalPoiScope.Accommodation;

    // Geocode the destination once (cached by the geocoder), make sure OpenStreetMap accommodation
    // around it has been fetched into our database (small dedicated query, never blocks longer than
    // MaxWaitSeconds), then answer from the database: everything to stay at within the radius.
    public async Task<NearbyAccommodationsDto> GetAroundDestinationAsync(Guid districtId, double? radiusKm, CancellationToken cancellationToken)
    {
        var district = await _districts.GetByIdAsync(districtId, cancellationToken)
            ?? throw new ShilpoHubBD.Application.Exceptions.NotFoundException("District not found.");
        var radius = Math.Clamp(radiusKm ?? _poiOptions.DefaultRadiusKm, 1, _poiOptions.MaxRadiusKm);
        var result = new NearbyAccommodationsDto { DestinationName = district.Name, RadiusKm = radius };

        var center = await _geocoder.GeocodeAsync($"{district.Name} District, Bangladesh", cancellationToken)
            ?? await _geocoder.GeocodeAsync($"{district.Name}, Bangladesh", cancellationToken);
        if (center is null)
        {
            result.Message = "Could not locate this destination, so nearby accommodation cannot be searched.";
            return result;
        }

        result.CenterLatitude = center.Latitude;
        result.CenterLongitude = center.Longitude;

        var sync = await _syncService.EnsureFreshAsync(districtId, cancellationToken, radius, scope: ExternalPoiScope.Accommodation);
        result.IsImporting = sync.Attempted && !sync.Succeeded && sync.Message == "Import continues in the background.";
        if (sync.Attempted && !sync.Succeeded && !result.IsImporting)
        {
            result.Message = sync.Message;   // e.g. OpenStreetMap unavailable; stored data is still shown
        }

        var dLat = radius / 111.0;
        var dLon = radius / (111.0 * Math.Max(0.1, Math.Cos(center.Latitude * Math.PI / 180)));
        var rows = await _locationRepository.GetInBoundsAsync(
            center.Latitude - dLat, center.Latitude + dLat, center.Longitude - dLon, center.Longitude + dLon, AccommodationTypes, cancellationToken);

        result.Items = rows
            .Select(l => (Row: l, Km: TourismExternalSyncService.DistanceMeters(center.Latitude, center.Longitude, l.Latitude, l.Longitude) / 1000))
            .Where(x => x.Km <= radius)
            // Rows imported before the eatery filter existed stay out of the list too (an admin-reviewed one is kept).
            .Where(x => x.Row.Source != TourismExternalSyncService.OpenStreetMap || x.Row.IsVerified || !OverpassPoiClient.LooksLikeEatery(x.Row.Name))
            .OrderBy(x => x.Km)
            .Select(x => ToNearbyDto(x.Row, x.Km))
            .ToList();
        return result;
    }
}

public class TourismPoiService : TourismLocationQueryBase, ITourismPoiService
{
    public static readonly TourismLocationType[] PoiTypes =
    [
        TourismLocationType.Restaurant, TourismLocationType.Cafe, TourismLocationType.Attraction, TourismLocationType.TouristPlace,
        TourismLocationType.HeritageSite, TourismLocationType.Museum, TourismLocationType.Park, TourismLocationType.Beach,
        TourismLocationType.Viewpoint, TourismLocationType.Mosque, TourismLocationType.Temple, TourismLocationType.HistoricalPlace,
    ];

    public TourismPoiService(ITourismLocationRepository locations, ITourismExternalSyncService sync, IOptions<ExternalPoiOptions> options)
        : base(locations, sync, options) { }

    protected override IReadOnlyCollection<TourismLocationType> Types => PoiTypes;
    protected override ExternalPoiScope Scope => ExternalPoiScope.Places;
}
