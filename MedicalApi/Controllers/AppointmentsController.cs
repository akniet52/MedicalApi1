using Microsoft.AspNetCore.Mvc;
using MedicalApi.Models;
using MedicalApi.Data;
namespace MedicalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<Appointment>), 200)]
    public ActionResult<List<Appointment>> GetAll([FromQuery] int? patientId)
    {
        lock (ClinicStore.Sync)
        {
            IEnumerable<Appointment> query = ClinicStore.Appointments;
            if (patientId.HasValue) query = query.Where(a => a.PatientId == patientId.Value);
            return Ok(query.ToList());
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Appointment), 200)]
    [ProducesResponseType(404)]
    public ActionResult<Appointment> GetById(int id)
    {
        lock (ClinicStore.Sync)
        {
            var item = ClinicStore.Appointments.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound();
            return Ok(item);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(Appointment), 201)]
    [ProducesResponseType(typeof(string), 400)]
    public ActionResult<Appointment> Create(Appointment item)
    {
        lock (ClinicStore.Sync)
        {
            var error = Validate(item);
            if (error != null) return BadRequest(error);
            
            item.Id = ClinicStore.NextAppointmentId++;
            
            ClinicStore.Appointments.Add(item);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Appointment), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(404)]
    public ActionResult<Appointment> Update(int id, Appointment item)
    {
        lock (ClinicStore.Sync)
        {
            var existing = ClinicStore.Appointments.FirstOrDefault(x => x.Id == id);
            if (existing == null) return NotFound();
            var error = Validate(item, id);
            if (error != null) return BadRequest(error);
            
            existing.PatientId = item.PatientId;
            existing.DoctorName = item.DoctorName;
            existing.Specialty = item.Specialty;
            existing.StartsAt = item.StartsAt;
            existing.DurationMinutes = item.DurationMinutes;
            existing.Notes = item.Notes;
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
            var item = ClinicStore.Appointments.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound();
            
            ClinicStore.Appointments.Remove(item);
            return NoContent();
        }
    }

    private static string? Validate(Appointment item, int excludeId = 0)
    {
        if (!ClinicStore.Patients.Any(p => p.Id == item.PatientId)) return "Пациент не найден. Укажите существующий PatientId";
        if (string.IsNullOrWhiteSpace(item.DoctorName) || item.DoctorName.Length > 150) return "Укажите имя врача длиной до 150 символов";
        if (string.IsNullOrWhiteSpace(item.Specialty)) return "Укажите специальность врача";
        if (item.StartsAt.Kind != DateTimeKind.Utc) return "Укажите время в UTC, с окончанием Z, например 2027-01-15T09:00:00Z";
        if (item.StartsAt <= DateTime.UtcNow || item.StartsAt > DateTime.UtcNow.AddYears(1)) return "Выберите будущее время в пределах года";
        if (item.DurationMinutes < 10 || item.DurationMinutes > 180) return "Продолжительность приёма должна быть от 10 до 180 минут";
        if (ClinicStore.Appointments.Any(a => a.Id != excludeId && (string.Equals(a.DoctorName.Trim(), item.DoctorName.Trim(), StringComparison.OrdinalIgnoreCase) || a.PatientId == item.PatientId) && a.StartsAt < item.StartsAt.AddMinutes(item.DurationMinutes) && item.StartsAt < a.StartsAt.AddMinutes(a.DurationMinutes))) return "Врач или пациент уже занят в выбранное время. Выберите другое время";
        return null;
    }
}
