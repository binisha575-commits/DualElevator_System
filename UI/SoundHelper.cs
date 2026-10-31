using System;
using System.IO;
using System.Media;

namespace Lift.UI
{
    /// <summary>
    /// Generates and plays elevator audio chimes, motor travel sounds, and arrival bells in memory
    /// using standard WAV PCM synthesis and System.Media.SoundPlayer.
    /// Audible on all laptop and desktop speaker configurations.
    /// </summary>
    public static class SoundHelper
    {
        private static SoundPlayer? _arrivalPlayer;
        private static SoundPlayer? _travelPlayer;
        private static SoundPlayer? _clickPlayer;
        private static SoundPlayer? _transitPlayer;
        private static SoundPlayer? _goingUpPlayer;
        private static SoundPlayer? _goingDownPlayer;
        private static SoundPlayer? _sosAlarmPlayer;
        private static int _movingCount = 0;
        private static readonly object _lock = new object();

        static SoundHelper()
        {
            try
            {
                _arrivalPlayer = CreatePlayer(GenerateArrivalBell());
                _travelPlayer = CreatePlayer(GenerateAudibleTravelSound());
                _clickPlayer = CreatePlayer(GenerateButtonClick());
                _transitPlayer = CreatePlayer(GenerateTransitTone());
                _goingUpPlayer = CreatePlayer(GenerateGoingUpTone());
                _goingDownPlayer = CreatePlayer(GenerateGoingDownTone());
                _sosAlarmPlayer = CreatePlayer(GenerateSosAlarm());
            }
            catch
            {
                // Graceful fallback if audio device is unavailable
            }
        }

        public static void PlayChime()
        {
            try { _arrivalPlayer?.Play(); } catch { }
        }

        public static void PlayButtonClick()
        {
            try { _clickPlayer?.Play(); } catch { }
        }

        public static void PlayTransitTone()
        {
            try { _transitPlayer?.Play(); } catch { }
        }

        public static void PlayGoingUp()
        {
            try { _goingUpPlayer?.Play(); } catch { }
        }

        public static void PlayGoingDown()
        {
            try { _goingDownPlayer?.Play(); } catch { }
        }

        public static void PlaySosAlarm()
        {
            try { _sosAlarmPlayer?.Play(); } catch { }
        }

        public static void StartTravelSound()
        {
            lock (_lock)
            {
                _movingCount++;
                if (_movingCount == 1)
                {
                    try { _travelPlayer?.PlayLooping(); } catch { }
                }
            }
        }

        public static void StopTravelSound()
        {
            lock (_lock)
            {
                _movingCount = Math.Max(0, _movingCount - 1);
                if (_movingCount == 0)
                {
                    try { _travelPlayer?.Stop(); } catch { }
                }
            }
        }

        private static SoundPlayer CreatePlayer(byte[] wavData)
        {
            var ms = new MemoryStream(wavData);
            var player = new SoundPlayer(ms);
            player.Load();
            return player;
        }

        /// <summary>
        /// Crisp two-tone harmonic arrival chime (A5 880Hz + E6 1320Hz + high harmonic).
        /// High volume and bright decay, clearly audible on all speakers.
        /// </summary>
        private static byte[] GenerateArrivalBell()
        {
            int sampleRate = 44100;
            double durationSec = 1.0;
            int numSamples = (int)(sampleRate * durationSec);
            short[] samples = new short[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                double t = (double)i / sampleRate;
                double decay = Math.Exp(-3.2 * t);
                // 880Hz primary + 1320Hz overtone + 1760Hz shimmer
                double tone = Math.Sin(2.0 * Math.PI * 880.0 * t) * 0.65
                            + Math.Sin(2.0 * Math.PI * 1320.0 * t) * 0.35
                            + Math.Sin(2.0 * Math.PI * 1760.0 * t) * 0.15;
                double val = tone * decay * 24000.0;
                samples[i] = (short)Math.Clamp(val, short.MinValue, short.MaxValue);
            }

            return BuildWav(samples, sampleRate);
        }

        /// <summary>
        /// Highly audible elevator travel sound:
        /// Rich mechanical movement harmonics (440Hz, 554Hz, 660Hz) with 3.5Hz rhythmic cable cadence.
        /// Perfectly designed for laptop and desktop speaker frequency response.
        /// </summary>
        private static byte[] GenerateAudibleTravelSound()
        {
            int sampleRate = 44100;
            double durationSec = 1.6;
            int numSamples = (int)(sampleRate * durationSec);
            short[] samples = new short[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                double t = (double)i / sampleRate;
                // Rhythmic amplitude modulation representing cable/pulley rotation (3.5 Hz)
                double modulation = 0.75 + 0.25 * Math.Sin(2.0 * Math.PI * 3.5 * t);

                // Motor drone with clear audible mid-frequencies (440Hz A4, 554Hz C#5, 660Hz E5)
                double tone = (Math.Sin(2.0 * Math.PI * 440.0 * t) * 0.50
                             + Math.Sin(2.0 * Math.PI * 554.37 * t) * 0.30
                             + Math.Sin(2.0 * Math.PI * 660.0 * t) * 0.20) * modulation;

                double val = tone * 22000.0;
                samples[i] = (short)Math.Clamp(val, short.MinValue, short.MaxValue);
            }

            return BuildWav(samples, sampleRate);
        }

