<<<<<<< HEAD
﻿using ShamDevs.EFatoraJo.Exceptions;
=======
﻿using Newtonsoft.Json;
using ShamDevs.EFatoraJo.Exceptions;
>>>>>>> 22f095a040cb70b6315767ed1773569f2609b27d
using ShamDevs.EFatoraJo.Models;
using ShamDevs.EFatoraJo.Models.Responses;
using ShamDevs.EFatoraJo.Services;
using ShamDevs.EFatoraJo.Utilities;
using System;
using System.Net.Http;
using System.Text;
<<<<<<< HEAD
using System.Text.Json;
using System.Text.Json.Serialization;
=======
>>>>>>> 22f095a040cb70b6315767ed1773569f2609b27d
using System.Threading.Tasks;

namespace ShamDevs.EFatoraJo
{
    /// <summary>
    /// Main service class for interacting with Jordanian e-invoicing system (Jofotara)
    /// </summary>
    /// <remarks>
    /// <para>This class handles:</para>
    /// <list type="bullet">
    /// <item>Invoice validation according to ZATCA requirements</item>
    /// <item>UBL 2.1 document generation</item>
    /// <item>API communication with Jofotara backend</item>
    /// <item>Response handling and error reporting</item>
    /// </list>
    /// </remarks>
    public static class EFatoraJoSdk
    {
        /// <summary>
        /// Submits a standard invoice to Jofotara e-invoicing system
        /// </summary>
        /// <param name="invoice">The invoice to be processed and sent</param>
        /// <param name="clientId">API client ID provided by Jofotara</param>
        /// <param name="secretKey">API secret key provided by Jofotara</param>
        /// <returns>EInvoiceResponse containing submission results</returns>
        /// <exception cref="InvoiceValidationException">
        /// Thrown when invoice fails validation checks (contains detailed error messages)
        /// </exception>
        /// <exception cref="UblGenerationException">
        /// Thrown when UBL document generation fails
        /// </exception>
        /// <exception cref="EInvoiceSerializationException">
        /// Thrown when XML serialization/deserialization fails
        /// </exception>
        /// <exception cref="EInvoiceApiException">
        /// Thrown when API communication fails (contains HTTP status code and response)
        /// </exception>
        /// <exception cref="EInvoiceException">
        /// Thrown for other unexpected errors (wraps original exception)
        /// </exception>
        /// <example>
        /// <code>
        /// try 
        /// {
        ///     var response = await EFatoraJoSdk.SendFatoraAsync(invoice, "client123", "secret123");
        ///     if (response.EINV_STATUS == "SUBMITTED")
        ///     {
        ///         Console.WriteLine($"Invoice submitted: {response.EINV_NUM}");
        ///     }
        /// }
        /// catch (InvoiceValidationException ex)
        /// {
        ///     foreach (var error in ex.ValidationErrors)
        ///     {
        ///         Console.WriteLine($"Validation Error: {error}");
        ///     }
        /// }
        /// </code>
        /// </example>
        public static async Task<EInvoiceResponse> SendFatoraAsync(Invoice invoice, string clientId, string secretKey)
        {
            try
            {
                // Validate the incoming invoice data
                InvoiceValidator.ValidateInvoice(invoice);

                // Generate UBL 2.1 document using InvoiceGenerator
                var ublInvoice = InvoiceGeneratorService.GenerateUBL21(invoice);
                var ublXml = InvoiceHelper.SerializeUBL(ublInvoice);
                var encryptedXml = Convert.ToBase64String(Encoding.UTF8.GetBytes(ublXml));

                using var client = new HttpClient();
                client.BaseAddress = new Uri("https://backend.jofotara.gov.jo/core/invoices/");

                var request = new HttpRequestMessage(HttpMethod.Post, "");
                request.Headers.Add("Client-Id", clientId);
                request.Headers.Add("Secret-Key", secretKey);
                request.Headers.Add("Cookie", "stickounet=4fdb7136e666916d0e373058e9e5c44e|7480c8b0e4ce7933ee164081a50488f1");

                var jsonBody = $@"{{ ""invoice"": ""{encryptedXml}"" }}";
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new EInvoiceApiException(
                        statusCode: (int)response.StatusCode,
                        response: responseContent);
                }

<<<<<<< HEAD
                return JsonSerializer.Deserialize<EInvoiceResponse>(responseContent, JsonOptions)
=======
                return JsonConvert.DeserializeObject<EInvoiceResponse>(responseContent)
>>>>>>> 22f095a040cb70b6315767ed1773569f2609b27d
                    ?? throw new EInvoiceSerializationException("API returned empty or invalid response");
            }
            catch (EInvoiceException)
            {
                throw; // Re-throw our custom exceptions
            }
            catch (Exception ex)
            {
                throw new EInvoiceException("Failed to process invoice", ex);
            }
        }

