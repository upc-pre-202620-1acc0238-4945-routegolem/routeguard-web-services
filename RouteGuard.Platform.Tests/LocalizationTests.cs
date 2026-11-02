using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using RouteGuard.Platform.IdentityAccessManagement.Domain.Model;
using RouteGuard.Platform.Shared.Interfaces.Rest.ProblemDetails;
using RouteGuard.Platform.Shared.Resources;
using RouteGuard.Platform.Shared.Resources.Errors;
using Xunit;

namespace RouteGuard.Platform.Tests;

public class DummyController : ControllerBase
{
    public DummyController()
    {
        ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }
}

public class LocalizationTests
{
    private readonly IServiceProvider _serviceProvider;

    public LocalizationTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddTransient<IStringLocalizer<ErrorMessages>, StringLocalizer<ErrorMessages>>();
        services.AddTransient<IStringLocalizer<CommonMessages>, StringLocalizer<CommonMessages>>();
        services.AddSingleton<Microsoft.AspNetCore.Mvc.Infrastructure.ProblemDetailsFactory, DefaultProblemDetailsFactory>();
        services.AddScoped<Shared.Interfaces.Rest.ProblemDetails.ProblemDetailsFactory>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Theory]
    [InlineData("es", "IamError.InvalidCredentials", "Correo o contraseña inválidos.")]
    [InlineData("en", "IamError.InvalidCredentials", "Invalid email or password.")]
    [InlineData("es", "IamError.UserNotFound", "Usuario no encontrado.")]
    [InlineData("en", "IamError.UserNotFound", "User not found.")]
    [InlineData("es", "FleetError.RouteNotFound", "La ruta especificada no fue encontrada.")]
    [InlineData("en", "FleetError.RouteNotFound", "The specified route was not found.")]
    public void ErrorMessages_ShouldReturnCorrectLocalizedText(string cultureName, string resourceKey, string expectedText)
    {
        var culture = new CultureInfo(cultureName);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        var localizer = _serviceProvider.GetRequiredService<IStringLocalizer<ErrorMessages>>();
        var localizedString = localizer[resourceKey];

        Assert.False(localizedString.ResourceNotFound, $"Resource key '{resourceKey}' should exist in culture '{cultureName}'");
        Assert.Equal(expectedText, localizedString.Value);
    }

    [Theory]
    [InlineData("es", "GenericError", "Ocurrió un error.")]
    [InlineData("en", "GenericError", "An error occurred.")]
    [InlineData("es", "InternalServerError", "Error interno del servidor.")]
    [InlineData("en", "InternalServerError", "Internal server error.")]
    public void CommonMessages_ShouldReturnCorrectLocalizedText(string cultureName, string resourceKey, string expectedText)
    {
        var culture = new CultureInfo(cultureName);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        var localizer = _serviceProvider.GetRequiredService<IStringLocalizer<CommonMessages>>();
        var localizedString = localizer[resourceKey];

        Assert.False(localizedString.ResourceNotFound, $"Resource key '{resourceKey}' should exist in culture '{cultureName}'");
        Assert.Equal(expectedText, localizedString.Value);
    }

    [Theory]
    [InlineData("es", IamError.InvalidCredentials, "Correo o contraseña inválidos.")]
    [InlineData("en", IamError.InvalidCredentials, "Invalid email or password.")]
    [InlineData("es", IamError.UserNotFound, "Usuario no encontrado.")]
    [InlineData("en", IamError.UserNotFound, "User not found.")]
    public void ProblemDetailsFactory_ShouldReturnLocalizedProblemDetails(string cultureName, IamError error, string expectedDetail)
    {
        var culture = new CultureInfo(cultureName);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        using var scope = _serviceProvider.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<Shared.Interfaces.Rest.ProblemDetails.ProblemDetailsFactory>();
        var controller = new DummyController();

        var actionResult = factory.CreateProblemDetails(controller, StatusCodes.Status400BadRequest, error, "fallback");
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        var problemDetails = Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(objectResult.Value);

        Assert.Equal(expectedDetail, problemDetails.Detail);
    }
}
