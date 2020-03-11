using System.Security.Claims;
using System.Threading.Tasks;
using Galvarino.Web.Models.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class CustomClaimsTransformer : IClaimsTransformation
{
    private readonly UserManager<Usuario> _userManager;
    public CustomClaimsTransformer(UserManager<Usuario> userManager)
    {
        _userManager = userManager;
    }
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var idt = ((ClaimsIdentity)principal.Identity);
        var rut = idt.Name.Replace(@"LAARAUCANA\", "");
        var user = await _userManager.Users
                                .Include(usr => usr.Oficina)
                                .ThenInclude(of => of.Comuna)
                                .ThenInclude(cm => cm.Region)
                                .FirstOrDefaultAsync(d => d.Identificador == rut);
        if(user != null)
        {
            idt.AddClaim(new Claim(CustomClaimTypes.UsuarioNombres, user.Nombres));
            idt.AddClaim(new Claim(CustomClaimTypes.OficinaCodigo, user.Oficina != null ? user.Oficina.Codificacion : ""));
            idt.AddClaim(new Claim(CustomClaimTypes.OficinaDescripcion, user.Oficina != null ? user.Oficina.Nombre : ""));
            idt.AddClaim(new Claim(CustomClaimTypes.UsuarioCorreo, user.NormalizedEmail.ToLower()));
            idt.AddClaim(new Claim(CustomClaimTypes.EsOficinaRM, user.Oficina.EsRM.ToString()));

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var rol in roles)
            {
                if(!idt.HasClaim(ClaimTypes.Role, rol))
                {
                    idt.AddClaim(new Claim(ClaimTypes.Role, rol));
                }
            }
            
        }

        return principal;
    }
}