        /// <summary>
        /// Short tactile click sound for button presses.
        /// </summary>
        private static byte[] GenerateButtonClick()
        {
            int sampleRate = 44100;
            double durationSec = 0.05;
            int numSamples = (int)(sampleRate * durationSec);
            short[] samples = new short[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                double t = (double)i / sampleRate;
                double decay = Math.Exp(-55.0 * t);
                double val = Math.Sin(2.0 * Math.PI * 980.0 * t) * decay * 22000.0;
                samples[i] = (short)Math.Clamp(val, short.MinValue, short.MaxValue);
            }

            return BuildWav(samples, sampleRate);
        }

        /// <summary>
        /// Transit pulse sound played during floor transition.
        /// </summary>
        private static byte[] GenerateTransitTone()
        {
            int sampleRate = 44100;
            double durationSec = 0.25;
            int numSamples = (int)(sampleRate * durationSec);
            short[] samples = new short[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                double t = (double)i / sampleRate;
                double decay = Math.Exp(-8.0 * t);
                double val = Math.Sin(2.0 * Math.PI * 659.25 * t) * decay * 20000.0;
                samples[i] = (short)Math.Clamp(val, short.MinValue, short.MaxValue);
            }

            return BuildWav(samples, sampleRate);
        }

        /// <summary>
        /// Rising two-tone chime indicating upward travel.
        /// </summary>
        private static byte[] GenerateGoingUpTone()
        {
            int sampleRate = 44100;
            double durationSec = 0.4;
            int numSamples = (int)(sampleRate * durationSec);
            short[] samples = new short[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                double t = (double)i / sampleRate;
                double decay = Math.Exp(-4.0 * t);
                // Rising pitch: 600Hz → 900Hz
                double freq = 600.0 + 750.0 * t;
                double val = Math.Sin(2.0 * Math.PI * freq * t) * decay * 20000.0;
                samples[i] = (short)Math.Clamp(val, short.MinValue, short.MaxValue);
            }

            return BuildWav(samples, sampleRate);
        }

        /// <summary>
        /// Falling two-tone chime indicating downward travel.
        /// </summary>
        private static byte[] GenerateGoingDownTone()
        {
            int sampleRate = 44100;
            double durationSec = 0.4;
            int numSamples = (int)(sampleRate * durationSec);
            short[] samples = new short[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                double t = (double)i / sampleRate;
                double decay = Math.Exp(-4.0 * t);
                // Falling pitch: 900Hz → 600Hz
                double freq = 900.0 - 750.0 * t;
                double val = Math.Sin(2.0 * Math.PI * freq * t) * decay * 20000.0;
                samples[i] = (short)Math.Clamp(val, short.MinValue, short.MaxValue);
            }

            return BuildWav(samples, sampleRate);
        }

        /// <summary>
        /// Pulsing emergency SOS alarm tone.
        /// </summary>
        private static byte[] GenerateSosAlarm()
        {
            int sampleRate = 44100;
            double durationSec = 1.5;
            int numSamples = (int)(sampleRate * durationSec);
            short[] samples = new short[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                double t = (double)i / sampleRate;
                // Rapid pulsing at 6Hz
                double pulse = (Math.Sin(2.0 * Math.PI * 6.0 * t) > 0) ? 1.0 : 0.3;
                // Alternating alarm frequencies: 880Hz and 1200Hz
                double freq = (Math.Sin(2.0 * Math.PI * 3.0 * t) > 0) ? 880.0 : 1200.0;
                double val = Math.Sin(2.0 * Math.PI * freq * t) * pulse * 22000.0;
                samples[i] = (short)Math.Clamp(val, short.MinValue, short.MaxValue);
            }

            return BuildWav(samples, sampleRate);
        }

        private static byte[] BuildWav(short[] samples, int sampleRate)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            int subchunk2Size = samples.Length * sizeof(short);
            int chunkSize = 36 + subchunk2Size;

            // RIFF header
            bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(chunkSize);
            bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

            // fmt subchunk
            bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16); // Subchunk1Size (16 for PCM)
            bw.Write((short)1); // AudioFormat PCM = 1
            bw.Write((short)1); // NumChannels = 1 (Mono)
            bw.Write(sampleRate);
            bw.Write(sampleRate * sizeof(short)); // ByteRate
            bw.Write((short)sizeof(short)); // BlockAlign
            bw.Write((short)16); // BitsPerSample

            // data subchunk
            bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            bw.Write(subchunk2Size);
            foreach (var s in samples)
            {
                bw.Write(s);
            }

            return ms.ToArray();
        }
    }
}
