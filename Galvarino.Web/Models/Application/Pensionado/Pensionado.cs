using DocumentFormat.OpenXml.Office2010.Excel;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.ComponentModel.DataAnnotations;

namespace Galvarino.Web.Models.Application.Pensionado
{
    public class Pensionado
    {
        [Key]
        public int Id { get; set; }
        public string Folio { get; set; }
        public DateTime FechaFormaliza { get; set; }
        public string RutCliente { get; set; }
        public string NombreCliente { get; set; }
        public string NumeroTicket  { get; set; }
        public string TipoPensionado { get; set; }


    }
}