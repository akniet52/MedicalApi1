using Microsoft.AspNetCore.Mvc;
using MedicalApi.Models;
using MedicalApi.Data;
namespace MedicalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpravkaTypesController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<SpravkaType>), 200)]
    public ActionResult<List<SpravkaType>> GetAll()
    {
        lock (ClinicStore.Sync)
        {
            IEnumerable<SpravkaType> query = ClinicStore.Types;
            
            return Ok(query.ToList());
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SpravkaType), 200)]
    [ProducesResponseType(404)]
    public ActionResult<SpravkaType> GetById(int id)
    {
        lock (ClinicStore.Sync)
        {
            var item = ClinicStore.Types.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound();
            return Ok(item);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(SpravkaType), 201)]
    [ProducesResponseType(typeof(string), 400)]
    public ActionResult<SpravkaType> Create(SpravkaType item)
    {
        lock (ClinicStore.Sync)
        {
            var error = Validate(item);
            if (error != null) return BadRequest(error);
            
            item.Id = ClinicStore.NextTypeId++;
            
            ClinicStore.Types.Add(item);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SpravkaType), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(404)]
    public ActionResult<SpravkaType> Update(int id, SpravkaType item)
    {
        lock (ClinicStore.Sync)
        {
            var existing = ClinicStore.Types.FirstOrDefault(x => x.Id == id);
            if (existing == null) return NotFound();
            var error = Validate(item, id);
            if (error != null) return BadRequest(error);
            
            existing.Name = item.Name;
            existing.Price = item.Price;
            existing.ValidityDays = item.ValidityDays;
            existing.RequiresExamination = item.RequiresExamination;
            return Ok(existing);
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(404)]
    public IActionResult Delete(int id)
    {
        lock (ClinicStore.Sync)
        {
            var item = ClinicStore.Types.FirstOrDefault(x => x.Id == id);
            if (item == null) return NotFound();
            if (ClinicStore.Spravki.Any(s => s.SpravkaTypeId == id))
                return BadRequest("Вид используется в выданных справках. Сначала удалите эти справки");
            ClinicStore.Types.Remove(item);
            return NoContent();
        }
    }

    private static string? Validate(SpravkaType item, int excludeId = 0)
    {
        if (string.IsNullOrWhiteSpace(item.Name)) return "Укажите название вида справки";
        if (item.Name.Length > 150) return "Название должно содержать не больше 150 символов";
        if (item.Price <= 0) return "Цена должна быть больше 0";
        if (item.ValidityDays < 1 || item.ValidityDays > 3650) return "Срок действия должен быть от 1 до 3650 дней";
        return null;
    }
}
