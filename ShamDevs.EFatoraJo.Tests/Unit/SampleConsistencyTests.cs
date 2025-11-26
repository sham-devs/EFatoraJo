using EFatoraJoConsoleApp.Samples;
using EFatoraJoConsoleApp.Serialization;
using FluentAssertions;

namespace ShamDevs.EFatoraJo.Tests.Unit;

public class SampleConsistencyTests
{
    [Fact]
    public void Invoice_samples_should_roundtrip_through_parser()
    {
        var sampleTypes = new[] { "income", "general", "special" };

        foreach (var sample in sampleTypes)
        {
            var json = SampleProvider.GetSampleJson(sample);
            var parsed = InvoiceJsonParser.ParseInvoice(json);

            parsed.Should().NotBeNull();
            parsed.InvoiceNumber.Should().NotBeNullOrWhiteSpace();
            parsed.InvoiceDetails.Should().NotBeEmpty();
        }
    }

    [Fact]
    public void Return_sample_should_roundtrip_and_apply_return_signs()
    {
        var json = SampleProvider.GetSampleJson("return");
        var parsed = ReturnInvoiceJsonParser.Parse(json);

        parsed.Should().NotBeNull();
        parsed.OriginalInvoiceNumber.Should().NotBeNullOrWhiteSpace();
        parsed.ReturnInvoiceNumber.Should().NotBeNullOrWhiteSpace();
        parsed.ReturnReason.Should().NotBeNullOrWhiteSpace();

        parsed.OriginalInvoice.InvoiceTotals.TotalInvoiceAmount.Should().BeLessThan(0);
        parsed.OriginalInvoice.InvoiceDetails.All(d => d.Quantity < 0).Should().BeTrue();
    }
}
