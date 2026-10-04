// ============================================================================
//  NotificationServices.cs — سه پیاده‌سازی مختلف از INotificationService
// ----------------------------------------------------------------------------
//  این سه کلاس، سه استراتژی مختلف برای ارسال اعلان پیاده‌سازی می‌کنند:
//   - EmailNotificationService  (با کلید "email")
//   - SmsNotificationService    (با کلید "sms")
//   - PushNotificationService   (با کلید "push")
//
//  نکته آموزشی: هر سه کلاس یک اینترفیس را پیاده‌سازی می‌کنند،
//  اما با کلیدهای مختلف در Program.cs ثبت می‌شوند.
// ============================================================================

namespace DIEduDemo.Services;

// ----------------------------------------------------------------------------
// ۱) Email Notification
// ----------------------------------------------------------------------------
/// <summary>
/// سرویس اعلان از طریق ایمیل.
/// در Program.cs با AddKeyedScoped<INotificationService, EmailNotificationService>("email") ثبت می‌شود.
/// </summary>
public class EmailNotificationService : INotificationService
{
    private readonly ILogger<EmailNotificationService> _logger;

    /// <summary>
    /// Constructor Injection: سرویس داخلی ILogger را می‌گیرد.
    /// </summary>
    public EmailNotificationService(ILogger<EmailNotificationService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public string ChannelName => "Email";

    /// <inheritdoc />
    public string Send(string recipient, string message)
    {
        _logger.LogInformation("Email sent to {Recipient}", recipient);
        return $"📧 ایمیل به {recipient} ارسال شد. متن: {message}";
    }
}

// ----------------------------------------------------------------------------
// ۲) SMS Notification
// ----------------------------------------------------------------------------
/// <summary>
/// سرویس اعلان از طریق پیامک.
/// با کلید "sms" ثبت می‌شود.
/// </summary>
public class SmsNotificationService : INotificationService
{
    private readonly ILogger<SmsNotificationService> _logger;

    public SmsNotificationService(ILogger<SmsNotificationService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public string ChannelName => "SMS";

    /// <inheritdoc />
    public string Send(string recipient, string message)
    {
        _logger.LogInformation("SMS sent to {Recipient}", recipient);
        return $"📱 پیامک به {recipient} ارسال شد. متن: {message}";
    }
}

// ----------------------------------------------------------------------------
// ۳) Push Notification
// ----------------------------------------------------------------------------
/// <summary>
/// سرویس اعلان از طریق Push Notification.
/// با کلید "push" ثبت می‌شود.
/// </summary>
public class PushNotificationService : INotificationService
{
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(ILogger<PushNotificationService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public string ChannelName => "Push";

    /// <inheritdoc />
    public string Send(string recipient, string message)
    {
        _logger.LogInformation("Push sent to {Recipient}", recipient);
        return $"🔔 Push Notification به {recipient} ارسال شد. متن: {message}";
    }
}
