namespace RestrictRService
{
    public class SystemClock : IClock
    {
        public DateTime Now => DateTime.Now;
    }
}
