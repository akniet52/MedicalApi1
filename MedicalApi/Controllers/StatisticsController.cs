using Microsoft.AspNetCore.Mvc;
using MedicalApi.Data;
namespace MedicalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(StatisticsResponse), 200)]
    public ActionResult<StatisticsResponse> Get()
    {
        lock (ClinicStore.Sync)
        {
            var spravki = ClinicStore.Spravki;
            return Ok(new StatisticsResponse
            {
                PatientsCount = ClinicStore.Patients.Count,
                SpravkiCount = spravki.Count,
                SpravkaTypesCount = ClinicStore.Types.Count,
                AppointmentsCount = ClinicStore.Appointments.Count,
                TotalAmount = spravki.Sum(s => s.Price),
                AveragePrice = spravki.Count == 0 ? 0 : spravki.Average(s => s.Price),
                ByType = spravki.GroupBy(s => s.SpravkaTypeId).Select(g => new TypeStatistics
                {
                    SpravkaTypeId = g.Key, Count = g.Count(), Amount = g.Sum(s => s.Price)
                }).ToList()
            });
        }
    }
}

public class StatisticsResponse
{
    public int PatientsCount { get; set; }
    public int SpravkiCount { get; set; }
    public int SpravkaTypesCount { get; set; }
    public int AppointmentsCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AveragePrice { get; set; }
    public List<TypeStatistics> ByType { get; set; } = new();
}
public class TypeStatistics
{
    public int SpravkaTypeId { get; set; }
    public int Count { get; set; }
    public decimal Amount { get; set; }
}
