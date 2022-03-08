using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyUtils.Web.Utils;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyUtils.MailService
{
    public enum ProviderMode
    {
        AwsSes_ByIAMRole,
        AwsSes_ByEnvironmentVariable,
        SendGrid
    }

    public interface IMailService
    {
        Task<bool> SendEmailAsync(System.Net.Mail.MailMessage mailMessage, CancellationToken cancellationToken = default);
    }

    public class MailService : IMailService
    {
        private readonly ILogger<MailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISendGridClient _sendGridClient;

        ProviderMode[] _providerModes;

        public MailService(
            ISendGridClient sendGridClient,
            ILogger<MailService> logger,
            IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _sendGridClient = sendGridClient ?? throw new ArgumentNullException(nameof(sendGridClient));
            _providerModes = _configuration.GetSection("MailProviders").Get<string[]>()
                .Select(x => Enum.TryParse(x, out ProviderMode parsedValue) ? parsedValue : (ProviderMode?)null)
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToArray();
        }

        public async Task<bool> SendEmailAsync(System.Net.Mail.MailMessage mailMessage, CancellationToken cancellationToken = default)
        {
            foreach(var providerMode in _providerModes)
            {
                switch (providerMode)
                {
                    case ProviderMode.AwsSes_ByIAMRole:
                        if (await TrySendEmailByAwsSESAsync(mailMessage, useAwsEnvIAMRole: true, cancellationToken: cancellationToken)) return true;
                        break;
                    case ProviderMode.AwsSes_ByEnvironmentVariable:
                        if (await TrySendEmailByAwsSESAsync(mailMessage, useAwsEnvIAMRole: false, cancellationToken: cancellationToken)) return true;
                        break;
                    case ProviderMode.SendGrid:
                        if (await TrySendEmailBySendGridAsync(mailMessage, cancellationToken)) return true;
                        break;
                    default:
                        break;
                }
            }
            //if (await TrySendEmailByAwsSESAsync(mailMessage, useAwsEnvIAMRole: true, cancellationToken: cancellationToken)) return true;
            //if (await TrySendEmailBySendGridAsync(mailMessage, cancellationToken)) return true;

            _logger.LogErrorWithCaller($"Failed to send email '{mailMessage.Subject}' from '{mailMessage.From}' to {mailMessage.To.Select(x => x.Address).JoinWithSeparator(" & ")}.");
            return false;
        }

        private async Task<bool> TrySendEmailByAwsSESAsync(System.Net.Mail.MailMessage mailMessage, bool useAwsEnvIAMRole, CancellationToken cancellationToken = default)
        {
            try
            {
                // see https://docs.aws.amazon.com/sdk-for-php/v3/developer-guide/guide_credentials_environment.html
                var AWS_ACCESS_KEY_ID = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
                var AWS_SECRET_ACCESS_KEY = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");

                if (!useAwsEnvIAMRole && (string.IsNullOrWhiteSpace(AWS_ACCESS_KEY_ID) || string.IsNullOrWhiteSpace(AWS_SECRET_ACCESS_KEY)))
                {
                    return false;
                }

                _logger.LogInformationWithCaller($"Using Amazon SES to send email '{mailMessage.Subject}' from '{mailMessage.From}' ...");

                string textBody = mailMessage.IsBodyHtml ? mailMessage.Body.FromHtmlToPlainText() : mailMessage.Body;

                using var client = useAwsEnvIAMRole ? new AmazonSimpleEmailServiceClient() : new AmazonSimpleEmailServiceClient(AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY);
                var sendRequest = new SendEmailRequest
                {
                    Source = mailMessage.From.ToString(),
                    Destination = new Destination(mailMessage.To.Select(x => x.ToString()).ToList()),
                    //{
                    //    ToAddresses =
                    //    new List<string> { receiverAddress }
                    //},
                    Message = new Message
                    {
                        Subject = new Amazon.SimpleEmail.Model.Content(mailMessage.Subject),
                        Body = new Body
                        {
                            Html = mailMessage.IsBodyHtml ? new Amazon.SimpleEmail.Model.Content
                            {
                                Charset = "UTF-8",
                                Data = mailMessage.Body
                            } : null,
                            Text = new Amazon.SimpleEmail.Model.Content
                            {
                                Charset = "UTF-8",
                                Data = textBody
                            }
                        }
                    },
                    // If you are not using a configuration set, comment
                    // or remove the following line 
                    //ConfigurationSetName = configSet
                };

                var response = await client.SendEmailAsync(sendRequest, cancellationToken).ConfigureAwait(false);
                //if ((int)response.HttpStatusCode < 400)
                if ((int)response.HttpStatusCode >= 200 && (int)response.HttpStatusCode <= 299)
                {
                    _logger.LogInformationWithCaller($"Successfully sent email '{mailMessage.Subject}' from '{mailMessage.From}' to {mailMessage.To.Select(x => x.Address).JoinWithSeparator(" & ")}.");
                    return true;
                }
                else
                {
                    _logger.LogInformationWithCaller($"Failed to send email '{mailMessage.Subject}' from '{mailMessage.From}' to {mailMessage.To.Select(x => x.Address).JoinWithSeparator(" & ")}. ({response.HttpStatusCode}){response.ResponseMetadata?.Metadata?.ToJson()}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithCaller(ex, $"Failed to send email '{mailMessage.Subject}' from '{mailMessage.From}' to {mailMessage.To.Select(x => x.Address).JoinWithSeparator(" & ")}.");
                return false;
            }
        }

        private async Task<bool> TrySendEmailBySendGridAsync(System.Net.Mail.MailMessage mailMessage, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformationWithCaller($"Using SendGrid to send email '{mailMessage.Subject}' from '{mailMessage.From}' to {mailMessage.To.Select(x => x.Address).JoinWithSeparator(" & ")} ...");

                string textBody = mailMessage.IsBodyHtml ? mailMessage.Body.FromHtmlToPlainText() : mailMessage.Body;
                SendGridMessage msg = new SendGridMessage()
                {
                    From = new EmailAddress(mailMessage.From.Address, mailMessage.From.DisplayName),
                    Subject = mailMessage.Subject,
                };

                msg.AddContent(MimeType.Text, textBody);
                if (mailMessage.IsBodyHtml)
                {
                    msg.AddContent(MimeType.Html, mailMessage.Body);
                }

                foreach (var recepient in mailMessage.To)
                {
                    msg.AddTo(new EmailAddress(recepient.Address, recepient.DisplayName));
                }

                //var response = await _sendGridClient.SendEmailAsync(msg, cancellationToken).ConfigureAwait(false);
                var response = await _sendGridClient.SendEmailAsync(msg, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformationWithCaller($"Successfully sent email '{mailMessage.Subject}' from '{mailMessage.From}' to {mailMessage.To.Select(x => x.Address).JoinWithSeparator(" & ")}.");
                    return true;
                }
                else
                {
                    var responseText = await response.Body.ReadAsStringAsync();
                    _logger.LogInformationWithCaller($"Failed to send email '{mailMessage.Subject}' from '{mailMessage.From}' to {mailMessage.To.Select(x => x.Address).JoinWithSeparator(" & ")}. ({response.StatusCode}) {responseText}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithCaller(ex, $"Failed to send email '{mailMessage.Subject}' from '{mailMessage.From}' to {mailMessage.To.Select(x => x.Address).JoinWithSeparator(" & ")}.");
                return false;
            }
        }
    }
}
