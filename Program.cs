using System;
using System.IO.Ports;
using System.Windows.Input;
using System.Linq;

namespace ConsoleTheremin {
    class Program {
        #region Variables
        static Signal soundWave;
        static SerialPort port;
        #endregion
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

                if (cntr % 2 == 0) //This is just adjustments on how fast the frequency will change
                    soundWave.frequency = GetNote((int)distance / 10);

                soundWave.amplitude = distance < 100 ? 0.1 : 0;
                cntr++;
            }
        }
        /// <summary>
        /// Sets up the port of the arduino, as well as the signal generator
        /// </summary>
        private static void Initialize () {
            soundWave = new Signal(); //Change this to the other constructor if you want to change any parameters
            //Also, if you want to set up a different type of wave you can just do soundWave.SignalType = [The signal that you want]
            port = new SerialPort();
            port.BaudRate = 9600; //Set up the range of the arduino port
            port.PortName = "COM3"; //Change this to the name of your port with the HC-SR04 sensor
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
            res = Math.Abs(referenceValue - res) <= 1.1 ? referenceValue : res; //This is just adjustments for thresholds
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
            return 440 * (Math.Pow(Math.Pow(2, fact), index - 2)); // Substract and add to the index depending on your tuning (every number you add/substract is a half step)
        }

    }
}
