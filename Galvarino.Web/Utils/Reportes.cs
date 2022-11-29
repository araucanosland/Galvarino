using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Galvarino.Web.Utils
{
    public class Reportes
    {
        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        public IEnumerable<dynamic> ListaReportes(string etapaIn = "",List<Roles> rolesUsuario)
        {
            string fechaCerrar = "";
            int offset = 0;
            int limit = 20;
            string sort = ""; 
            string order = "";
            string notaria = "";

            var rolesUsuario = User.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToArray();
            var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            var ObjetoOficinaUsuario = _context.Oficinas.Include(of => of.OficinaProceso).FirstOrDefault(ofc => ofc.Codificacion == oficinaUsuario);
            var OficinasUsuario = _context.Oficinas.Where(ofc => ofc.OficinaProceso.Id == ObjetoOficinaUsuario.Id && ofc.EsMovil == true);
            var ofinales = new List<Oficina>();
            ofinales.Add(ObjetoOficinaUsuario);
            ofinales.AddRange(OficinasUsuario.ToList());

            var lasOff = ofinales.Select(x => x.Codificacion).ToArray();
            var lasEtps = new string[] { etapaIn };
            string orden = sort + " " + order;
            var salida = _solicitudRepository.listarSolicitudes(rolesUsuario, User.Identity.Name, lasOff, lasEtps, orden, fechaCerrar, notaria);
            var lida = new
            {
                total = salida.Count(),
                rows = salida.Count() < offset ? salida : salida.Skip(offset).Take(limit).ToList()
            };

            return lida;
        }
    }
}
