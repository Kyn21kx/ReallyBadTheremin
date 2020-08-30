using System;
using System.IO.Ports;
using System.Windows.Input;
using System.Linq;

namespace ConsoleTheremin {
    class Program {
        static Signal soundWave;
        static SerialPort port;
        static void Main(string[] args) {
            Initialize();
            Console.WriteLine("Press any key to start the theremin");
            Console.ReadKey();
            soundWave.Play();
            double distance = 0;
            long cntr = 0;
            while (true) {
                Console.WriteLine("Current frequency: " + soundWave.frequency);
                distance = GetDistance(distance);
                if (cntr % 2 == 0)
                    soundWave.frequency = GetNote((int)distance / 10);

                if (Console.KeyAvailable) {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key) {
                        case ConsoleKey.Spacebar:
                            Console.WriteLine("Space bar pressed");
                            break;
                        default:
                            Console.WriteLine("I guess no space bars");
                            break;
                    }
                }
                else
                    Console.WriteLine("Key up");
                
                soundWave.amplitude = distance < 100 ? 0.1 : 0;
                cntr++;
            }
        }

        private static void Initialize () {
            soundWave = new Signal();
            port = new SerialPort();
            port.BaudRate = 9600;
            port.PortName = "COM3";
            port.Open();
        }

        private static double GetDistance(double referenceValue) {
            string s = port.ReadLine();
            double res;
            try {
                res = Convert.ToDouble(s);
            }
            catch (Exception err) {
                Console.WriteLine("Error! " + err.Message);
                res = 0;
            }
            Console.WriteLine("Distance: " + res);
            res = Math.Abs(referenceValue - res) <= 1.1 ? referenceValue : res;
            return res;
        }
        /// <summary>
        /// Gets the note based on the fn = f0 * (a)^n equation
        /// </summary>
        /// <param name="index">Index of the note to get (condidering 0 as A4 at 440 HZ)</param>
        /// <returns></returns>
        private static double GetNote(int index) {
            //fn = f0 * (a)^n
            const double fact = 1.0 / 12.0;
            return 440 * (Math.Pow(Math.Pow(2, fact), index - 2));
        }

    }
}
