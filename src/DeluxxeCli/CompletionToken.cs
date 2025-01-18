namespace DeluxxeCli;

public class CompletionToken(CancellationTokenSource source)
{
    public void Complete()
    {
        source.Cancel();
    }
}