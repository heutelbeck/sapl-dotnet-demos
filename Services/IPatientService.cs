using Sapl.Core.Attributes;
using Sapl.Core.Subscription;

namespace Sapl.Demo.Services;

public interface IPatientService
{
    [PreEnforce(Action = "listPatients", Resource = "patients")]
    Task<object?> ListPatients(CancellationToken ct = default);

    [PreEnforce(Action = "findPatient", Resource = "patient")]
    Task<object?> FindPatient(string name, CancellationToken ct = default);

    [PreEnforce(Action = "searchPatients", Resource = "patientSearch")]
    Task<object?> SearchPatients(string query, CancellationToken ct = default);

    // Policy matches on resource.type == "patientDetail" (a JSON object).
    // C# attributes only accept constants, so a customizer builds the structured resource.
    [PostEnforce(Action = "getPatientDetail", Customizer = typeof(PatientDetailCustomizer))]
    Task<object?> GetPatientDetail(string id, CancellationToken ct = default);

    [PreEnforce(Action = "getPatientSummary", Resource = "patientSummary")]
    Task<object?> GetPatientSummary(string id, CancellationToken ct = default);

    [PreEnforce(Action = "transfer", Resource = "account")]
    Task<object?> Transfer(double amount, CancellationToken ct = default);
}

internal sealed class PatientDetailCustomizer : ISubscriptionCustomizer
{
    public void Customize(SubscriptionContext context, SubscriptionBuilder builder)
    {
        builder.WithStaticResource(new { type = "patientDetail" });
    }
}
