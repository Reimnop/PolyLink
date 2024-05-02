namespace PolyLink.Server.Util;

public class PeriodicAsyncTimer(float period, Func<Task> callback)
{
    private float accumulator;
    
    public async Task UpdateAsync(float delta)
    {
        accumulator += delta;
        while (accumulator >= period)
        {
            accumulator -= period;
            await callback();
        }
    }
}