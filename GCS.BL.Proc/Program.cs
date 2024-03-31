using GCS.CL.Net;
using GCS.BL.Conv;
using log4net.Repository.Hierarchy;
using System.Diagnostics;
using log4net;

Console.WriteLine("GCS.BL.Proc Module Run!");

namespace GCS.BL.Proc
{
    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Entering Main() application entry point.");

            int millisecondsDelay = 2000;
            await CallingMethodAsync(millisecondsDelay);

            Console.WriteLine("Exiting Main() application entry point.");

            await Task.Delay(millisecondsDelay + 500);
        }

        static async Task CallingMethodAsync(int millisecondsDelay)
        {
            Console.WriteLine("  Entering calling method.");
            Console.WriteLine("  Returning from calling method.");

            // CalledMethodAsync 메서드를 호출합니다.
            await CalledMethodAsync(millisecondsDelay);
        }

        static async Task CalledMethodAsync(int millisecondsDelay)
        {
            Console.WriteLine("    Entering called method, starting and awaiting Task.Delay.");

            await Task.Delay(millisecondsDelay);

            Console.WriteLine("    Task.Delay is finished--returning from called method.");
        }
    }
}