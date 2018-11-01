using System.Security.Claims;
using System.Threading.Tasks;
using Galvarino.Web.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

public class GalvarinoClaimsPrincipalFactory : UserClaimsPrincipalFactory<Usuario, Rol>
{
    public GalvarinoClaimsPrincipalFactory(
        UserManager<Usuario> userManager,
        RoleManager<Rol> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {

    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(Usuario user)
    {
        //user = await UserManager.Users.Include(usr => usr.Oficina).FirstOrDefaultAsync(d => d.Id == user.Id);
        var identity = await base.GenerateClaimsAsync(user);
        //identity.AddClaim(new Claim("Oficina", user.Oficina != null ? user.Oficina.Codificacion : ""));
        //identity.AddClaim(new Claim("Nombres", user.Nombres ?? ""));
        //identity.AddClaim(new Claim("Correo", user.NormalizedEmail ?? ""));
        identity.AddClaim(new Claim(CustomClaimTypes.TipoAcceso, "login"));
        return identity;
    }

    /*public async override Task<ClaimsPrincipal> CreateAsync(Usuario user)
    {
        var principal = await base.CreateAsync(user);

        // Add your claims here
        ((ClaimsIdentity)principal.Identity).AddClaims(new[] { 
            new Claim(ClaimTypes.Email, user.Email),
            //new Claim("Oficina", user.Oficina.Codificacion ?? ""),
            new Claim("Nombres", user.Nombres ?? ""),
            new Claim("Correo", user.NormalizedEmail ?? "")
        });
        return principal;
    }*/
}