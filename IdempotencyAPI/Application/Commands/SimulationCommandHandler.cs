using IdempotencyAPI.Application.IdempotencyService;
using IdempotencyAPI.Application.Model;
using MediatR;
using System.Text.Json;

namespace IdempotencyAPI.Application.Commands
{
    public class SimulationCommandHandler : IRequestHandler<SimulationCommand, bool>
    {
        private readonly IIdempotencyService _idempotencyService;

        public SimulationCommandHandler(IIdempotencyService idempotencyService)
        {
            _idempotencyService = idempotencyService;
        }


        public async Task<bool> Handle(SimulationCommand request, CancellationToken cancellationToken)
        {

            IdempotentResponse idempotentResponse = await _idempotencyService.HasProcessedAsync<IdempotentResponse>(request.IdempotencyKey);
            if (idempotentResponse != null) 
            {
                return true;
            }

            if (request.WeatherForecast.TemperatureC < 1)
            {
                return false;
            }

            await _idempotencyService.MarkAsProcessedAsync(request.IdempotencyKey, JsonSerializer.Serialize<bool>(true));


            return true;
        }
    }
}
