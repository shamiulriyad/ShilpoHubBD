using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Services.Auth;
using ShilpoHubBD.Application.Services.Commerce;
using ShilpoHubBD.Application.Services.Marketplace;
using ShilpoHubBD.Application.Services.Reviews;

namespace ShilpoHubBD.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoleService, RoleService>();

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IDistrictService, DistrictService>();
        services.AddScoped<ICraftStoryService, CraftStoryService>();
        services.AddScoped<IProducerStoryService, ProducerStoryService>();
        services.AddScoped<IWorkshopGalleryService, WorkshopGalleryService>();

        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IReviewService, ReviewService>();

        return services;
    }
}
