using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Galvarino.Web.Data.Repository.Pensionado
{
    public class CargaInicialRepository : ICargaInicialRepository
    {

        readonly private PensionadoDbContext _context;

        CargaInicialRepository(PensionadoDbContext context)
        {
            _context = context;
        }


        public string CargaAfiliaciones(StringBuilder inserts)
        {
          
        }

        public string CargaDesafialiacion()
        {
            throw new NotImplementedException();
        }
    }
}
