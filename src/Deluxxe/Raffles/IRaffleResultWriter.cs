namespace Deluxxe.Raffles;

public interface IRaffleResultWriter
{
    public Task<bool> WriteAsync(RaffleResult result, CancellationToken cancellationToken);
}