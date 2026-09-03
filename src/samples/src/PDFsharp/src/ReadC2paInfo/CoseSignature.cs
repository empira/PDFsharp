// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Security.Cryptography.X509Certificates;

namespace ReadC2paInfo
{
    /// <summary>
    /// Result of parsing a COSE_Sign1 structure (RFC 9052 §4.2): the signature algorithm and the
    /// certificate chain carried in the protected header's x5chain (COSE header parameter 33).
    /// </summary>
    record CoseSignatureInfo(string Algorithm, List<X509Certificate2> Certificates);

    /// <summary>
    /// Extracts the algorithm and certificate chain from a COSE_Sign1 CBOR structure, as used by
    /// the "c2pa.signature" JUMBF box.
    /// </summary>
    static class CoseSignature
    {
        // IANA COSE Algorithms registry (the ones relevant to C2PA signing).
        static readonly Dictionary<long, string> KnownAlgorithms = new()
        {
            [-7] = "ES256",
            [-35] = "ES384",
            [-36] = "ES512",
            [-37] = "PS256",
            [-38] = "PS384",
            [-39] = "PS512",
            [-257] = "RS256",
            [-258] = "RS384",
            [-259] = "RS512",
            [-8] = "EdDSA",
        };

        public static CoseSignatureInfo Parse(object? decoded)
        {
            // COSE_Sign1 = [protected: bstr, unprotected: map, payload: bstr / nil, signature: bstr]
            // Some encoders wrap this in CBOR tag 18; unwrap it if present.
            var array = decoded switch
            {
                CborTagged { Value: List<object?> arr } => arr,
                List<object?> arr => arr,
                _ => throw new InvalidOperationException("Not a COSE_Sign1 structure (expected a 4-element array)."),
            };

            if (array.Count != 4 || array[0] is not byte[] protectedHeaderBytes)
                throw new InvalidOperationException("Unexpected COSE_Sign1 layout.");

            var protectedHeader = Cbor.Decode(protectedHeaderBytes);

            var algorithm = "unknown";
            if (Cbor.GetMapValueByIntKey(protectedHeader, 1) is long algCode)
                algorithm = KnownAlgorithms.GetValueOrDefault(algCode, $"alg({algCode})");

            var certificates = new List<X509Certificate2>();
            switch (Cbor.GetMapValueByIntKey(protectedHeader, 33))
            {
                case byte[] singleCert:
                    certificates.Add(LoadCertificate(singleCert));
                    break;
                case List<object?> chain:
                    foreach (var item in chain)
                    {
                        if (item is byte[] der)
                            certificates.Add(LoadCertificate(der));
                    }
                    break;
            }

            return new CoseSignatureInfo(algorithm, certificates);
        }

#pragma warning disable SYSLIB0057 // X509Certificate2(byte[]) is obsolete since .NET 9, but still the only ctor available on all our target frameworks.
        static X509Certificate2 LoadCertificate(byte[] der) => new(der);
#pragma warning restore SYSLIB0057
    }
}
