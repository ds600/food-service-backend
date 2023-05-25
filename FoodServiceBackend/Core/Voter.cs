using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodServiceBackend.Core
{
    public class Voter
    {
        public string Token { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string VoteOptionChosen { get; set; } = string.Empty;
    }
}
