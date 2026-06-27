// -----------------------------------------------------------------------------
// This file was taken from:
//   https://github.com/kmaragon/Konscious.Security.Cryptography
//
// Original Author: Keef Aragon
// License: MIT
// -----------------------------------------------------------------------------

namespace Konscious.Security.Cryptography
{
    internal interface IArgon2PseudoRands
    {
        ulong PseudoRand(int segment, int prevLane, int prevOffset);
    }
}