using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlikonDAO.Models
{
    public class QueryOperators
    {
        public List<FieldQueryElements> lstFieldsQuery { get; set; }
        public string LogicOperator { get; set; }
        public List<ItemsSort>? Sort { get; set; }
        public List<ItemFunction>? Funcion { get; set; }
    }
}
