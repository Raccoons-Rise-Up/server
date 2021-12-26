using System;
using System.IO;
using System.Text.Json;
using System.Security.Cryptography;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace GameServer.Utilities
{
    public class JsonWebToken
    {
        public TokenPayload Payload { get; private set; }
        public TokenValidateResponse IsValid { get; private set; }

        private readonly string publicRsaKey; // RS256
        private readonly string token;

        public JsonWebToken(string token)
        {
            this.token = token;
            publicRsaKey = File.ReadAllText("./public.key");

            var decoded = DecodeToken();

            if (decoded != "")
            {
                Payload = JsonSerializer.Deserialize<TokenPayload>(decoded);
                IsValid = Validate();
            }
            else 
            {
                IsValid = new TokenValidateResponse
                {
                    Error = TokenValidateError.Malformed,
                    Message = "The JWT is malformed"
                };
            }
        }

        private TokenValidateResponse Validate()
        {
            // Did the token expire?
            if ((DateTimeOffset.Now.ToUnixTimeSeconds() - Payload.exp) >= 0)
                return new TokenValidateResponse 
                {
                    Error = TokenValidateError.Expired,
                    Message = "The JWT reached its expiration date"
                };

            // Check iss, aud and sub
            if (Payload.iss != "Raccoons Rise Up")
                return new TokenValidateResponse 
                {
                    Error = TokenValidateError.InvalidIssuer,
                    Message = "The JWT issuer did not match up"
                };
            if (Payload.aud != "localhost:4000")
                return new TokenValidateResponse
                {
                    Error = TokenValidateError.InvalidAudience,
                    Message = "The JWT audience did not match up"
                };
            if (Payload.sub != "Game Client")
                return new TokenValidateResponse 
                {
                    Error = TokenValidateError.InvalidSub,
                    Message = "The JWT sub did not match up"
                };

            return new TokenValidateResponse 
            {
                Error = TokenValidateError.Ok,
                Message = "The token is valid"
            };
        }

        private string DecodeToken()
        {
            RSAParameters rsaParams;

            using (var tr = new StringReader(publicRsaKey))
            {
                var pemReader = new PemReader(tr);
                if (pemReader.ReadObject() is not RsaKeyParameters publicKeyParams)
                {
                    throw new Exception("Could not read RSA public key");
                }
                rsaParams = DotNetUtilities.ToRSAParameters(publicKeyParams);
            }
            using RSACryptoServiceProvider rsa = new();
            rsa.ImportParameters(rsaParams);

            try
            {
                // This will throw if the signature is invalid
                return Jose.JWT.Decode(token, rsa, Jose.JwsAlgorithm.RS256);
            }
            catch (JsonException)
            {
                return "";
            }
        }

#pragma warning disable IDE1006 // Naming Styles
        public struct TokenPayload
        {
            public string username { get; set; }
            public string password { get; set; }
            public int iat { get; set; }
            public int exp { get; set; }
            public string aud { get; set; }
            public string iss { get; set; }
            public string sub { get; set; }
        }
#pragma warning restore IDE1006 // Naming Styles

        public struct TokenValidateResponse 
        {
            public TokenValidateError Error { get; set; }
            public string Message { get; set; }
        }

        public enum TokenValidateError 
        {
            Ok,
            Malformed,
            Expired,
            InvalidIssuer,
            InvalidAudience,
            InvalidSub
        }
    }
}
