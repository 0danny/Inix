namespace Inix
{
    public class InixLogger
    {
        public static bool shouldLog = true;

        public static void log(object message)
        {
            if(shouldLog)
                Console.WriteLine($"[Inix]: {message}");
        }
    }
}
