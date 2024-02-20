using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlikonDAO.Tools
{
    public class SetTypes
    {
        Dictionary<string, string> tiposDatos = new()
        {
            {"date", "ISODate"},
            {"double", "Double"},
            {"decimal", "NumberDecimal"},
            {"int", "NumberInt"}
        };
        public string SetType(string tipo, string value)
        {
            string resultado = string.Empty;

            if (tipo == "string")
            {
                resultado = "'" + value + "'";
            }
            else
            {
                string tipoDato = tiposDatos[tipo];
                resultado = tipoDato + "('" + value + "')";
            }
            return resultado;
        }

        public string ArraySetType(string tipo, string[] value, string operador)
        {
            string resultado = string.Empty;
            string ArrayFields = string.Empty;
            Char trimChar = ',';
            if (tipo == "string")
            {
                ArrayFields = String.Join("','", value);
                ArrayFields = ArrayFields.TrimEnd(trimChar);
                resultado = String.Format("{{ {0} : ['{1}'] }}", operador, ArrayFields);
            }
            else
            {
                string tipoDato = tiposDatos[tipo];
                foreach (string field in value)
                {
                    ArrayFields += tipoDato + "('" + field + "'),";
                }
                ArrayFields = ArrayFields.TrimEnd(trimChar);
                resultado = String.Format("{{ {0} : [{1}] }}", operador, ArrayFields);
            }
            return resultado;
        }
    }
}
