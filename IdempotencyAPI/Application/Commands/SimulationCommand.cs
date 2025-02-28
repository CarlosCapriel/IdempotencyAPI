using MediatR;
using System.ComponentModel.DataAnnotations;

namespace IdempotencyAPI.Application.Commands
{
    public class SimulationCommand : IRequest<bool>
    {
        [Required(ErrorMessage = "El campo Nombre no puede estar vacío")]
        public string IdempotencyKey { get; set; }
        public WeatherForecast WeatherForecast { get; set; }
    }
}
