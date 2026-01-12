using G_StockVente.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace G_StockVente.Infrastructure.Services;

/// <summary>
/// Interface pour le service d'envoi d'emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envoyer un email simple
    /// </summary>
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envoyer un email à plusieurs destinataires
    /// </summary>
    Task<bool> SendEmailAsync(List<string> to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envoyer un email avec pièce jointe
    /// </summary>
    Task<bool> SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath, string attachmentName, bool isHtml = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envoyer un rapport par email (format HTML avec données)
    /// </summary>
    Task<bool> SendReportAsync(string to, string subject, string reportHtml, string? attachmentPath = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service d'envoi d'emails via SMTP
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        return await SendEmailAsync(new List<string> { to }, subject, body, isHtml, cancellationToken);
    }

    public async Task<bool> SendEmailAsync(List<string> to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Email service is disabled. Email would have been sent to: {Recipients}", string.Join(", ", to));
            return true; // On retourne true car l'envoi est désactivé volontairement
        }

        if (!_settings.IsValid())
        {
            _logger.LogError("Email settings are invalid. Please check configuration.");
            return false;
        }

        try
        {
            // En mode développement, rediriger tous les emails
            if (_settings.DevelopmentMode && !string.IsNullOrWhiteSpace(_settings.DevelopmentEmail))
            {
                _logger.LogWarning("Development mode: Redirecting email from {OriginalRecipients} to {DevEmail}", 
                    string.Join(", ", to), _settings.DevelopmentEmail);
                to = new List<string> { _settings.DevelopmentEmail };
            }

            using var client = CreateSmtpClient();
            using var message = new MailMessage();

            // Expéditeur
            message.From = new MailAddress(_settings.FromEmail, _settings.FromName);
            
            // Destinataires
            foreach (var recipient in to)
            {
                if (!string.IsNullOrWhiteSpace(recipient))
                {
                    message.To.Add(new MailAddress(recipient.Trim()));
                }
            }

            if (message.To.Count == 0)
            {
                _logger.LogWarning("No valid recipients for email: {Subject}", subject);
                return false;
            }

            // Réponse à
            if (!string.IsNullOrWhiteSpace(_settings.ReplyToEmail))
            {
                message.ReplyToList.Add(new MailAddress(_settings.ReplyToEmail));
            }

            // Sujet et corps
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = isHtml;

            // Encodage
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.SubjectEncoding = System.Text.Encoding.UTF8;

            // Priorité (normale par défaut)
            message.Priority = MailPriority.Normal;

            await client.SendMailAsync(message, cancellationToken);

            _logger.LogInformation("Email sent successfully to: {Recipients}, Subject: {Subject}", 
                string.Join(", ", message.To.Select(t => t.Address)), subject);

            return true;
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error while sending email to {Recipients}: {Message}", 
                string.Join(", ", to), ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Recipients}: {Message}", 
                string.Join(", ", to), ex.Message);
            return false;
        }
    }

    public async Task<bool> SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath, string attachmentName, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Email service is disabled. Email with attachment would have been sent to: {Recipient}", to);
            return true;
        }

        if (!File.Exists(attachmentPath))
        {
            _logger.LogError("Attachment file not found: {FilePath}", attachmentPath);
            return false;
        }

        try
        {
            using var client = CreateSmtpClient();
            using var message = new MailMessage();

            message.From = new MailAddress(_settings.FromEmail, _settings.FromName);
            message.To.Add(new MailAddress(to));
            
            if (!string.IsNullOrWhiteSpace(_settings.ReplyToEmail))
            {
                message.ReplyToList.Add(new MailAddress(_settings.ReplyToEmail));
            }

            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = isHtml;
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.SubjectEncoding = System.Text.Encoding.UTF8;

            // Ajouter la pièce jointe
            var attachment = new Attachment(attachmentPath, MediaTypeNames.Application.Octet);
            attachment.ContentDisposition.FileName = attachmentName;
            attachment.ContentDisposition.CreationDate = File.GetCreationTime(attachmentPath);
            attachment.ContentDisposition.ModificationDate = File.GetLastWriteTime(attachmentPath);
            message.Attachments.Add(attachment);

            await client.SendMailAsync(message, cancellationToken);

            _logger.LogInformation("Email with attachment sent successfully to: {Recipient}, Subject: {Subject}, Attachment: {Attachment}", 
                to, subject, attachmentName);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email with attachment to {Recipient}: {Message}", to, ex.Message);
            return false;
        }
    }

    public async Task<bool> SendReportAsync(string to, string subject, string reportHtml, string? attachmentPath = null, CancellationToken cancellationToken = default)
    {
        // Créer le corps HTML complet du rapport
        var fullHtml = $@"
<!DOCTYPE html>
<html lang=""fr"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{subject}</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 800px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f5f5f5;
        }}
        .container {{
            background-color: #ffffff;
            border-radius: 8px;
            padding: 30px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .header {{
            border-bottom: 2px solid #3481c0;
            padding-bottom: 20px;
            margin-bottom: 30px;
        }}
        .header h1 {{
            color: #3481c0;
            margin: 0;
            font-size: 24px;
        }}
        .footer {{
            margin-top: 40px;
            padding-top: 20px;
            border-top: 1px solid #e0e0e0;
            font-size: 12px;
            color: #666;
            text-align: center;
        }}
        table {{
            width: 100%;
            border-collapse: collapse;
            margin: 20px 0;
        }}
        th, td {{
            padding: 12px;
            text-align: left;
            border-bottom: 1px solid #e0e0e0;
        }}
        th {{
            background-color: #f8f9fa;
            font-weight: 600;
            color: #3481c0;
        }}
        .badge {{
            display: inline-block;
            padding: 4px 8px;
            border-radius: 4px;
            font-size: 12px;
            font-weight: 600;
        }}
        .badge-success {{
            background-color: #d4edda;
            color: #155724;
        }}
        .badge-warning {{
            background-color: #fff3cd;
            color: #856404;
        }}
        .badge-danger {{
            background-color: #f8d7da;
            color: #721c24;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>{_settings.ApplicationName}</h1>
            <p style=""margin: 5px 0; color: #666;"">{subject}</p>
            <p style=""margin: 5px 0; font-size: 14px; color: #999;"">Généré le {DateTime.UtcNow:dd/MM/yyyy à HH:mm} UTC</p>
        </div>
        
        <div class=""content"">
            {reportHtml}
        </div>
        
        <div class=""footer"">
            <p>Cet email a été généré automatiquement par le système {_settings.ApplicationName}.</p>
            <p>Pour toute question, contactez votre administrateur système.</p>
        </div>
    </div>
</body>
</html>";

        if (!string.IsNullOrEmpty(attachmentPath))
        {
            return await SendEmailWithAttachmentAsync(to, subject, fullHtml, attachmentPath, Path.GetFileName(attachmentPath), true, cancellationToken);
        }
        else
        {
            return await SendEmailAsync(to, subject, fullHtml, true, cancellationToken);
        }
    }

    /// <summary>
    /// Créer et configurer le client SMTP
    /// </summary>
    private SmtpClient CreateSmtpClient()
    {
        var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
        {
            EnableSsl = _settings.UseSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword),
            Timeout = _settings.Timeout,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        // Configuration pour StartTLS
        if (_settings.UseStartTls && !_settings.UseSsl)
        {
            // Note: .NET utilise EnableSsl pour StartTLS aussi, donc si UseStartTls est true, on active SSL
            client.EnableSsl = true;
        }

        return client;
    }
}