        /// <summary>
        /// Submits a sales return invoice to Jofotara e-invoicing system
        /// </summary>
        /// <param name="invoice">The return invoice to be processed</param>
        /// <param name="clientId">API client ID provided by Jofotara</param>
        /// <param name="secretKey">API secret key provided by Jofotara</param>
        /// <returns>EInvoiceResponse containing submission results</returns>
        /// <exception cref="InvoiceValidationException">
        /// Thrown when the original invoice fails validation checks
        /// </exception>
        /// <exception cref="UblGenerationException">
        /// Thrown when return UBL document generation fails
        /// </exception>
        /// <exception cref="EInvoiceSerializationException">
        /// Thrown when XML serialization/deserialization fails
        /// </exception>
        /// <exception cref="EInvoiceApiException">
        /// Thrown when API communication fails (contains HTTP status code and response)
        /// </exception>
        /// <exception cref="EInvoiceException">
        /// Thrown for other unexpected errors (wraps original exception)
        /// </exception>
        /// <remarks>
        /// <para>The method will:</para>
        /// <list type="number">
        /// <item>Validate the original invoice from the return</item>
        /// <item>Generate a credit note UBL document</item>
        /// <item>Submit to Jofotara's returns endpoint</item>
        /// </list>
        /// </remarks>
        public static async Task<EInvoiceResponse> SendReturnFatoraAsync(SalesReturnInvoice invoice, string clientId, string secretKey)
        {
            try
            {
                // Validate the original invoice
                InvoiceValidator.ValidateInvoice(invoice.ReturnedInvoice);

                // Generate return UBL document
                var ublInvoice = ReturnInvoiceGeneratorService.GenerateReturnUBL21(invoice);
                var ublXml = InvoiceHelper.SerializeUBL(ublInvoice);
                var encryptedXml = Convert.ToBase64String(Encoding.UTF8.GetBytes(ublXml));

                using var client = new HttpClient();
                client.BaseAddress = new Uri("https://backend.jofotara.gov.jo/core/invoices/");

                var request = new HttpRequestMessage(HttpMethod.Post, "");
                request.Headers.Add("Client-Id", clientId);
                request.Headers.Add("Secret-Key", secretKey);
                request.Headers.Add("Cookie", "stickounet=4fdb7136e666916d0e373058e9e5c44e|7480c8b0e4ce7933ee164081a50488f1");

                var jsonBody = $@"{{ ""invoice"": ""{encryptedXml}"" }}";
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new EInvoiceApiException(
                        statusCode: (int)response.StatusCode,
                        response: responseContent);
                }

<<<<<<< HEAD
                return JsonSerializer.Deserialize<EInvoiceResponse>(responseContent, JsonOptions)
=======
                return JsonConvert.DeserializeObject<EInvoiceResponse>(responseContent)
>>>>>>> 22f095a040cb70b6315767ed1773569f2609b27d
                    ?? throw new EInvoiceSerializationException("API returned empty or invalid response");
            }
            catch (EInvoiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new EInvoiceException("Failed to process return invoice", ex);
            }
        }
<<<<<<< HEAD

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, // For snake_case properties
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Converters = { new JsonStringEnumConverter() }
        };
=======
>>>>>>> 22f095a040cb70b6315767ed1773569f2609b27d
    }
}