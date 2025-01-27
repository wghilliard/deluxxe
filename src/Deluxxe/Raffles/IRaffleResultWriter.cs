namespace Deluxxe.Raffles;

public interface IRaffleResultWriter
{
    public Task<Uri> WriteAsync(RaffleResult result, CancellationToken cancellationToken);
}