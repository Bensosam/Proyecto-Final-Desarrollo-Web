using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Clinica.API.Hubs;

[Authorize(Policy = "HabitacionesConsultar")]
public class HabitacionesHub : Hub
{
}