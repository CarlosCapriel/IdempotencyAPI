namespace IdempotencyAPI.Application.Model
{
    public class IdempotentResponse
    {
        public int StatusCode { get; set; }
        public string Body { get; set; }
    }
}
