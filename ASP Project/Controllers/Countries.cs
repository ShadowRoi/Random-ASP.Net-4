/*
Έχω αφαιρέσει τα comments εδώ από τα προηγούμενα projects στα σημεία όπου ο κώδικας είναι ίδιος για να μην ποιάνει πολύ χώρο
Σε αυτό το project θα φτιάξουμε μία DataBase με Dapper implementation καθώς είναι ποιο γρήγορο και efficient από το EF Core
καθώς μπορούμε να συνεχίζουμε να χρησιμοποιούμε Data Objects για να μπορέσουμε να κάνουμε store τα data με ευκολία
Δυστηχώς το Microsoft SQL Server δεν υποστηρίζεται σε mac οπότε θα το τρέξουμε μέσω του Docker 
*/
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient; // Χρησιμοποιούμε αυτό το library για να μπορούμε να συνδέσουμε την SQL
using Dapper;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
/*
 Δεδομένα για την DB. Η βάση δεδομένων ονομάζεται Main και περιέχει ένα table(Countries) και έχει τισ παρακάτω στήλες:
1 countries_id ως Primary key ως Integer με Auto Increment γιατί μπορεί κάποια χώρα να αλλάξει όνομα η κάτι
2 name ως nvchar μέχρι 50 στοιχεία διότι λογικά φτάνουν
3 capital ως nvchar μέχρι 50 στοιχεία για την προτεύουσα
4 borders ως nvchar μέχρι 100 στοιχεία διότι τελικά κάποια χώρα είχε παραπάνω από 50..
H Βάση δεδομένων προς το παρόν έτσι όπως είναι φτιαγμένη τα δεδομένα θα γίνονται duplicate κάθε φορά που δέχεται το HttpGet αλλά αυτό θα το φτιάξουμε στο επόμενο version 
*/

namespace ASP_Project.Controllers
{
    [Route("api/intergercontroller")] 
    [ApiController]

    public class Countries : ControllerBase 
    {

        
        [HttpGet] 
        public IActionResult Main() 
        {
           
            string result = client.GetStringAsync(client.BaseAddress).Result; 
           
            List<Objectjson> DeserializedJSON = JsonSerializer.Deserialize<List<Objectjson>>(result);

            List<DB_ObjectStructure> DB_AllObjects = new List<DB_ObjectStructure>(); // Θα φτιάξουμε μία λίστα όπου θα βάλουμε μέσα όλα τα objects σε μορφή όπως θέλουμε για να μπορούμε να τα βάλουμε στην Βάση δεδομένων

            foreach (var i in DeserializedJSON) // Δεν μπορούσα να σκεφτώ κάτι καλύτερο που να δούλευε, οπότε απλά φτιάξαμε ένα converter που να μετατρέπει το Deserialized JSON αρχείο ως μορφή του DB_ObjectStructure που έχουμε δηλώσει 
            {
                DB_ObjectStructure DB_Object = new DB_ObjectStructure // Για κάθε loop να δημηιουργεί ένα καινούργιο object DB_OBjectStructure 
                {
                    name = i.name.common, 
                    capital = string.Join(",",i.capital), //Βάζουμε τα στοιχεία από το DeserializedJSON ως DB_ObjectStructure και βάζουμε ανάμεσα ένα "," για να βγεί στην μορφή που θέλουμε αλλιώς θα έχει error 
                    borders = string.Join(",",i.borders)
                };

                DB_AllObjects.Add(DB_Object); //Βάζουμε κάθε έτοιμο Object στην λίστα DB_AllObjects 
            }
            
            string addquery = "INSERT INTO Countries (name,capital,borders) VALUES (@name,@capital,@borders)"; // Δηλώνουμε με Dapper implementation τον κώδικα SQL με τρόπο που θέλουμε για να βάλει τα Object στην μορφή που χρειαζόμαστε 
            var connectionstring = "Server=localhost; Database=Main; User Id=SA; Password=Asahilinux1;"; //Τα στοιχεία που χρειάζονται για να συνδεθούμε στην βάση δεδομένων μαζί με το όνομα χρήστη και κωδικό(Είναι ένας τυχαίος κωδικός που σκέφτηκα στην στιγμή)

            using (var db = new SqlConnection(connectionstring)) //Χρησημοποιούμε το using για να αφαιρέσουμε τα πράγματα που δεν χρειάζονται αφού ολοκληρωθούν για καλύτερο efficiency, νομίζω μόνο SQLConnection θα γίνει dispose
            {
                foreach (var i in DB_AllObjects)
                {
                    db.Query(addquery, i); // Κάνουμε μία loop για κάθε αντικείμενο στην λίστα και να στέλνει στην db τα δεδομένα και να τα αποθηκεύει
                }

            }

              return Ok(DB_AllObjects);  //Στέλνουμε πως η διαδικασία ολοκληρώθηκε με επιτυχεία και στέλνουμε πίσω την λίστα DB_AllObjects αντί το DeserializedJSON διότι είναι ποιο οργανωμένο
            
        }
        

        private static readonly HttpClient client = new(new SocketsHttpHandler 
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(1) 
        })
        {
            BaseAddress = new Uri("https://restcountries.com/v3.1/all?fields=name,capital,borders") 
        };

    }
}