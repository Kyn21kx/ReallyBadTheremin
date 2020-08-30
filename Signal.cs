using System;
using NAudio.Wave;

namespace ConsoleTheremin {
    public class Signal : WaveStream {

        public enum SignalTypes { Sine, Square, Triangle, Sawtooth };

        public SignalTypes SignalType { private get; set; }

        public double amplitude;
        public double frequency;
        public double time;
        private DirectSoundOut output = null;
        private BlockAlignReductionStream stream = null;


        public Signal(double frequency, double amplitude, SignalTypes signalType) {
            this.time = 0;
            this.frequency = frequency;
            this.amplitude = amplitude;
            this.SignalType = signalType;
        }

        public Signal() {
            this.time = 0;
            this.frequency = 0;
            this.amplitude = 0.1;
            this.SignalType = SignalTypes.Sine;
        }

        public override long Length { get { return long.MaxValue; } }

        public override long Position { get; set; }

        public override WaveFormat WaveFormat { get { return new WaveFormat(44100, 16, 1); } }

        public override int Read(byte[] buffer, int offset, int count) {
            int samples = count / 2;
            for (int i = 0; i < samples; i++) {
                double value = 0;
                double period = 1 / frequency;
                switch (SignalType) {
                    case SignalTypes.Sine:
                        value = amplitude * Math.Sin(Math.PI * 2 * frequency * time);
                        break;
                    case SignalTypes.Square:
                        value = amplitude * (2 * (2 * Math.Floor(frequency * time) - Math.Floor(2 * frequency * time)) + 1);
                        break;
                    case SignalTypes.Triangle:
                        //(4/p) * (t - p/2 Floor( (2t/p) + 1/2)) (-1)^Floor( (2t/p) + 1/2))........ https://en.wikipedia.org/wiki/Triangle_wave
                        double floor = Math.Floor(2 * time / period + 1 / 2);
                        double eq = 4 / period * (time - period / 2 * floor) * Math.Pow(-1, floor);
                        value = amplitude * eq;
                        break;
                    case SignalTypes.Sawtooth:
                        value = amplitude * 2 * (time / period - Math.Floor(1 / 2 + time / period));
                        break;
                }
                time += 1.0 / 44100;
                short truncated = (short)Math.Round(value * (Math.Pow(2, 15) - 1));
                buffer[i * 2] = (byte)(truncated & 0x00ff);
                buffer[i * 2 + 1] = (byte)((truncated & 0xff00) >> 8);
            }

            return count;
        }

        public void Play() {
            try {
                stream = new BlockAlignReductionStream(this);
                output = new DirectSoundOut();
                output.Init(stream);
                output.Play();
            }
            catch (Exception err) {
                Console.WriteLine("Error! " + err.Message);
            }
        }

        public void Stop() {
            if (output != null) output.Stop();
        }

    }
}
