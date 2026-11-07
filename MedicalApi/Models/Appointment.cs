namespace MedicalApi.Models;

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string DoctorName { get; set; } = "";
    public string Specialty { get; set; } = "";
    public DateTime StartsAt { get; set; }
    public int DurationMinutes { get; set; }
    public string Notes { get; set; } = "";
}
