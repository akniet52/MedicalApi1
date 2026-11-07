using Microsoft.AspNetCore.Mvc;
using MedicalApi.Models;
using MedicalApi.Data;
namespace MedicalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<Patient>), 200)]
    public ActionResult<List<Patient>> GetAll([FromQuery] string? search)
    {
        lock (ClinicStore.Sync)
        {
            IEnumerable<Patient> query = ClinicStore.Patients;
            if (!string.IsNullOrWhiteSpace(search)) query = query.Where(p => p.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase));
            return Ok(query.ToList());
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Patient), 200)]
    [ProducesResponseType(404)]
    public ActionResult<Patient> GetById(int id)
    {
        lock (ClinicStore.Sync)
        {
            var item = ClinicStore.Patients.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound();
            return Ok(item);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(Patient), 201)]
    [ProducesResponseType(typeof(string), 400)]
    public ActionResult<Patient> Create(Patient item)
    {
        lock (ClinicStore.Sync)
        {
            var error = Validate(item);
            if (error != null) return BadRequest(error);
            
            item.Id = ClinicStore.NextPatientId++;
            
            ClinicStore.Patients.Add(item);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Patient), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(404)]
    public ActionResult<Patient> Update(int id, Patient item)
    {
        lock (ClinicStore.Sync)
        {
            var existing = ClinicStore.Patients.FirstOrDefault(x => x.Id == id);
            if (existing == null) return NotFound();
            var error = Validate(item, id);
            if (error != null) return BadRequest(error);
            
            existing.Name = item.Name;
            existing.BirthDate = item.BirthDate;
            existing.Phone = item.Phone;
            existing.Address = item.Address;
            existing.HasInsurance = item.HasInsurance;
            return Ok(existing);
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public IActionResult Delete(int id)
    {
        lock (ClinicStore.Sync)
        {
            var item = ClinicStore.Patients.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound();
            ClinicStore.Spravki.RemoveAll(s => s.PatientId == id);
            ClinicStore.Appointments.RemoveAll(a => a.PatientId == id);
            ClinicStore.Patients.Remove(item);
            return NoContent();
        }
    }

    [HttpGet("{id}/spravki")]
    [ProducesResponseType(typeof(List<Spravka>), 200)]
    [ProducesResponseType(404)]
    public ActionResult<List<Spravka>> GetSpravki(int id)
    {
        lock (ClinicStore.Sync)
        {
            if (!ClinicStore.Patients.Any(p => p.Id == id)) return NotFound();
            return Ok(ClinicStore.Spravki.Where(s => s.PatientId == id).ToList());
        }
    }

    private static string? Validate(Patient item, int excludeId = 0)
    {
        if (string.IsNullOrWhiteSpace(item.Name)) return "Укажите имя пациента";
        if (item.Name.Length > 150) return "Имя должно содержать не больше 150 символов";
        if (item.BirthDate == default || item.BirthDate.Date > DateTime.Today || item.BirthDate.Date < DateTime.Today.AddYears(-130)) return "Укажите дату рождения за последние 130 лет, не позднее сегодняшней";
        if (string.IsNullOrWhiteSpace(item.Phone) || item.Phone.Count(char.IsDigit) < 7 || item.Phone.Length > 25) return "Укажите телефон: минимум 7 цифр, не больше 25 символов";
        return null;
    }
}
