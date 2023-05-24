using ExampleAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;
using System;

namespace ExampleAPI.Controllers
{
    public class JWT
    {
        public JWT_Header header;
        public JWT_Body body;
        public string signature;
    }
    public class JWT_Header
    {
        public string typ;
        public string alg;
    }
    public class JWT_Body 
    {
        public string username;
        public string role;
        public DateTime expiry;
        public DateTime issued;
    }

    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        static List<User> Users { get; set; } = new List<User>()
        {
            new User() { 
                EMail = "jgrieve@ualberta.ca",
                Password = "Password1",
                Role = "Admin"
            },
            new User() {
                EMail = "johndoe@example.com",
                Password = "password",
                Role = "Moderator"
            },
            new User() {
                EMail = "janedoe@example.com",
                Password = "P@ssW0rD!",
                Role = "User"
            }
        };
        private static string secret = "GmZXA7tptImdxVAhwQOT";

        [HttpPost]
        public IActionResult Post()
        {
            string auth = Request.Headers.Authorization.ToString();
            // The user didn't give us even a username, so we have no idea who they are (hence 401).
            if (string.IsNullOrWhiteSpace(auth)) return StatusCode(401, "No credentials provided with request.");
            // Ensure user is authenticating with Basic (username and password).
            if (auth.Split(' ')[0] != "Basic") return StatusCode(400, "Only Basic authentication is allowed.");
            User found;
            try
            {
                // Convert from base64 back to UTF8.
                string decoded = Base64UrlEncoder.Decode(auth.Split(' ')[1]);
                // Split the username from the password.
                string[] credentials = decoded.Split(' ');
                // Too few or too many arguments.
                if (credentials.Length != 2) return StatusCode(400, "Please provide only the username and password in that order, separated by a space.");
                // Try to find a matching user.
                found = Users.SingleOrDefault(user => user.EMail == credentials[0]);
                // If the user wasn't found.
                if (found == null) return StatusCode(401, "No account with that email address is registered.");
                // If the password is wrong.
                if (found.Password != credentials[1]) return StatusCode(401, "Incorrect password.");
            }
            catch (Exception e)
            {
                return StatusCode(500, "Unknown error occured. "+e.Message);
            }


            JWT jwt = new JWT()
            {
                header = new JWT_Header()
                {
                    alg = "SHA256",
                    typ = "JWT"
                },
                body = new JWT_Body()
                {
                    username = found.EMail,
                    role = found.Role,
                    issued = DateTime.Now,
                    expiry = DateTime.Now.AddDays(1)
                }
            };
            string header64 = Base64UrlEncoder.Encode(JsonConvert.SerializeObject(jwt.header));
            string body64 = Base64UrlEncoder.Encode(JsonConvert.SerializeObject(jwt.body));
            using (HMACSHA256 sha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(header64 + "." + body64));
                jwt.signature = Encoding.UTF8.GetString(hash);
            }
            string sig64 = Base64UrlEncoder.Encode(jwt.signature);
            return Ok($"Bearer {header64}.{body64}.{sig64}");
        }

        [HttpGet]
        public IActionResult Get()
        {
            string auth = Request.Headers.Authorization.ToString();
            // The user didn't give us even a username, so we have no idea who they are (hence 401).
            if (string.IsNullOrWhiteSpace(auth)) return StatusCode(401, "No credentials provided with request.");
            // Ensure user is authenticating with Basic (username and password).
            if (auth.Split(' ')[0] != "Bearer") return StatusCode(400, "Only Bearer authentication is allowed.");
            try
            {
                string[] jwtPieces = auth.Split(' ')[1].Split('.');
                byte[] hash;
                using (HMACSHA256 sha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
                {
                    hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(jwtPieces[0] + "." + jwtPieces[1]+secret));
                }
                string[] decodedPieces = jwtPieces.Select(piece => Base64UrlEncoder.Decode(piece)).ToArray();
                JWT decoded = new JWT()
                {
                    header = (JWT_Header)JsonConvert.DeserializeObject(decodedPieces[0], typeof(JWT_Header)),
                    body = (JWT_Body)JsonConvert.DeserializeObject(decodedPieces[1], typeof(JWT_Body))
                };
                if (Encoding.UTF8.GetString(hash) != decodedPieces[2]) return StatusCode(401, "Failed checksum.");
                if (decoded.body.expiry > DateTime.Now) return StatusCode(403, "JWT expired.");
            }
            catch (Exception e)
            {
                return StatusCode(500, "Unknown error occured. " + e.Message);
            }
            return Ok("JWT valid.");
        }
    }
}
