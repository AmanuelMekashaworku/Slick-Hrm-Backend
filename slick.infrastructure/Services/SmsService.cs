using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using slick.Application.Services.Interfaces;

namespace slick.Infrastructure.Services
{
    public class SmsService : ISmsService
    {
        private readonly HttpClient _httpClient;
        private readonly SmsSettings _smsSettings;

        public SmsService(IOptions<SmsSettings> smsSettings, HttpClient httpClient)
        {
            _smsSettings = smsSettings.Value ?? throw new ArgumentNullException(nameof(smsSettings));
            _httpClient = httpClient;
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentNullException(nameof(phoneNumber));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message));

            // Example: generic JSON payload (adjust for your SMS provider)
            var payload = new
            {
                to = phoneNumber,
                from = _smsSettings.SenderId,
                body = message
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, _smsSettings.ApiUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload))
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // If provider needs authentication (e.g., Twilio Basic Auth)
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _smsSettings.ApiKey);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Failed to send SMS to {phoneNumber}. Status: {response.StatusCode}. Response: {content}");
            }
        }

        public async Task SendBulkSmsAsync(IEnumerable<string> phoneNumbers, string message)
        {
            foreach (var number in phoneNumbers)
            {
                await SendSmsAsync(number, message);
            }
        }

        public async Task SendVerificationSmsAsync(string phoneNumber, string verificationCode)
        {
            var msg = $"Your verification code is: {verificationCode}";
            await SendSmsAsync(phoneNumber, msg);
        }

        public async Task SendPasswordResetSmsAsync(string phoneNumber, string newPassword)
        {
            var msg = $"Your password has been reset. New password: {newPassword}";
            await SendSmsAsync(phoneNumber, msg);
        }
    }
}
