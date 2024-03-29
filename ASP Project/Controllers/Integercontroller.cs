using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ASP_Project.Controllers
{
    [Route("api/intergercontroller")] //Βάζουμε το σημείο του API που θα δέχεται το post
    [ApiController]
    public class IntegerArray : ControllerBase // Κάνουμε inherit από την κλάση ControllerBase 
    {
        [HttpPost] // Δηλώνουμε πως ο server δέχεται HttpPost έτσι ώστε να δέχεται arguments από τον client και να πρέπει να κάνει return.
        public IActionResult SecondLargestInt([FromBody] RequestObj Obj)
        {
            var integerlist = Obj.RequestArrayObj.ToList(); //Βάζουμε τους αριθμούς σε λίστα για να μπορέσουμε να κάνουμε operations και checks.

               if (integerlist.Count < 2) // Ελένχουμε εαν στην Lista υπάρχουν τουλάχιστον 2 integers για να μπορέσουμε να βρούμε το δεύτερο μεγαλύτερο integer
                {
                    return BadRequest("The array must contain minimum 2 integers"); // Εάν είναι λιγότερο από 2 integers τότε να γυρίσει πίσω στον client πως υπάρχει error
                }

                int secondLargest = SecondLargestInt(integerlist); // Βάζουμε το αποτέλεσμα στο secondLargest το αποτέλεσμα του FindSecondLargest method

            return Ok("The second largest integer is: " + secondLargest); // Γυρίζουμε στον client το αποτέλεσμα 
        }

        private int SecondLargestInt(List<int> integerlist) //Φτιάχνουμε μέθοδο έτσι για να πάρει το δεύτερο μεγαλύτερο integer
        {
            integerlist.Sort(); // Κάνει sort την λίστα από το μεγαλύτερο στο μικρότερο
            return integerlist[integerlist.Count - 2]; // Γυρίζει πίσω το δεύτερο element από την λίστα όπου θα είναι το δεύτερο μεγαλύτερο
        }
    }

    public class RequestObj
    {
        public IEnumerable<int> RequestArrayObj { get; set; } //Δηλώνει τα στοιχεία που θα στέλνει ο client και θα είναι πρέπει να είναι σαν integers αλλιώς θα κάνει error
    }
}