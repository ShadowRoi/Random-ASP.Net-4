using System;
using static ASP_Project.Controllers.Countries;
// Βάζουμε όλες τις classes σε άλλο αρχείο για να είναι ποιο οργανωμένα και clean
namespace ASP_Project.Controllers
{
    public class Objectjson
    {
        public Name name { get; set; }
        public List<string> capital { get; set; }
        public List<string> borders { get; set; } 
    }

    public class Name
    {
        public string common { get; set; }
    }

    public class DB_ObjectStructure
    {

        public string name { get; set; }

        public string capital { get; set; }

        public string borders { get; set; }
    }

}

