using Sapl.Demo.Data;

namespace Sapl.Demo.Services;

public sealed class PatientService : IPatientService
{
    public Task<object?> ListPatients(CancellationToken ct = default)
    {
        var result = PatientData.Patients.Select(p => new
        {
            p.Id,
            p.Name,
            p.Ssn,
            p.Diagnosis,
            p.Classification,
        }).ToArray();

        return Task.FromResult<object?>(result);
    }

    public Task<object?> FindPatient(string name, CancellationToken ct = default)
    {
        var result = PatientData.Patients
            .Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .Select(p => new { p.Id, p.Name, p.Ssn, p.Diagnosis, p.Classification })
            .ToArray();

        return Task.FromResult<object?>(result);
    }

    public Task<object?> SearchPatients(string query, CancellationToken ct = default)
    {
        var result = PatientData.Patients
            .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                     || p.Diagnosis.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Select(p => new { p.Id, p.Name, p.Ssn, p.Diagnosis, p.Classification })
            .ToArray();

        return Task.FromResult<object?>(result);
    }

    public Task<object?> GetPatientDetail(string id, CancellationToken ct = default)
    {
        var patient = PatientData.Patients.FirstOrDefault(p => p.Id == id);
        if (patient is null)
            return Task.FromResult<object?>(null);

        object? data = new { patient.Id, patient.Name, patient.Ssn, patient.Diagnosis, patient.Classification };
        return Task.FromResult<object?>(data);
    }

    public Task<object?> GetPatientSummary(string id, CancellationToken ct = default)
    {
        var patient = PatientData.Patients.FirstOrDefault(p => p.Id == id);
        if (patient is null)
            return Task.FromResult<object?>(null);

        object? data = new
        {
            patient.Id,
            patient.Name,
            patient.Ssn,
            patient.Diagnosis,
            patient.Classification,
            insurance = "INS-9876-XYZ",
        };

        return Task.FromResult<object?>(data);
    }

    public Task<object?> Transfer(double amount, CancellationToken ct = default)
    {
        return Task.FromResult<object?>(new { transferred = amount });
    }
}
