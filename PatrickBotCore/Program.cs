using System.Threading.Tasks;

namespace PatrickBotCore
{
    public class Program
    {
        public static Task Main(string[] args)
            =>  Startup.RunAsync(args);
    }
}
