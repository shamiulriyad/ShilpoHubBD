namespace ShilpoHubBD.Domain.Entities.Tourism;

public enum TourismLocationType
{
    Hotel,
    Resort,
    Hostel,
    TouristPlace,
    HeritageSite,
    Restaurant,
    Attraction,
    // Appended (the column stores the name, not the number, so existing rows are unaffected).
    GuestHouse,
    Motel,
    Park,
    Museum,
    HistoricalPlace,
    Cafe,
    Beach,
    Homestay,
    Viewpoint,
    Mosque,
    Temple,
}
