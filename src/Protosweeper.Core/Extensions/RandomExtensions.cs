// Claude prompt:
// Without a seed, .Net uses xoshiro128** under the hood and has done since .Net 6.0; as I only need to generate 32 bit numbers and I want to seed it with 128 bits of data this should be sufficient.
// `Random` is a partial class, with `XoshiroImpl` as an `internal sealed` subclass.
// Using reflection I should be able to set the private variables inside the implementation
// `private uint _s0, _s1, _s2, _s3;` are the variables I want to set, how do I do this with reflection?
//
// Had to add some null handling to get rid of warnings

using System.Reflection;

namespace Protosweeper.Core.Extensions;

public static class RandomExtensions
{
    extension(Random _)
    {
        public static Random CreateSeeded(Guid seed)
        {
            var bytes = seed.ToByteArray();
            var s0 = BitConverter.ToUInt32(bytes, 0);
            var s1 = BitConverter.ToUInt32(bytes, 4);
            var s2 = BitConverter.ToUInt32(bytes, 8);
            var s3 = BitConverter.ToUInt32(bytes, 12);

            var random = new Random();

            // Get the XoshiroImpl instance — Random is a partial class, the actual
            // implementation is stored in _impl
            var implField = typeof(Random).GetField("_impl", BindingFlags.NonPublic | BindingFlags.Instance);
            
            var impl = implField?.GetValue(random);

            var implType = impl?.GetType(); // Xoshiro128StarStarImpl

            implType?.GetField("_s0", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(impl, s0);
            implType?.GetField("_s1", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(impl, s1);
            implType?.GetField("_s2", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(impl, s2);
            implType?.GetField("_s3", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(impl, s3);

            return random;
        }
    }
}