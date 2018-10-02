using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Web.Data;
using Galvarino.Web.Models.Workflow;


namespace Galvarino.Web.Services.Workflow.Assignment
{
    public class MotorAsignacion : AssignmentBoot
    {
        private string Result;
        public MotorAsignacion(ApplicationDbContext context, string nombreTarea, string numeroTicket) 
            :base(context, numeroTicket)
        {
            switch (nombreTarea)
            {
                case "USUARIO_OF_PROCESA":
                    Result = AsignaUsuarioOfProcesa(numeroTicket);
                    break;
                default:
                    throw new Exception("Error tarea no existe");
            }
        }

        public string AsignaUsuarioOfProcesa(string numeroTicket)
        {
            /*
             Logica:

                1.-Se debe obtener la oficina que procesa la legalizacion del documente
                2.-Se debe obtener al usuario encargado de los documentos de la ofcina de proceso
                3.-Retornar el idnetificador del usuario.
             */

            //_variables[""].Valor 
            return "17042783-1";
        }

        public string GetResult()
        {
            return Result;
        }
    }
}
