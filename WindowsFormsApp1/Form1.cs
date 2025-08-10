using ShamDevs.EFatoraJo;
using ShamDevs.EFatoraJo.Exceptions;
using ShamDevs.EFatoraJo.Utilities;
using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void Button1_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                // Generate and submit a standard invoice
                var invoice = RandomInvoiceGenerator.GenerateRandomInvoice();
                var invoiceResponse = await EFatoraJoSdk.SendFatoraAsync(
                    invoice,
                    "your_client_id",
                    "your_secret_key");

                // Handle response
                if (invoiceResponse.IsSuccessfullySubmitted())
                {
                    MessageBox.Show($"Invoice {invoiceResponse.InvoiceNumber} submitted successfully!\n" +
                                  $"QR Code: {invoiceResponse.Qr}",
                                  "Success",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Information);

                    // Save to database
                    //await SaveInvoiceToDatabaseAsync(
                    //    invoiceResponse.InvoiceNumber,
                    //    invoiceResponse.InvoiceUuid,
                    //    invoiceResponse.Qr);
                }
                else if (invoiceResponse.IsAlreadySubmitted())
                {
                    var message = new StringBuilder();
                    message.AppendLine($"Invoice {invoiceResponse.InvoiceNumber} was already submitted");

                    if (!string.IsNullOrEmpty(invoiceResponse.Qr))
                    {
                        message.AppendLine("\nThis invoice has a valid QR code");
                    }

                    MessageBox.Show(message.ToString(),
                                  "Duplicate Submission",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Warning);
                }
                else if (invoiceResponse.HasErrors())
                {
                    var errorMessage = new StringBuilder();
                    errorMessage.AppendLine("Submission failed with errors:");
                    errorMessage.AppendLine(invoiceResponse.GetFormattedErrors());

                    if (invoiceResponse.HasWarnings())
                    {
                        errorMessage.AppendLine();
                        errorMessage.AppendLine("Additional warnings:");
                        errorMessage.AppendLine(invoiceResponse.GetFormattedWarnings());
                    }

                    MessageBox.Show(errorMessage.ToString(),
                                  "Submission Failed",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Received unknown response status from API",
                                   "Unknown Status",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning);
                }
            }
            catch (InvoiceValidationException ex)
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine("Invoice validation failed:");
                foreach (var error in ex.ValidationErrors)
                {
                    errorMessage.AppendLine($"- {error}");
                }

                MessageBox.Show(errorMessage.ToString(),
                              "Validation Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
            catch (EInvoiceApiException ex) when (ex.StatusCode == 401)
            {
                MessageBox.Show("Authentication failed - please check your client ID and secret key",
                              "Authentication Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
            catch (EInvoiceApiException ex)
            {
                MessageBox.Show($"API Error ({ex.StatusCode}):\n{ex.ResponseContent}",
                              "API Communication Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
            catch (EInvoiceException ex)
            {
                MessageBox.Show($"E-Invoice Error: {ex.Message}",
                              "Processing Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}",
                              "System Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
        }

        private async void Button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Generate original invoice and its return
                var originalInvoice = RandomInvoiceGenerator.GenerateRandomInvoice();
                var returnInvoice = RandomInvoiceGenerator.GenerateRandomSalesReturnInvoice(originalInvoice);

                // Submit the return
                var returnResponse = await EFatoraJoSdk.SendReturnFatoraAsync(
                    returnInvoice,
                    "your_client_id",
                    "your_secret_key");

                // Handle response
                if (returnResponse.IsSuccessfullySubmitted())
                {
                    MessageBox.Show($"Return for invoice {returnInvoice.ReturnedInvoice.InvoiceNumber} was successful!\n" +
                                  $"Return Reference: {returnResponse.InvoiceNumber}",
                                  "Return Processed",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Information);

                    // Update original invoice status in database
                    //await UpdateInvoiceStatusAsync(
                    //    returnInvoice.ReturnedInvoice.InvoiceNumber,
                    //    "RETURNED",
                    //    returnResponse.InvoiceNumber);
                }
                else if (returnResponse.HasErrors())
                {
                    var errorDetails = new StringBuilder();
                    errorDetails.AppendLine("Return processing failed:");
                    errorDetails.AppendLine(returnResponse.GetFormattedErrors());

                    if (returnResponse.HasWarnings())
                    {
                        errorDetails.AppendLine();
                        errorDetails.AppendLine("Additional warnings:");
                        errorDetails.AppendLine(returnResponse.GetFormattedWarnings());
                    }

                    MessageBox.Show(errorDetails.ToString(),
                                  "Return Failed",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);

                    // Log full error details
                    //Logger.LogError($"Return failed for {returnInvoice.InvoiceNumber}\n" +
                    //               $"Errors: {string.Join("\n", returnResponse.Results.Errors.Select(err => err.Message))}");
                }
                else if (returnResponse.IsAlreadySubmitted())
                {
                    MessageBox.Show($"This return was already processed\n" +
                                  $"Original return reference: {returnResponse.InvoiceNumber}",
                                  "Duplicate Return",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Unexpected response from return processing",
                                  "Unknown Status",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Warning);
                }
            }
            catch (InvoiceValidationException ex)
            {
                var validationErrors = string.Join("\n", ex.ValidationErrors
                    .Select((err, i) => $"{i + 1}. {err}"));

                MessageBox.Show($"Original invoice validation failed:\n{validationErrors}",
                              "Validation Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
            catch (EInvoiceApiException ex) when (ex.StatusCode == 404)
            {
                MessageBox.Show("The original invoice was not found in the system",
                              "Invalid Return",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
            catch (EInvoiceApiException ex)
            {
                MessageBox.Show($"API Communication Error ({ex.StatusCode}):\n{ex.Message}",
                              "API Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
            catch (EInvoiceException ex)
            {
                MessageBox.Show($"Return processing error: {ex.Message}",
                              "Processing Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"System error during return processing: {ex.Message}",
                              "System Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
                //Logger.LogCritical(ex);
            }
        }
    }
}
