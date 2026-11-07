using MedicalApi.Models;
namespace MedicalApi.Data;

// Учебное хранилище в памяти. Один замок защищает связанные операции.
public static class ClinicStore
{
    public static readonly object Sync = new();
    public static List<Patient> Patients = new()
    {
        new() { Id = 1, Name = "Алиса Иванова", BirthDate = new(2000, 5, 15), Phone = "+996700123456", Address = "Бишкек", HasInsurance = true },
        new() { Id = 2, Name = "Тимур Асанов", BirthDate = new(1995, 2, 10), Phone = "+996555123456", Address = "Бишкек" }
    };
    public static List<SpravkaType> Types = new()
    {
        new() { Id = 1, Name = "Справка 083", Price = 100, ValidityDays = 180, RequiresExamination = true },
        new() { Id = 2, Name = "Санитарная книжка", Price = 750, ValidityDays = 365, RequiresExamination = true },
        new() { Id = 3, Name = "Справка для занятий плаванием", Price = 350, ValidityDays = 90, RequiresExamination = true },
        new() { Id = 4, Name = "Справка для участия в марафоне", Price = 500, ValidityDays = 30, RequiresExamination = true }
    };
    public static List<Spravka> Spravki = new()
    {
        new() { Id = 1, PatientId = 1, SpravkaTypeId = 3, Name = "Справка для занятий плаванием", Price = 350, IssuedAt = DateTime.UtcNow },
        new() { Id = 2, PatientId = 2, SpravkaTypeId = 4, Name = "Справка для участия в марафоне", Price = 500, IssuedAt = DateTime.UtcNow }
    };
    public static List<Appointment> Appointments = new();
    // Счётчики никогда не уменьшаются при удалении.
    public static int NextPatientId = 3;
    public static int NextSpravkaId = 3;
    public static int NextTypeId = 5;
    public static int NextAppointmentId = 1;
}
