using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieCatalog.Shared.Util
{
    public class GetHttpStatusCode
    {
        private static readonly IDictionary<string, int> StatusCode = new Dictionary<string, int>(){
            {"CREATED", 201},
            {"OK", 200},
            {"OPERATIONAL_ERROR", 400},
            {"UNAUTHORIZED_ERROR", 401},
            {"FORBIDDEN_ERROR", 403},
            {"NOT_FOUND", 404},
            {"INTERNAL_ERROR", 500},
        };
        public static int Get(string status)
        {
            return StatusCode[status];
        }
    }
}
