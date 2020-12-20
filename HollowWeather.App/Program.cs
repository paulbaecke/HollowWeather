namespace HollowWeather.App
{
    using System;
    using HollowMan.Core;

    class Program
    {
        static void Main()
        {
            Console.WriteLine("HollowMan Weather");
            Console.WriteLine("Press q to exit");
            using (var weathermanager = new WeatherManager(60, 85, -5.2))
            {
                weathermanager.StartObserving();                
                while (Console.ReadKey().Key != ConsoleKey.Q)
                {
                    weathermanager.Stop();
                }
            }
        }
    }
}
