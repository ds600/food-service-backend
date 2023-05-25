using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodServiceBackend.Core
{
    public class VoteOptions
    {
        public List<string> Options { get; set; } = new List<string>();
        public string LastVoteWinner { get; set; } = string.Empty;
    }
}
