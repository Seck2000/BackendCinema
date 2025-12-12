using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Data;
using AuthService.DTOs;
using AuthService.Models;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Données invalides"
                });
            }

            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Un utilisateur avec cet email existe déjà"
                });
            }

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Prenom = model.Prenom,
                Nom = model.Nom,
                TypeUtilisateur = model.TypeUtilisateur,
                NomEntreprise = model.NomEntreprise,
                DescriptionEntreprise = model.DescriptionEntreprise,
                DateInscription = DateTime.Now,
                EstActif = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = $"Erreur lors de la création du compte: {errors}"
                });
            }

            // Ajouter le rôle
            await _userManager.AddToRoleAsync(user, model.TypeUtilisateur);

            // Générer le token JWT
            var token = GenerateJwtToken(user);

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Inscription réussie",
                Token = token,
                Expiration = DateTime.Now.AddHours(24),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    Prenom = user.Prenom,
                    Nom = user.Nom,
                    TypeUtilisateur = user.TypeUtilisateur,
                    NomEntreprise = user.NomEntreprise
                }
            });
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Données invalides"
                });
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Email ou mot de passe incorrect"
                });
            }

            if (!user.EstActif)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Ce compte a été désactivé"
                });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (!result.Succeeded)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Email ou mot de passe incorrect"
                });
            }

            var token = GenerateJwtToken(user);

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Connexion réussie",
                Token = token,
                Expiration = DateTime.Now.AddHours(24),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    Prenom = user.Prenom,
                    Nom = user.Nom,
                    TypeUtilisateur = user.TypeUtilisateur,
                    NomEntreprise = user.NomEntreprise
                }
            });
        }

        // GET: api/Auth/profile (protégé par JWT)
        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Non authentifié" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Utilisateur non trouvé" });
            }

            return Ok(new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                Prenom = user.Prenom,
                Nom = user.Nom,
                TypeUtilisateur = user.TypeUtilisateur,
                NomEntreprise = user.NomEntreprise
            });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "CineReservSecretKey2025VeryLongAndSecure!";
            var issuer = jwtSettings["Issuer"] ?? "CineReservAPI";
            var audience = jwtSettings["Audience"] ?? "CineReservClient";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, $"{user.Prenom} {user.Nom}"),
                new Claim("TypeUtilisateur", user.TypeUtilisateur),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

