using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient; 
using Dapper;
using Microsoft.Extensions.Caching.Memory;


namespace ASP_Project.Controllers
{
    [Route("api/intergercontroller")]
    [ApiController]

    public class Countries : ControllerBase
    {

        private readonly IMemoryCache memorycache; // Προσθέτουμε το IMemoryCache στο controller που έχουμε μέσω dependency injection
        private readonly string memorycachekey = "countrieskey"; // Βάζουμε ένα κλειδί για να μπορεί ο server να αναγνωρίσει το μέρος του cache που περιέχει τις χώρες

        public Countries(IMemoryCache _memorycache) // φτιάχνουμε το memory cache μέσο του constractor 
        {
            memorycache = _memorycache;
        }


        [HttpGet]
        public IActionResult Main()
        {
            // Ξεκινάμε την υλοποίηση του ERP Diagram, οι ρόμβοι σε κώδικα είναι σαν να μεταφράζονται σε if
            if (memorycache.TryGetValue(memorycachekey, out List<DB_ObjectStructure> CachedObjects)) // Ελέγχουμε εαν στην memorycache υπάρχει το κλειδί για τις χώρες, εαν υπάρχει τότε να δημιουργήσε μία λίστα σε μορφή DB_ObjectStructure και τα τοποθετεί μέσα στην λίστα
            {
                return Ok(CachedObjects); // Στείλε πίσω στον client τα Objects από την cache 
            }

            else // Εαν δεν υπάρχουν στην cache τότε ελέγχουμε στην βάση δεδομένων 
            {

                var connectionstring = "Server=localhost; Database=Main; User Id=SA; Password=Asahilinux1;";
                string getcountries = "SELECT name,capital,borders FROM Countries"; // Φτιάχνουμε σε SQL να πάρουμε τα name,capital,borders εαν υπάρχουν

                var db = new SqlConnection(connectionstring);

                var answer = db.Query<DB_ObjectStructure>(getcountries); // Ζητούμε να μας επιστρέψει τα δεδομένα η βάση δεδομένων σε μορφή DB_OBjectsStructure
                int count = answer.Count(); // ελέγχουμε τις σειρές που υπάρχουν για να ελένξουμε εαν η βάση δεδομένων είναι άδεια 

                if (count != 0) // Εαν δεν είναι άδεια τότε έχει τις χώρες οπότε 
                {       
                    memorycache.Set(memorycachekey, answer); // Βάζουμε τις χώρες που μας επέστρεψε βάση δεδομένων στην cache 
                    return Ok(answer); //Επιστρέφουμε στον client τις χώρες
                }

                else //Εαν η Βάση δεδομένων είναι άδεια τότε
                {
                    List<Objectjson> DeserializedJSON; // Δηλώνουμε μία λίστα ως Objectjson έξω από το eexception handling για να μην είναι local variable 

                    try // Θα τρέξουμε τον παρακάτω κώδικα μέσω try και catch έτσι ώστε να πιάσουμε πιθανά errors που μπορεί να συμβούν
                    {
                        string result = client.GetStringAsync(client.BaseAddress).Result; // Σε περίπτωση που το rest countries δεν είναι διαθέσημο θα υπάρξει error
                        DeserializedJSON = JsonSerializer.Deserialize<List<Objectjson>>(result);// Βάζουμε και το Deserialization για exception handling καθώς υπάρχει το ενδεχόμενο να αλλάξουν τον τρόπο που έρχονται τα δεδομένα μέσω του json και να υπάρξει πρόβλημα
                    }
                    catch (Exception)
                    {
                        return StatusCode(500); // Για οποιοδήποτε πρόβλημα που συμβεί σε αυτή τη διαδικασία τότε επέστρεψε στον client "Server Internal Error"
                    }


                    List<DB_ObjectStructure> DB_AllObjects = new List<DB_ObjectStructure>(); 

                    foreach (var i in DeserializedJSON) 
                    {
                        DB_ObjectStructure DB_Object = new DB_ObjectStructure 
                        {
                            name = i.name.common,
                            capital = string.Join(",", i.capital), 
                            borders = string.Join(",", i.borders)
                        };

                        DB_AllObjects.Add(DB_Object); 
                    }

                    string addquery = "INSERT INTO Countries (name,capital,borders) VALUES (@name,@capital,@borders)"; 
                    {
                        foreach (var i in DB_AllObjects)
                        {
                            db.Query(addquery, i); 
                        }

                    }
                    memorycache.Set(memorycachekey, DB_AllObjects); // Αφού τοποθετηθούν τα δεδομένα στην βάση δεδομένων τότε θα τα βάλουμε και στην cache με βάση του ERP Diagram 
                    return Ok(DB_AllObjects);

                }
            }

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

/* Συμπεράσματα
    Καθώς τα αποτελέσματα που ζητούνται τηρούνται αξίζει να σημειωθεί πως μπορούν να υπάρξουν κάποιες πολύ σημαντικές βελτιώσεις.
Αρχικά η εφαρμογή στην συγκεκριμένη κατάσταση ΔΕΝ είναι scalable διότι δεν μπορούμε να χρησημοποιήσουμε την βάση δεδομένων για άλλα πράξεις,
καθώς το Primary key δεν αντιστοιχεί σε μία χώρα. Θεωρητικά με τον συγκεκριμένο τρόπο λαμβάνουμε πληροφορίες όπως name,capital και borders όπου όλα αυτά τα δεδομένα μπορεί να αλλάξουν.
Αυτό που θα πρέπει να κάνουμε είναι να ζητάμε από το rest countries να μας φέρνει για κάθε χώρα ένα δεδομένο όπως πχ serial number όπου θα μπορούμε να το χρησημοποιήσουμε ως Primary key.
Έτσι με αυτόν τον τρόπο το serial number δεν θα αλλάξει ποτέ και θα μπορούμε να το χρησημοποιήσουμε σωστά και να μπορούμε να ενημερώνουμε την βάση δεδομένων σωστά και ασφαλές.

    Όσο αφορά το memory caching αναλόγως την αφαρμογή και τις πράξεις που κάνουμε θα πρέπει να την ρυθμίσουμε για το πόση ώρα θα πρέπει να κρατήσει τα δεδομένα στην cache.
Διότι εαν δεν χρειάζεται συχνά να στέλνει τα συγκεκριμένα δεδομένα τότε μπορούμε να την ρυθμήσουμε έτσι ώστε να αδειάζει την memory από τον server μετά από κάποιο χρονικό διάστημα.

    Επίσης πιθανών να είναι καλύτερα και ποιο efficient αντί να πέρνουμε τα δεδομένα με GetStringASync να χρησιμοποιούσαμε το GetFromJsonAsync καθώς με μία μέθοδο θα το έκανε και deserialise.


*/