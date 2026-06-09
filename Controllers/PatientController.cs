using Microsoft.AspNetCore.Mvc;
using Sapl.Core.Attributes;
using Sapl.Demo.Data;
using Sapl.Demo.Services;

namespace Sapl.Demo.Controllers;

[ApiController]
[Route("api")]
public sealed class PatientController(IPatientService patientService) : ControllerBase
{
    [HttpGet("patient/{id}")]
    [PreEnforce(Action = "readPatient", Resource = "patient")]
    public IActionResult GetPatient(string id)
    {
        var patient = PatientData.Patients.FirstOrDefault(p => p.Id == id);
        if (patient is null)
            return NotFound();
        return Ok(new { patient.Name, patient.Ssn, patient.Diagnosis, email = "jane.doe@example.com" });
    }

    [HttpGet("patients")]
    [PostEnforce(Action = "readPatients", Resource = "patients")]
    public IActionResult GetPatients()
    {
        return Ok(PatientData.Patients.Select(p => new
        {
            p.Id,
            p.Name,
            p.Ssn,
            p.Diagnosis,
            p.Classification,
        }));
    }

    [HttpPost("transfer")]
    [PreEnforce(Action = "transfer", Resource = "account")]
    public IActionResult Transfer([FromQuery] double amount)
    {
        return Ok(new { transferred = amount });
    }

    // Service-layer endpoints: enforcement is on the IPatientService methods, not here.

    [HttpGet("services/patients")]
    public async Task<IActionResult> ServiceListPatients() =>
        Ok(await patientService.ListPatients(HttpContext.RequestAborted));

    [HttpGet("services/patients/find")]
    public async Task<IActionResult> ServiceFindPatient([FromQuery] string name) =>
        Ok(await patientService.FindPatient(name, HttpContext.RequestAborted));

    [HttpGet("services/patients/search")]
    public async Task<IActionResult> ServiceSearchPatients([FromQuery] string q) =>
        Ok(await patientService.SearchPatients(q, HttpContext.RequestAborted));

    [HttpGet("services/patients/{id}/summary")]
    public async Task<IActionResult> ServiceGetPatientSummary(string id)
    {
        var result = await patientService.GetPatientSummary(id, HttpContext.RequestAborted);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("services/patients/{id}")]
    public async Task<IActionResult> ServiceGetPatientDetail(string id)
    {
        var result = await patientService.GetPatientDetail(id, HttpContext.RequestAborted);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("services/transfer")]
    public async Task<IActionResult> ServiceTransfer([FromQuery] double amount) =>
        Ok(await patientService.Transfer(amount, HttpContext.RequestAborted));
}
