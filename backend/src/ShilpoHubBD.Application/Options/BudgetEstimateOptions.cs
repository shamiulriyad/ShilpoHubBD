namespace ShilpoHubBD.Application.Options;

// Bound to "Tourism:BudgetEstimate". Rough planning rates used ONLY for the separate "estimated"
// total when no verified figure exists. They are assumptions, shown as such, and never mixed into
// the verified total. Set a rate to 0 to leave that item out of the estimate.
public class BudgetEstimateOptions
{
    public decimal FoodPerPersonPerDay { get; set; } = 500;
    public decimal LocalTransportPerPersonPerDay { get; set; } = 300;
    // One-way bus fare per road km (used with the routed distance, doubled for the return).
    public decimal BusFarePerKm { get; set; } = 2.0m;
    public decimal AccommodationPerRoomPerNight { get; set; } = 2000;
    public int PersonsPerRoom { get; set; } = 2;
}
