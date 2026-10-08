using RouteGuard.Platform.Shared.Domain.Model.Entities;

namespace RouteGuard.Platform.NotificationsCommunication.Domain.Model.Entities;

/// <summary>Text template of a notification type and locale, with <c>{name}</c> placeholders (table <c>notification_templates</c>).</summary>
public class NotificationTemplate : IAuditableEntity
{
    protected NotificationTemplate()
    {
    }

    public NotificationTemplate(string type, string locale, string titleTemplate, string bodyTemplate)
    {
        Id = Guid.NewGuid();
        Type = type;
        Locale = locale;
        TitleTemplate = titleTemplate;
        BodyTemplate = bodyTemplate;
    }

    public Guid Id { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Locale { get; private set; } = "es-PE";
    public string TitleTemplate { get; private set; } = string.Empty;
    public string BodyTemplate { get; private set; } = string.Empty;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public string RenderTitle(IReadOnlyDictionary<string, string> values) => Render(TitleTemplate, values);

    public string RenderBody(IReadOnlyDictionary<string, string> values) => Render(BodyTemplate, values);

    private static string Render(string template, IReadOnlyDictionary<string, string> values) =>
        values.Aggregate(template, (text, pair) => text.Replace("{" + pair.Key + "}", pair.Value));
}
