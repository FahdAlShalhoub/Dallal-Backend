using System.Text;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace Dallal_Backend_v2.ThirdParty;

public class FirebaseTokenVerifier
{
    public FirebaseTokenVerifier(string serviceAccountBase64)
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            byte[] data = Convert.FromBase64String(serviceAccountBase64);
            string decodedString = Encoding.UTF8.GetString(data);

            var credential = GoogleCredential
                .FromJson(decodedString);

            FirebaseApp.Create(new AppOptions
            {
                Credential = credential
            });
        }
    }

    public async Task<FirebaseToken> VerifyIdTokenAsync(string idToken)
    {
        try
        {
            FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance
                .VerifyIdTokenAsync(idToken);

            return decodedToken;
        }
        catch (FirebaseAuthException ex)
        {
            // Token is invalid, expired, or malformed
            Console.WriteLine($"Firebase Auth Error: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            // Other errors
            Console.WriteLine($"Error verifying token: {ex.Message}");
            return null;
        }
    }
}