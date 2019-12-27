﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinyCsvParser.Mapping;
namespace Galvarino.Web.Models.Mappings
{
    public class CargaInicialIM
    {

        public int Id { get; set; }
        public string FechaCarga { get; set; }
        public string FechaCorresponde { get; set; }
        public string FolioCredito { get; set; }
        public string RutAfiliado { get; set; }
        public string CodigoOficinaIngreso { get; set; }
        public string CodigoOficinaPago { get; set; }
        public string LineaCredito { get; set; }
        public string RutResponsable { get; set; }
        public string CanalVenta { get; set; }
        public string Estado { get; set; }
        public string FechaVigencia { get; set; }
        public string NombreArchivoCarga { get; set; }
        public string SeguroCesantia { get; set; }
        public string Afecto { get; set; }
        public string Aval { get; set; }
    }


    public class CargaInicialMapping : CsvMapping<CargaInicialIM>
    {
        public CargaInicialMapping()
            : base()
        {
            MapProperty(0, x => x.RutAfiliado);
            MapProperty(1, x => x.FolioCredito);
            MapProperty(2, x => x.CodigoOficinaIngreso);
            MapProperty(4, x => x.CodigoOficinaPago);
            MapProperty(6, x => x.Estado);
            MapProperty(7, x => x.LineaCredito);
            MapProperty(8, x => x.RutResponsable);
            MapProperty(9, x => x.CanalVenta);
            MapProperty(10, x => x.FechaCorresponde);
            MapProperty(11, x => x.SeguroCesantia);
            MapProperty(12, x => x.Afecto);
            MapProperty(13, x => x.Aval);

        }





    }

}
