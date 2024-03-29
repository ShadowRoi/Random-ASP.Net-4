
using System.Text.Json; // Χρησιμοποιούμε το library Json για να κάνουμε το Deserialise 
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ASP_Project.Controllers
{
    [Route("api/intergercontroller")] //Βάζουμε το σημείο του API που θα δέχεται το get από τον client
    [ApiController]

    public class Countries : ControllerBase // Κάνουμε inherit από την κλάση ControllerBase 
    {
        [HttpGet] // Δηλώνουμε πως ο server δέχεται HttpGet από το Rest API έτσι ώστε να γνωρίζει πως δεν υπάρχουν arguments από τον client
        public IActionResult Main() // Φτιάχνουμε την main κλάση
        {
            string result = client.GetStringAsync(client.BaseAddress).Result; // Δηλώνουμε ένα variable όπου θα παίρνει σε String από το Address που έχουμε δώσει το JSON από τα countries
            //Για παραπάνω efficiency ώστε να υπάρξει λιγότερο network data που θα στείλουμε στον client θα βγάλουμε τα μη απαραίτητα δεδομένα που έχουμε από το restcountries και θα κρατήσουμε μόνο αυτά που χρειαζόμαστε μέσω του Json Deserialize
            List<Objectjson> test = JsonSerializer.Deserialize<List<Objectjson>>(result); //Φτιάχνουμε μία λίστα όπου θα βάλουμε τα στοιχεία που θα γίνουν Deserialize στην μορφή του Objectjson που έχουμε φτιάξει
            return Ok(test);  // επιστρέφουμε Ok δηλαδή status code 200 ότι η διαδικασία ολοκληρώθηκε επιτυχώς και επιστρέφουμε την λίστα test που έχουμε κάνει Deserialize πίσω στον client 
            
        }

        private static readonly HttpClient client = new(new SocketsHttpHandler // δημιουργούμε ένα καινούργιο HttpClient με τρόπο έτσι ώστε να μπορεί να ξανα χρησημοποιηθεί αντί να ξαναφτιαχτεί από την αρχή για παραπάνω efficiency σε περίπτωση που είναι σαν Transient ή Scoped
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(1) //Βάζουμε τον χρόνο που θα τελειώνει το socket να είναι κάθε ένα λεπτό για να ελέχνει την IP σε περίπτωση που λήξη το προτόκολο TTL του DNS
        })
        {
            BaseAddress = new Uri("https://restcountries.com/v3.1/all?fields=name,capital,borders") // Βάζουμε την διεύθυνση που θα παίρνει το JSON το Http(Μέσα στο Documentation τους δεν βρήκα τρόπο εαν γίνεται να φέρεις μόνο το common name για παραπάνω efficiency)
        };

        public class Objectjson  // Φτιάχνουμε την κλάση όπου περιέχονται τα στοιχεία που χρειαζόμαστε να επιστρέψουμε στον client για να κάνουμε Deserialize από το JSON που μας επιστρέφει το restcountries
        {
            public Name name { get; set; } // παίρνουμε το στοιχείο name και το δηλώνουμε ως Object 
            public List<string> capital { get; set; } // Δηλώνουμε και το capital σαν list παρόλο που περιέχει μόνο ένα string διότι το JSON το επιστρέφει μέσα σε Array
            public List<string> borders { get; set; } // Δηλώνουμε και το borders σαν list διότι περιέχει ένά Array που ανάλογα την χώρα είτε έχει Data ή και όχι
        }
        public class Name //Δηλώνουμε το Name σαν object διότι περιέχει το common name μέσα του όταν έρχεται από το JSON αρχέιο
        {
            public string common { get; set; } // Δηλώνουμε πως μέσα στο Object name περιέχεται το common και το δηλώνουμε σαν string 
        }

    }
}