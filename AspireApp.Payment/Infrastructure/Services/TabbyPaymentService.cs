using AspireApp.Payment.Domain.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace AspireApp.Payment.Infrastructure.Services;

/// <summary>
/// Service for interacting with Tabby payment API
/// </summary>
public class TabbyPaymentService
{
    private readonly TabbyOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<TabbyPaymentService> _logger;

    public TabbyPaymentService(
        IOptions<TabbyOptions> options,
        HttpClient httpClient,
        ILogger<TabbyPaymentService> logger)
    {
        _options = options.Value;
        _httpClient = httpClient;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_options.BaseAddress);
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _options.Token);
    }

    /// <summary>
    /// Creates a Tabby checkout session
    /// </summary>
    public async Task<TabbySessionResponse> CreateSessionAsync(
        decimal amount,
        string currency,
        string buyerPhone,
        string buyerEmail,
        string buyerName,
        string orderNumber,
        string productName,
        string? productImage,
        string successUrl,
        string failureUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new
            {
                payment = new
                {
                    amount = amount.ToString("F2"),
                    currency,
                    buyer = new
                    {
                        phone = buyerPhone,
                        email = buyerEmail,
                        name = buyerName
                    },
                    order = new
                    {
                        reference_id = orderNumber,
                        items = new[]
                        {
                            new
                            {
                                title = productName,
                                quantity = 1,
                                unit_price = amount.ToString("F2"),
                                category = "general",
                                image_url = productImage
                            }
                        }
                    },
                    buyer_history = new
                    {
                        registered_since = DateTime.UtcNow.AddYears(-1).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        loyalty_level = 0
                    }
                },
                lang = "en",
                merchant_code = _options.MerchantCode,
                merchant_urls = new
                {
                    success = successUrl,
                    cancel = failureUrl,
                    failure = failureUrl
                }
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("checkout", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Tabby API error: {StatusCode} - {Content}",
                    response.StatusCode, responseContent);
                throw new HttpRequestException($"Tabby API error: {responseContent}");
            }

            var sessionResponse = JsonConvert.DeserializeObject<TabbySessionResponse>(responseContent);

            _logger.LogInformation(
                "Created Tabby session {SessionId} for order {OrderNumber}",
                sessionResponse?.Id, orderNumber);

            return sessionResponse!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Tabby session for order {OrderNumber}", orderNumber);
            throw;
        }
    }

    /// <summary>
    /// Gets a Tabby payment by ID
    /// </summary>
    public async Task<TabbyPaymentResponse> GetPaymentAsync(
        string paymentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"payments/{paymentId}", cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Tabby API error retrieving payment: {StatusCode} - {Content}",
                    response.StatusCode, responseContent);
                throw new HttpRequestException($"Tabby API error: {responseContent}");
            }

            var paymentResponse = JsonConvert.DeserializeObject<TabbyPaymentResponse>(responseContent);
            return paymentResponse!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Tabby payment {PaymentId}", paymentId);
            throw;
        }
    }

    /// <summary>
    /// Captures an authorized Tabby payment
    /// </summary>
    public async Task<TabbyCaptureResponse> CapturePaymentAsync(
        string paymentId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new
            {
                amount = amount.ToString("F2")
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                $"payments/{paymentId}/captures",
                content,
                cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Tabby API error capturing payment: {StatusCode} - {Content}",
                    response.StatusCode, responseContent);
                throw new HttpRequestException($"Tabby API error: {responseContent}");
            }

            var captureResponse = JsonConvert.DeserializeObject<TabbyCaptureResponse>(responseContent);

            _logger.LogInformation("Captured Tabby payment {PaymentId}", paymentId);

            return captureResponse!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing Tabby payment {PaymentId}", paymentId);
            throw;
        }
    }

    /// <summary>
    /// Refunds a Tabby payment
    /// </summary>
    public async Task<TabbyRefundResponse> RefundPaymentAsync(
        string paymentId,
        decimal amount,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new
            {
                amount = amount.ToString("F2"),
                reason = reason ?? "Customer requested refund"
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                $"payments/{paymentId}/refunds",
                content,
                cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Tabby API error refunding payment: {StatusCode} - {Content}",
                    response.StatusCode, responseContent);
                throw new HttpRequestException($"Tabby API error: {responseContent}");
            }

            var refundResponse = JsonConvert.DeserializeObject<TabbyRefundResponse>(responseContent);

            _logger.LogInformation("Refunded Tabby payment {PaymentId}", paymentId);

            return refundResponse!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding Tabby payment {PaymentId}", paymentId);
            throw;
        }
    }
}

// Tabby API response models
public class TabbySessionResponse
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("payment")]
    public TabbyPaymentInfo Payment { get; set; } = null!;

    [JsonProperty("configuration")]
    public TabbyConfiguration Configuration { get; set; } = null!;
}

public class TabbyPaymentInfo
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("amount")]
    public string Amount { get; set; } = string.Empty;

    [JsonProperty("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;
}

public class TabbyConfiguration
{
    [JsonProperty("available_products")]
    public TabbyProduct[] AvailableProducts { get; set; } = Array.Empty<TabbyProduct>();
}

public class TabbyProduct
{
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("web_url")]
    public string WebUrl { get; set; } = string.Empty;
}

public class TabbyPaymentResponse
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("amount")]
    public string Amount { get; set; } = string.Empty;

    [JsonProperty("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;
}

public class TabbyCaptureResponse
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("amount")]
    public string Amount { get; set; } = string.Empty;
}

public class TabbyRefundResponse
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("amount")]
    public string Amount { get; set; } = string.Empty;
}

