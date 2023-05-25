using Galvarino.Web.Models.Application.Pensionado;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Galvarino.Web.Data.Repository
{

    public class SolicitudesPensionadoRepository : ISolicitudPensionadoRepository
    {
        private readonly PensionadoDbContext _context;
        public SolicitudesPensionadoRepository(PensionadoDbContext context)
        {
            _context = context;
        }

        public IEnumerable<SolicitudPensionados> ListarSolicitudes(string etapaIn = "")
        {
            var respuesta = new List<SolicitudPensionados>();
            var etapa = _context.Etapas.Where(et => et.NombreInterno == etapaIn).FirstOrDefault();
            var tareas = _context.Tareas.Include(t => t.Solicitudes).Include(e => e.Etapas).Where(s => s.Etapas.Id == etapa.Id).ToList();
            
            foreach (var t in tareas)
            {
                SolicitudPensionados solicitudPensionados = new SolicitudPensionados();
                var pensionado = _context.Pensionado.Where(p => p.NumeroTicket == t.Solicitudes.NumeroTicket).FirstOrDefault();
                var tipo = _context.Tipo.Where(t1 => t1.Id == pensionado.TipoPensionado).FirstOrDefault();
                var expediente = _context.Expedientes.Include(p => p.Pensionado).Where(e => e.Pensionado.Id == pensionado.Id).FirstOrDefault();
                //var documentos = _context.Documentos.Include(cd => cd.ConfiguracionDocumento).Include(e => e.ExpedienteId == expediente.Id).ToList();

                solicitudPensionados.Folio = pensionado.Folio;
                solicitudPensionados.RutCliente = pensionado.RutCliente;
                solicitudPensionados.NombreCliente = pensionado.NombreCliente;
                solicitudPensionados.Tipo = tipo;
                //solicitudPensionados.Documentos = documentos;

                respuesta.Add(solicitudPensionados); ;
            }

            return (respuesta);
        }

        public object listarSolicitudes(string etapaIn)
        {
            throw new NotImplementedException();
        }
    }

    public class SolicitudPensionados
    {
        public string Folio { get; set; }
        public string RutCliente { get; set; }
        public IEnumerable<Documentos> Documentos { get; set; }
        public string NombreCliente { get; set; }
        public Tipo Tipo { get; set; }
    }

}