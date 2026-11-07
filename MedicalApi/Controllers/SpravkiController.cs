using Microsoft.AspNetCore.Mvc;
using MedicalApi.Models;
using MedicalApi.Data;
namespace MedicalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpravkiController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<Spravka>), 200)]
    [ProducesResponseType(typeof(string), 400)]
    public ActionResult<List<Spravka>> GetAll([FromQuery] int? patientId, [FromQuery] string? sort)
    {
        lock (ClinicStore.Sync)
        {
            IEnumerable<Spravka> query = ClinicStore.Spravki;
            if (patientId.HasValue) query = query.Where(s => s.PatientId == patientId.Value);
            if (sort is not null && sort != "price" && sort != "priceDesc" && sort != "date") return BadRequest("Сортировка: price, priceDesc или date");
            query = sort switch { "price" => query.OrderBy(s => s.Price), "priceDesc" => query.OrderByDescending(s => s.Price), "date" => query.OrderByDescending(s => s.IssuedAt), _ => query };
            return Ok(query.ToList());
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Spravka), 200)]
    [ProducesResponseType(404)]
    public ActionResult<Spravka> GetById(int id)
    {
        lock (ClinicStore.Sync)
        {
            var item = ClinicStore.Spravki.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound();
            return Ok(item);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(Spravka), 201)]
    [ProducesResponseType(typeof(string), 400)]
    public ActionResult<Spravka> Create(Spravka item)
    {
        lock (ClinicStore.Sync)
        {
            var error = Validate(item);
            if (error != null) return BadRequest(error);
            var type = ClinicStore.Types.First(t => t.Id == item.SpravkaTypeId);
            item.Name = type.Name;
            item.Price = type.Price;
            item.Id = ClinicStore.NextSpravkaId++;
            item.IssuedAt = DateTime.UtcNow;
            ClinicStore.Spravki.Add(item);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Spravka), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(404)]
    public ActionResult<Spravka> Update(int id, Spravka item)
    {
        lock (ClinicStore.Sync)
        {
            var existing = ClinicStore.Spravki.FirstOrDefault(x => x.Id == id);
            if (existing == null) return NotFound();
            var error = Validate(item, id);
            if (error != null) return BadRequest(error);
            var type = ClinicStore.Types.First(t => t.Id == item.SpravkaTypeId);
            item.Name = type.Name;
            item.Price = type.Price;
            existing.PatientId = item.PatientId;
            existing.SpravkaTypeId = item.SpravkaTypeId;
            existing.Name = item.Name;
            existing.Price = item.Price;
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
            var item = ClinicStore.Spravki.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound();
            
            ClinicStore.Spravki.Remove(item);
            return NoContent();
        }
    }

    private static string? Validate(Spravka item, int excludeId = 0)
    {
        if (!ClinicStore.Patients.Any(p => p.Id == item.PatientId)) return "Пациент не найден. Сначала создайте пациента и укажите его PatientId";
        if (!ClinicStore.Types.Any(t => t.Id == item.SpravkaTypeId)) return "Вид справки не найден. Выберите SpravkaTypeId из каталога";
        return null;
    }
}
