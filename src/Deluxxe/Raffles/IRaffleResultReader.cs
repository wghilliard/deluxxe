namespace Deluxxe.Raffles;

public interface IRaffleResultReader
{
    public Task<RaffleResult> ReadAsync(Uri uri, CancellationToken cancellationToken);
    
    public Task<RaffleResult> ReadCurrentContextAsync(CancellationToken cancellationToken);
}