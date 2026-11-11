// [GT] File này gom các lệnh using dùng chung cho cả project, nên các file khác không phải viết lại.
// [GT] (ImplicitUsings của .NET đã tự thêm System, System.Linq, System.IO... rồi.)
global using EduHub.Ch02.Abstractions;   // [GT] các interface (hợp đồng)
global using EduHub.Ch02.Domain;         // [GT] Club, Member, Event
global using EduHub.Ch02.Infrastructure; // [GT] class cụ thể: logger, notification, repository
global using EduHub.Ch02.Patterns;       // [GT] Observer, Strategy
global using EduHub.Ch02.Services;       // [GT] ClubService, EventService, ReportService
