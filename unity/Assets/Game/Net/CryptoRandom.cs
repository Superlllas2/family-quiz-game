using System;
using System.Security.Cryptography;
using FamilyQuiz.Core;

namespace FamilyQuiz.Net
{
    /// <summary>Random numbers from the operating system's cryptographic generator (for codes, secrets, tokens).</summary>
    public sealed class CryptoRandom : IRandom, IDisposable
    {
        private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        private readonly byte[] _buffer = new byte[4];
        private readonly object _lock = new object();

        public int Next(int maxExclusive)
        {
            if (maxExclusive <= 0) throw new ArgumentOutOfRangeException(nameof(maxExclusive));
            // Rejection sampling keeps every value equally likely.
            uint limit = uint.MaxValue - (uint.MaxValue % (uint)maxExclusive);
            lock (_lock)
            {
                while (true)
                {
                    _rng.GetBytes(_buffer);
                    uint value = BitConverter.ToUInt32(_buffer, 0);
                    if (value < limit) return (int)(value % (uint)maxExclusive);
                }
            }
        }

        public void Dispose()
        {
            _rng.Dispose();
        }
    }
}
